using Artemis.Plugins.Games.EliteDangerous.DataModels;
using Artemis.Plugins.Games.EliteDangerous.Utils;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Artemis.Plugins.Games.EliteDangerous.Journal
{
    internal class JournalParser : FileReaderBase
    {
        private const string JournalFileFilter = "Journal.*.log";
        private static readonly Regex OldJournalFileName = new(
            @"^Journal\.(?<timestamp>\d{12})\.(?<part>\d+)\.log$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        private static readonly Regex NewJournalFileName = new(
            @"^Journal\.(?<timestamp>\d{4}-\d{2}-\d{2}T\d{6})\.(?<part>\d+)\.log$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private readonly string dataDirectory;
        private readonly object journalLock = new();
        private FileSystemWatcher journalFileWatcher;
        private JournalFile currentJournal;
        private JournalFile pendingJournal;
        private bool active;
        private bool disposed;

        public JournalParser(string dataDirectory) : base(true)
        {
            this.dataDirectory = dataDirectory;

            // Create file system watcher to watch for when new journal files are created.
            journalFileWatcher = new FileSystemWatcher(dataDirectory, JournalFileFilter);
            journalFileWatcher.Created += JournalFileWatcher_Created;
        }

        /// <summary>
        /// Returns the filename of the newest journal log in the given journal directory.
        /// </summary>
        internal static JournalFile FindLatestJournal(string directory)
        {
            return Directory
                .EnumerateFiles(directory, JournalFileFilter)
                .Select(TryCreateJournalFile)
                .Where(journal => journal != null)
                .OrderByDescending(journal => journal.Timestamp)
                .ThenByDescending(journal => journal.Part)
                .FirstOrDefault();
        }

        internal static JournalFile TryCreateJournalFile(string path)
        {
            var fileName = Path.GetFileName(path);
            var match = NewJournalFileName.Match(fileName);
            var timestampFormat = "yyyy-MM-dd'T'HHmmss";

            if (!match.Success)
            {
                match = OldJournalFileName.Match(fileName);
                timestampFormat = "yyyyMMddHHmmss";
            }

            if (!match.Success)
                return null;

            var timestampText = match.Groups["timestamp"].Value;
            if (timestampFormat == "yyyyMMddHHmmss")
                timestampText = "20" + timestampText;

            if (!DateTime.TryParseExact(timestampText, timestampFormat,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp) ||
                !int.TryParse(match.Groups["part"].Value, NumberStyles.None,
                    CultureInfo.InvariantCulture, out var part))
                return null;

            return new JournalFile(path, timestamp, part);
        }

        /// <summary>
        /// When the module is activated, begin reading the latest journal file.
        /// </summary>
        public override void Activate()
        {
            lock (journalLock)
            {
                ObjectDisposedException.ThrowIf(disposed, this);
                active = true;
                journalFileWatcher.EnableRaisingEvents = true;
                RegisterPendingJournal(FindLatestJournal(dataDirectory));
            }

            TryOpenPendingJournal();
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            lock (journalLock)
            {
                if (disposed)
                    return;

                active = false;
                journalFileWatcher.EnableRaisingEvents = false;
                currentJournal = null;
                pendingJournal = null;
                base.Deactivate();
            }
        }

        public override void PerformUpdate(EliteDangerousDataModel dataModel)
        {
            TryOpenPendingJournal();
            base.PerformUpdate(dataModel);
        }

        /// <summary>
        /// Parses a single journal line and if it is a known event applies it to the datamodel.
        /// </summary>
        protected override void OnContentRead(EliteDangerousDataModel dataModel, string line)
        {
            var @event = JsonConvert.DeserializeObject<IJournalEvent>(line, JournalEvent.JournalEventSettings);
            @event?.ApplyUpdate(dataModel);
        }

        /// <summary>
        /// Handles a newly created journal file by opening it if it is newer than the current file.
        /// </summary>
        private void JournalFileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            HandleJournalCreated(e.FullPath);
        }

        internal void HandleJournalCreated(string path)
        {
            RegisterPendingJournal(TryCreateJournalFile(path));
            TryOpenPendingJournal();
        }

        private void RegisterPendingJournal(JournalFile journal)
        {
            if (journal == null)
                return;

            lock (journalLock)
            {
                if (!active || disposed)
                    return;

                if (currentJournal != null && journal.CompareTo(currentJournal) <= 0)
                    return;
                if (pendingJournal != null && journal.CompareTo(pendingJournal) <= 0)
                    return;

                pendingJournal = journal;
            }
        }

        private void TryOpenPendingJournal()
        {
            lock (journalLock)
            {
                if (!active || disposed || pendingJournal == null)
                    return;

                try
                {
                    OpenFile(pendingJournal.Path);
                    currentJournal = pendingJournal;
                    pendingJournal = null;
                }
                catch (IOException)
                {
                    // The game may still hold the newly created journal exclusively.
                    // Keep it pending and try again during the next update.
                }
                catch (UnauthorizedAccessException)
                {
                    // Access may become available later; do not abandon the journal.
                }
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            lock (journalLock)
            {
                if (disposed)
                    return;

                active = false;
                disposed = true;
                journalFileWatcher.EnableRaisingEvents = false;
                currentJournal = null;
                pendingJournal = null;
                base.Dispose();
            }

            journalFileWatcher.Dispose();
        }
    }

    internal sealed record JournalFile(string Path, DateTime Timestamp, int Part) : IComparable<JournalFile>
    {
        public int CompareTo(JournalFile other)
        {
            if (other == null) return 1;
            var timestampComparison = Timestamp.CompareTo(other.Timestamp);
            return timestampComparison != 0 ? timestampComparison : Part.CompareTo(other.Part);
        }
    }
}
