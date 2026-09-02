using Artemis.Plugins.Games.EliteDangerous.DataModels;
using Artemis.Plugins.Games.EliteDangerous.Journal;
using System;
using System.IO;
using Xunit;
using StatusParser = Artemis.Plugins.Games.EliteDangerous.Status.StatusParser;

namespace Artemis.Plugins.Games.EliteDangerous.Tests;

public sealed class FuelTests
{
    [Fact]
    public void JournalLoadoutAndStatusUpdateFuelCapacityAndPercentage()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"artemis-elite-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);

        try
        {
            File.WriteAllText(Path.Combine(directory, "Journal.2026-09-02T180000.01.log"), """
                {"timestamp":"2026-09-02T18:00:00Z","event":"Loadout","Ship":"asp","ShipName":"STEINER III","ShipIdent":"DA-08A","MaxJumpRange":50.0,"FuelCapacity":{"Main":32.0,"Reserve":0.63}}
                """);
            File.WriteAllText(Path.Combine(directory, "Status.json"), """
                {"Fuel":{"FuelMain":16.0,"FuelReservoir":0.63}}
                """);

            var model = new EliteDangerousDataModel();
            using var journalParser = new JournalParser(directory);
            using var statusParser = new StatusParser(directory);

            journalParser.Activate();
            journalParser.PerformUpdate(model);

            Assert.Equal(32f, model.Ship.Fuel.MainCapacity);
            Assert.Equal(0.63f, model.Ship.Fuel.ReserveCapacity);

            statusParser.Activate();
            statusParser.PerformUpdate(model);

            Assert.Equal(0.5f, model.Ship.Fuel.FuelPercent);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Theory]
    [InlineData(16f, 32f, 0.5f)]
    [InlineData(32f, 32f, 1f)]
    [InlineData(0f, 32f, 0f)]
    public void CalculatesNormalizedMainTankPercentage(float fuelMain, float mainCapacity, float expected)
    {
        var fuel = new Fuel();

        fuel.UpdateCapacity(mainCapacity, 0.63f);
        fuel.UpdateMainFuel(fuelMain);

        Assert.Equal(expected, fuel.FuelPercent);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0f)]
    [InlineData(-1f)]
    public void PercentageIsUnavailableWithoutValidMainCapacity(float? mainCapacity)
    {
        var fuel = new Fuel();

        fuel.UpdateMainFuel(16f);
        fuel.UpdateCapacity(mainCapacity, 0.63f);

        Assert.Null(fuel.MainCapacity);
        Assert.Null(fuel.FuelPercent);
    }

    [Fact]
    public void RecalculatesWhenCapacityChangesAfterLoadoutSwitch()
    {
        var fuel = new Fuel();
        fuel.UpdateMainFuel(16f);
        fuel.UpdateCapacity(32f, 0.63f);

        fuel.UpdateCapacity(64f, 0.63f);

        Assert.Equal(0.25f, fuel.FuelPercent);
    }
}
