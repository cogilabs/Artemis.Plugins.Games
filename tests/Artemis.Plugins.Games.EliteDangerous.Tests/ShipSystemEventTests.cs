using Artemis.Plugins.Games.EliteDangerous.DataModels;
using Artemis.Plugins.Games.EliteDangerous.Journal;
using Newtonsoft.Json;
using Xunit;

namespace Artemis.Plugins.Games.EliteDangerous.Tests;

public sealed class ShipSystemEventTests
{
    [Fact]
    public void RebootRepairTriggersOnceWithModulesInOrder()
    {
        var model = new EliteDangerousDataModel();
        var journalEvent = Parse("""
                                 {"event":"RebootRepair","Modules":["MainEngines","TinyHardpoint1"]}
                                 """);

        journalEvent.ApplyUpdate(model);

        Assert.Equal(1, model.Ship.RebootRepair.TriggerCount);
        Assert.Equal(new[] { "MainEngines", "TinyHardpoint1" }, model.Ship.RebootRepair.LastEventArguments.Modules);
    }

    [Fact]
    public void RebootRepairWithEmptyModulesStillTriggers()
    {
        var model = new EliteDangerousDataModel();
        var journalEvent = Parse("""{"event":"RebootRepair","Modules":[]}""");

        journalEvent.ApplyUpdate(model);

        Assert.Equal(1, model.Ship.RebootRepair.TriggerCount);
        Assert.Empty(model.Ship.RebootRepair.LastEventArguments.Modules);
    }

    [Fact]
    public void RebootRepairWithOmittedModulesStillTriggers()
    {
        var model = new EliteDangerousDataModel();
        var journalEvent = Parse("""{"event":"RebootRepair"}""");

        journalEvent.ApplyUpdate(model);

        Assert.Equal(1, model.Ship.RebootRepair.TriggerCount);
        Assert.Empty(model.Ship.RebootRepair.LastEventArguments.Modules);
    }

    [Fact]
    public void SystemsShutdownTriggersExactlyOnceWithoutArguments()
    {
        var model = new EliteDangerousDataModel();
        var journalEvent = Parse("""{"event":"SystemsShutdown"}""");

        journalEvent.ApplyUpdate(model);

        Assert.Equal(1, model.Ship.SystemsShutdown.TriggerCount);
    }

    [Fact]
    public void ShutdownDoesNotResolveAsSystemsShutdownOrTriggerIt()
    {
        var model = new EliteDangerousDataModel();

        var gameShutdown = JsonConvert.DeserializeObject<IJournalEvent>(
            """{"event":"Shutdown"}""", JournalEvent.JournalEventSettings);

        Assert.Null(gameShutdown);
        Assert.Equal(0, model.Ship.SystemsShutdown.TriggerCount);
    }

    private static IJournalEvent Parse(string json)
    {
        var journalEvent = JsonConvert.DeserializeObject<IJournalEvent>(json, JournalEvent.JournalEventSettings);
        Assert.NotNull(journalEvent);
        return journalEvent;
    }
}
