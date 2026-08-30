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
        private FileSystemWatcher journalFileWatcher;

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
            journalFileWatcher.EnableRaisingEvents = true;
            var latestJournal = FindLatestJournal(dataDirectory);
            if (latestJournal != null)
                OpenFile(latestJournal.Path);
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            journalFileWatcher.EnableRaisingEvents = false;
            base.Deactivate();
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
        /// Handles when a new journal file is created by opening the newest file.
        /// </summary>
        private void JournalFileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            var latestJournal = FindLatestJournal(dataDirectory);
            if (latestJournal != null)
                OpenFile(latestJournal.Path);
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            journalFileWatcher.Dispose();
            journalFileWatcher = null;
            base.Dispose();
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
