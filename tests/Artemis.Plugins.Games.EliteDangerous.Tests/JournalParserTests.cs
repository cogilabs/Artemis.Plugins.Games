using Artemis.Plugins.Games.EliteDangerous.Journal;
using Artemis.Plugins.Games.EliteDangerous.DataModels;
using System;
using System.IO;
using System.Threading;
using Xunit;

namespace Artemis.Plugins.Games.EliteDangerous.Tests;

public sealed class JournalParserTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), $"artemis-elite-tests-{Guid.NewGuid():N}");

    public JournalParserTests() => Directory.CreateDirectory(directory);

    [Fact]
    public void SelectsNewFormatByParsedTimestampInsteadOfLexicographicalOrder()
    {
        Create("Journal.220314184740.01.log");
        var expected = Create("Journal.2022-03-15T182502.01.log");

        Assert.Equal(expected, JournalParser.FindLatestJournal(directory)?.Path);
    }

    [Fact]
    public void SelectsNewestOldFormatJournal()
    {
        Create("Journal.210101000000.01.log");
        var expected = Create("Journal.220314184740.01.log");

        Assert.Equal(expected, JournalParser.FindLatestJournal(directory)?.Path);
    }

    [Fact]
    public void SelectsNewestNewFormatJournal()
    {
        Create("Journal.2025-12-31T235959.01.log");
        var expected = Create("Journal.2026-01-01T000000.01.log");

        Assert.Equal(expected, JournalParser.FindLatestJournal(directory)?.Path);
    }

    [Fact]
    public void ReturnsNullWhenNoJournalsExist()
    {
        Assert.Null(JournalParser.FindLatestJournal(directory));
    }

    [Fact]
    public void IgnoresInvalidAlphaAndBetaJournalNames()
    {
        Create("JournalAlpha.2026-08-30T120000.01.log");
        Create("JournalBeta.260830120000.01.log");
        Create("Journal.unrelated.log");

        Assert.Null(JournalParser.FindLatestJournal(directory));
    }

    [Fact]
    public void UsesNumericPartAsTieBreaker()
    {
        Create("Journal.2026-08-30T120000.02.log");
        var expected = Create("Journal.2026-08-30T120000.10.log");

        Assert.Equal(expected, JournalParser.FindLatestJournal(directory)?.Path);
    }

    [Fact]
    public void RetriesPendingJournalOnUpdateUntilItCanBeOpened()
    {
        var path = Create("Journal.2026-08-30T120000.01.log");
        using var exclusiveStream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        using var parser = new JournalParser(directory);

        parser.Activate();
        Assert.False(parser.IsOpen);

        parser.PerformUpdate(new EliteDangerousDataModel());
        Assert.False(parser.IsOpen);

        exclusiveStream.Dispose();
        parser.PerformUpdate(new EliteDangerousDataModel());

        Assert.True(parser.IsOpen);
    }

    [Fact]
    public void SwitchesToNewJournalWhenItIsCreated()
    {
        Create("Journal.2026-08-30T120000.01.log", """{"event":"Loadout","Ship":"asp","ShipName":"FIRST"}""");
        var model = new EliteDangerousDataModel();
        using var parser = new JournalParser(directory);

        parser.Activate();
        parser.PerformUpdate(model);
        Assert.Equal("FIRST", model.Ship.Name);

        Create("Journal.2026-08-30T130000.01.log", """{"event":"Loadout","Ship":"asp","ShipName":"SECOND"}""");

        var switched = SpinWait.SpinUntil(() =>
        {
            parser.PerformUpdate(model);
            return model.Ship.Name == "SECOND";
        }, TimeSpan.FromSeconds(2));

        Assert.True(switched, "The parser did not switch to the newly created journal.");
    }

    [Fact]
    public void QueuedCreatedCallbackAfterDisposeDoesNotReopenParser()
    {
        Create("Journal.2026-08-30T120000.01.log");
        var parser = new JournalParser(directory);
        parser.Activate();
        Assert.True(parser.IsOpen);

        parser.Dispose();
        var newerJournal = Create("Journal.2026-08-30T130000.01.log");

        parser.HandleJournalCreated(newerJournal);

        Assert.False(parser.IsOpen);
    }

    private string Create(string fileName, string content = "")
    {
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    public void Dispose() => Directory.Delete(directory, true);
}
