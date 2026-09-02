using Artemis.Plugins.Games.EliteDangerous.DataModels;
using Artemis.Plugins.Games.EliteDangerous.Journal;
using Newtonsoft.Json;
using Xunit;

namespace Artemis.Plugins.Games.EliteDangerous.Tests;

public sealed class FighterEventTests
{
    [Fact]
    public void InitialStateIsNotDeployed()
    {
        Assert.False(new EliteDangerousDataModel().Ship.Fighter.IsDeployed);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LaunchFighterMarksDeployedRegardlessOfPlayerControl(bool playerControlled)
    {
        var model = new EliteDangerousDataModel();

        Apply(model, $$"""{"event":"LaunchFighter","ID":10,"PlayerControlled":{{playerControlled.ToString().ToLowerInvariant()}}}""");

        Assert.True(model.Ship.Fighter.IsDeployed);
    }

    [Fact]
    public void DockFighterEndsDeployment()
    {
        var model = new EliteDangerousDataModel();
        Apply(model, """{"event":"LaunchFighter","ID":10}""");

        Apply(model, """{"event":"DockFighter","ID":10}""");

        Assert.False(model.Ship.Fighter.IsDeployed);
    }

    [Fact]
    public void DestroyedFighterCanBeRelaunchedWithSameId()
    {
        var model = new EliteDangerousDataModel();
        Apply(model, """{"event":"LaunchFighter","ID":12,"PlayerControlled":true}""");

        Apply(model, """{"event":"FighterDestroyed","ID":12}""");
        Assert.False(model.Ship.Fighter.IsDeployed);

        Apply(model, """{"event":"LaunchFighter","ID":12,"PlayerControlled":true}""");
        Assert.True(model.Ship.Fighter.IsDeployed);
    }

    [Fact]
    public void MultipleFightersRemainDeployedUntilEachKnownIdEnds()
    {
        var model = new EliteDangerousDataModel();
        Apply(model, """{"event":"LaunchFighter","ID":10}""");
        Apply(model, """{"event":"CrewLaunchFighter","ID":20}""");

        Assert.True(model.Ship.Fighter.IsDeployed);
        Apply(model, """{"event":"DockFighter","ID":10}""");
        Assert.True(model.Ship.Fighter.IsDeployed);
        Apply(model, """{"event":"FighterDestroyed","ID":20}""");
        Assert.False(model.Ship.Fighter.IsDeployed);
    }

    [Fact]
    public void DuplicateKnownLaunchIsCountedOnce()
    {
        var model = new EliteDangerousDataModel();
        Apply(model, """{"event":"LaunchFighter","ID":10}""");
        Apply(model, """{"event":"LaunchFighter","ID":10}""");

        Apply(model, """{"event":"DockFighter","ID":10}""");

        Assert.False(model.Ship.Fighter.IsDeployed);
    }

    [Theory]
    [InlineData("DockFighter")]
    [InlineData("FighterDestroyed")]
    public void UnknownEndIdDoesNotClearAnotherFighter(string eventName)
    {
        var model = new EliteDangerousDataModel();
        Apply(model, """{"event":"LaunchFighter","ID":10}""");

        Apply(model, $$"""{"event":"{{eventName}}","ID":99}""");

        Assert.True(model.Ship.Fighter.IsDeployed);
    }

    [Fact]
    public void FighterRebuiltDoesNotMarkDeployed()
    {
        var model = new EliteDangerousDataModel();

        var rebuilt = JsonConvert.DeserializeObject<IJournalEvent>(
            """{"event":"FighterRebuilt","ID":10}""", JournalEvent.JournalEventSettings);

        Assert.Null(rebuilt);
        Assert.False(model.Ship.Fighter.IsDeployed);
    }

    [Fact]
    public void LoadoutResetsTrackedFighters()
    {
        var model = new EliteDangerousDataModel();
        Apply(model, """{"event":"LaunchFighter","ID":10}""");

        Apply(model, """{"event":"Loadout","Ship":"anaconda"}""");

        Assert.False(model.Ship.Fighter.IsDeployed);
    }

    [Fact]
    public void DeathResetsTrackedFighters()
    {
        var model = new EliteDangerousDataModel();
        Apply(model, """{"event":"LaunchFighter","ID":10}""");

        Apply(model, """{"event":"Died"}""");

        Assert.False(model.Ship.Fighter.IsDeployed);
    }

    [Fact]
    public void ReplaySequenceReconstructsFinalDeploymentState()
    {
        var model = new EliteDangerousDataModel();
        var replay = new[]
        {
            """{"event":"Loadout","Ship":"anaconda"}""",
            """{"event":"LaunchFighter","ID":10,"PlayerControlled":false}""",
            """{"event":"CrewLaunchFighter","ID":20}""",
            """{"event":"DockFighter","ID":10}"""
        };

        foreach (var line in replay)
            Apply(model, line);

        Assert.True(model.Ship.Fighter.IsDeployed);
        Apply(model, """{"event":"DockFighter","ID":20}""");
        Assert.False(model.Ship.Fighter.IsDeployed);
    }

    private static void Apply(EliteDangerousDataModel model, string json) => Parse(json).ApplyUpdate(model);

    private static IJournalEvent Parse(string json)
    {
        var journalEvent = JsonConvert.DeserializeObject<IJournalEvent>(json, JournalEvent.JournalEventSettings);
        Assert.NotNull(journalEvent);
        return journalEvent;
    }
}
