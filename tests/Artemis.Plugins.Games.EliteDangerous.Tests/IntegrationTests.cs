using Artemis.Plugins.Games.EliteDangerous.DataModels;
using Artemis.Plugins.Games.EliteDangerous.Journal;
using Artemis.Plugins.Games.EliteDangerous.Status;
using System;
using System.IO;
using Xunit;

namespace Artemis.Plugins.Games.EliteDangerous.Tests;

public sealed class IntegrationTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), $"artemis-elite-tests-{Guid.NewGuid():N}");

    public IntegrationTests() => Directory.CreateDirectory(directory);

    [Fact]
    public void JournalFighterLifecycleUpdatesDeploymentState()
    {
        WriteJournal("""
                     {"event":"LaunchFighter","ID":12,"PlayerControlled":true}
                     {"event":"FighterDestroyed","ID":12}
                     """);
        var model = new EliteDangerousDataModel();
        using var parser = new JournalParser(directory);

        parser.Activate();
        parser.PerformUpdate(model);

        Assert.False(model.Ship.Fighter.IsDeployed);
    }

    [Fact]
    public void DisembarkJournalAndStatusFilePopulateOnFootModel()
    {
        WriteJournal("""{"event":"Disembark","Taxi":false,"Multicrew":false,"ID":42,"OnStation":false,"OnPlanet":true,"StarSystem":"Sol","Body":"Earth"}""");
        File.WriteAllText(Path.Combine(directory, "Status.json"),
            """{"Flags2":32785,"Oxygen":0.75,"Health":0.9,"Temperature":287.0,"Gravity":1.0,"BodyName":"Earth"}""");
        var model = new EliteDangerousDataModel();
        using var journalParser = new JournalParser(directory);
        using var statusParser = new StatusParser(directory);

        journalParser.Activate();
        statusParser.Activate();
        journalParser.PerformUpdate(model);
        statusParser.PerformUpdate(model);

        Assert.Equal(1, model.Player.OnFoot.Disembark.TriggerCount);
        Assert.True(model.Player.OnFoot.IsOnFoot);
        Assert.True(model.Player.OnFoot.IsOnPlanet);
        Assert.True(model.Player.OnFoot.IsExterior);
        Assert.Equal(Vehicle.OnFoot, model.Player.CurrentlyPiloting);
        Assert.Equal(0.75f, model.Player.OnFoot.Oxygen);
        Assert.Equal("Earth", model.Player.OnFoot.BodyName);
    }

    private void WriteJournal(string content) =>
        File.WriteAllText(Path.Combine(directory, "Journal.2026-09-02T180000.01.log"), content);

    public void Dispose() => Directory.Delete(directory, true);
}
