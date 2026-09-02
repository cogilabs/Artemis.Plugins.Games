using Artemis.Plugins.Games.EliteDangerous.DataModels;
using Artemis.Plugins.Games.EliteDangerous.Journal;
using Artemis.Plugins.Games.EliteDangerous.Status;
using Newtonsoft.Json;
using Xunit;

namespace Artemis.Plugins.Games.EliteDangerous.Tests;

public sealed class OdysseyStatusTests
{
    [Theory]
    [InlineData("$wpn_m_assaultrifle_kinetic_fauto_name;", SelectedWeaponType.Primary)]
    [InlineData("$wpn_m_sniper_plasma_charged_name;", SelectedWeaponType.Primary)]
    [InlineData("$wpn_s_pistol_laser_sauto_name;", SelectedWeaponType.Secondary)]
    [InlineData("$humanoid_fists_name;", SelectedWeaponType.Fists)]
    [InlineData("$humanoid_rechargetool_name;", SelectedWeaponType.RechargeTool)]
    [InlineData("$humanoid_companalyser_name;", SelectedWeaponType.CompAnalyser)]
    [InlineData("$humanoid_repairtool_name;", SelectedWeaponType.SuitTool)]
    [InlineData("$humanoid_cuttertool_name;", SelectedWeaponType.SuitTool)]
    [InlineData("", SelectedWeaponType.None)]
    [InlineData(null, SelectedWeaponType.None)]
    [InlineData("$future_weapon_name;", SelectedWeaponType.Unknown)]
    [InlineData("  $WPN_M_ASSAULTRIFLE_KINETIC_FAUTO_NAME;  ", SelectedWeaponType.Primary)]
    public void ClassifiesSelectedWeapon(string selectedWeapon, SelectedWeaponType expected)
    {
        var model = Apply(new StatusJson
        {
            Flags2 = StatusFlags2.OnFoot,
            SelectedWeapon = selectedWeapon
        });

        Assert.Equal(selectedWeapon, model.Player.OnFoot.SelectedWeapon);
        Assert.Equal(expected, model.Player.OnFoot.SelectedWeaponType);
    }

    [Fact]
    public void MapsDocumentedFlagsFromRealStationHangarSample()
    {
        var model = Apply(new StatusJson { Flags2 = (StatusFlags2)90121 });

        Assert.True(model.Player.OnFoot.IsOnFoot);
        Assert.True(model.Player.OnFoot.IsInStation);
        Assert.True(model.Player.OnFoot.IsInHangar);
        Assert.True(model.Player.OnFoot.IsInSocialSpace);
        Assert.True(model.Player.OnFoot.HasBreathableAtmosphere);
        Assert.False(model.Player.OnFoot.IsOnPlanet);
        Assert.False(model.Player.OnFoot.IsLowOnOxygen);
        Assert.False(model.Ship.IsSupercruiseOverdriveActive);
        Assert.Equal(Vehicle.OnFoot, model.Player.CurrentlyPiloting);
    }

    [Fact]
    public void ZeroFlags2LeavesAllOdysseyStatesFalse()
    {
        var model = Apply(new StatusJson { Flags2 = StatusFlags2.None });

        Assert.False(model.Player.OnFoot.IsOnFoot);
        Assert.False(model.Player.OnFoot.IsInStation);
        Assert.False(model.Player.OnFoot.IsInHangar);
        Assert.False(model.Player.OnFoot.HasBreathableAtmosphere);
        Assert.False(model.Ship.IsInGlideMode);
        Assert.False(model.Ship.FSD.IsHyperdriveCharging);
        Assert.Equal(Vehicle.Unknown, model.Player.CurrentlyPiloting);
    }

    [Fact]
    public void ExposesLiveOnFootFieldsWithoutChangingTheirUnits()
    {
        var model = Apply(new StatusJson
        {
            Flags2 = StatusFlags2.OnFoot,
            Oxygen = 1f,
            Health = 1f,
            Temperature = 293f,
            SelectedWeapon = "",
            Gravity = 0.8f,
            BodyName = "Horner Relay"
        });

        Assert.Equal(1f, model.Player.OnFoot.Oxygen);
        Assert.Equal(1f, model.Player.OnFoot.Health);
        Assert.Equal(293f, model.Player.OnFoot.Temperature);
        Assert.Equal("", model.Player.OnFoot.SelectedWeapon);
        Assert.Equal(0.8f, model.Player.OnFoot.Gravity);
        Assert.Equal("Horner Relay", model.Player.OnFoot.BodyName);
    }

    [Fact]
    public void ClearsTransientFieldsAfterLeavingOnFoot()
    {
        var model = new EliteDangerousDataModel();
        StatusParser.ApplyStatus(model, new StatusJson
        {
            Flags2 = StatusFlags2.OnFoot,
            Oxygen = 1f,
            Health = 1f,
            Temperature = 293f,
            SelectedWeapon = "weapon",
            Gravity = 1f,
            BodyName = "Horner Relay"
        });

        StatusParser.ApplyStatus(model, new StatusJson
        {
            Flags = StatusFlags.PilotingMainShip,
            Flags2 = StatusFlags2.None
        });

        Assert.False(model.Player.OnFoot.IsOnFoot);
        Assert.Null(model.Player.OnFoot.Oxygen);
        Assert.Null(model.Player.OnFoot.Health);
        Assert.Null(model.Player.OnFoot.Temperature);
        Assert.Null(model.Player.OnFoot.SelectedWeapon);
        Assert.Equal(SelectedWeaponType.None, model.Player.OnFoot.SelectedWeaponType);
        Assert.Null(model.Player.OnFoot.Gravity);
        Assert.Null(model.Player.OnFoot.BodyName);
        Assert.Equal(Vehicle.Ship, model.Player.CurrentlyPiloting);
    }

    [Fact]
    public void MissingFlags2IsTreatedAsNoOdysseyFlags()
    {
        var model = Apply(new StatusJson { Flags = StatusFlags.PilotingSRV });

        Assert.False(model.Player.OnFoot.IsOnFoot);
        Assert.Equal(Vehicle.SRV, model.Player.CurrentlyPiloting);
    }

    [Fact]
    public void MapsCurrentFsdScoAndSupercruiseAssistFlags()
    {
        var model = Apply(new StatusJson
        {
            Flags2 = StatusFlags2.FSDHyperdriveCharging |
                     StatusFlags2.SupercruiseOverdriveActive |
                     StatusFlags2.SupercruiseAssistActive
        });

        Assert.True(model.Ship.FSD.IsHyperdriveCharging);
        Assert.True(model.Ship.IsSupercruiseOverdriveActive);
        Assert.True(model.Ship.IsSupercruiseAssistActive);
    }

    [Theory]
    [InlineData("Embark")]
    [InlineData("Disembark")]
    public void ParsesAndTriggersOnFootTransitionEvents(string eventName)
    {
        var json = $$"""
                     {"event":"{{eventName}}","SRV":false,"Taxi":true,"Multicrew":false,"ID":36,"OnStation":true,"OnPlanet":false,"StarSystem":"Sol","Body":"Earth","StationName":"Galileo"}
                     """;
        var model = new EliteDangerousDataModel();

        var journalEvent = JsonConvert.DeserializeObject<IJournalEvent>(json, JournalEvent.JournalEventSettings);
        Assert.NotNull(journalEvent);
        journalEvent.ApplyUpdate(model);

        var args = eventName == "Embark"
            ? model.Player.OnFoot.Embark.LastEventArguments
            : model.Player.OnFoot.Disembark.LastEventArguments;
        Assert.NotNull(args);
        Assert.False(args.SRV);
        Assert.True(args.Taxi);
        Assert.False(args.Multicrew);
        Assert.Equal(36, args.ShipID);
        Assert.True(args.OnStation);
        Assert.False(args.OnPlanet);
        Assert.Equal("Sol", args.StarSystem);
        Assert.Equal("Earth", args.Body);
        Assert.Equal("Galileo", args.StationName);
    }

    private static EliteDangerousDataModel Apply(StatusJson status)
    {
        var model = new EliteDangerousDataModel();
        StatusParser.ApplyStatus(model, status);
        return model;
    }
}
