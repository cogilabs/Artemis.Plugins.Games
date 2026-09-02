using Artemis.Plugins.Games.EliteDangerous.DataModels;
using Artemis.Plugins.Games.EliteDangerous.Utils;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;

namespace Artemis.Plugins.Games.EliteDangerous.Status
{
    internal class StatusParser : FileReaderBase
    {
        private readonly string dataDirectory;

        public StatusParser(string dataDirectory) : base(false)
        {
            this.dataDirectory = dataDirectory;
        }

        public override void Activate() => OpenFile(Path.Combine(dataDirectory, "Status.json"));

        protected override void OnContentRead(EliteDangerousDataModel dataModel, string content)
        {
            var trimmed = content.Split(Environment.NewLine).FirstOrDefault();
            if (trimmed == null) return;
            var status = JsonConvert.DeserializeObject<StatusJson>(trimmed);
            if (status == null) return;

            ApplyStatus(dataModel, status);
        }

        internal static void ApplyStatus(EliteDangerousDataModel dataModel, StatusJson status)
        {
            var flags2 = status.Flags2 ?? StatusFlags2.None;

            bool Has(StatusFlags flag) => (status.Flags & flag) != 0;
            bool Has2(StatusFlags2 flag) => (flags2 & flag) != 0;

            //
            // Update the datamodel based on the parsed values
            //

            // Player details
            dataModel.Player.CurrentlyPiloting =
                  Has2(StatusFlags2.OnFoot) ? Vehicle.OnFoot
                : Has(StatusFlags.PilotingMainShip) ? Vehicle.Ship
                : Has(StatusFlags.PilotingFighter) ? Vehicle.Fighter
                : Has(StatusFlags.PilotingSRV) ? Vehicle.SRV
                : Vehicle.Unknown;
            dataModel.Player.LegalState = status.LegalState;
            dataModel.Player.InWing = Has(StatusFlags.InWing);

            // Odyssey/on-foot state
            var onFoot = dataModel.Player.OnFoot;
            onFoot.IsOnFoot = Has2(StatusFlags2.OnFoot);
            onFoot.IsInTaxi = Has2(StatusFlags2.InTaxi);
            onFoot.IsInMulticrew = Has2(StatusFlags2.InMulticrew);
            onFoot.IsInStation = Has2(StatusFlags2.OnFootInStation);
            onFoot.IsOnPlanet = Has2(StatusFlags2.OnFootOnPlanet);
            onFoot.IsAimingDownSights = Has2(StatusFlags2.AimDownSight);
            onFoot.IsLowOnOxygen = Has2(StatusFlags2.LowOxygen);
            onFoot.IsLowOnHealth = Has2(StatusFlags2.LowHealth);
            onFoot.IsCold = Has2(StatusFlags2.Cold);
            onFoot.IsHot = Has2(StatusFlags2.Hot);
            onFoot.IsVeryCold = Has2(StatusFlags2.VeryCold);
            onFoot.IsVeryHot = Has2(StatusFlags2.VeryHot);
            onFoot.IsInHangar = Has2(StatusFlags2.OnFootInHangar);
            onFoot.IsInSocialSpace = Has2(StatusFlags2.OnFootSocialSpace);
            onFoot.IsExterior = Has2(StatusFlags2.OnFootExterior);
            onFoot.HasBreathableAtmosphere = Has2(StatusFlags2.BreathableAtmosphere);
            onFoot.IsInTelepresenceMulticrew = Has2(StatusFlags2.TelepresenceMulticrew);
            onFoot.IsInPhysicalMulticrew = Has2(StatusFlags2.PhysicalMulticrew);

            onFoot.Oxygen = onFoot.IsOnFoot ? status.Oxygen : null;
            onFoot.Health = onFoot.IsOnFoot ? status.Health : null;
            onFoot.Temperature = onFoot.IsOnFoot ? status.Temperature : null;
            onFoot.SelectedWeapon = onFoot.IsOnFoot ? status.SelectedWeapon : null;
            onFoot.Gravity = onFoot.IsOnFoot ? status.Gravity : null;
            onFoot.BodyName = onFoot.IsOnFoot ? status.BodyName : null;

            // HUD
            dataModel.HUD.FocusedPanel = status.GuiFocus;
            dataModel.HUD.AnalysisModeActive = Has(StatusFlags.AnalysisMode);
            dataModel.HUD.NightVisionActive = Has(StatusFlags.NightVision);

            // Nav
            dataModel.Navigation.DockStatus.IsDocked = Has(StatusFlags.Docked);
            dataModel.Navigation.DockStatus.IsLanded = Has(StatusFlags.Landed);
            dataModel.Navigation.Latitude = status.Latitude;
            dataModel.Navigation.Longitude = status.Longitude;
            dataModel.Navigation.Altitude = status.Altitude;
            dataModel.Navigation.Heading = status.Heading;

            // Ship
            dataModel.Ship.IsInSupercruise = Has(StatusFlags.Supercruise);
            dataModel.Ship.Systems.LandingGearDeployed = Has(StatusFlags.LandingGearDeployed);
            dataModel.Ship.Systems.CargoScoopDeployed = Has(StatusFlags.CargoScoopDeployed);
            dataModel.Ship.Systems.HardpointsDeployed = Has(StatusFlags.HardpointsDeployed);
            dataModel.Ship.Systems.ShieldsActive = Has(StatusFlags.ShieldsUp);
            dataModel.Ship.Systems.FlightAssistActive = !Has(StatusFlags.FlightAssistOff);
            dataModel.Ship.Systems.LightsActive = Has(StatusFlags.PilotingMainShip) && Has(StatusFlags.LightsOn);
            dataModel.Ship.Systems.SilentRunningActive = Has(StatusFlags.SilentRunning);
            dataModel.Ship.Systems.IsOverheating = Has(StatusFlags.Overheating);
            dataModel.Ship.IsInDanger = Has(StatusFlags.InDanger);
            dataModel.Ship.IsBeingInterdicted = Has(StatusFlags.BeingInterdicted);
            dataModel.Ship.IsInGlideMode = Has2(StatusFlags2.GlideMode);
            dataModel.Ship.IsSupercruiseOverdriveActive = Has2(StatusFlags2.SupercruiseOverdriveActive);
            dataModel.Ship.IsSupercruiseAssistActive = Has2(StatusFlags2.SupercruiseAssistActive);

            // Ship power
            dataModel.Ship.Systems.SystemPips = status.Pips[0] / 2f;
            dataModel.Ship.Systems.EnginePips = status.Pips[1] / 2f;
            dataModel.Ship.Systems.WeaponPips = status.Pips[2] / 2f;

            // Ship FSD
            dataModel.Ship.FSD.IsCharging = Has(StatusFlags.FSDCharging);
            dataModel.Ship.FSD.IsJumping = Has(StatusFlags.FSDJump);
            dataModel.Ship.FSD.IsCoolingDown = Has(StatusFlags.FSDCooldown);
            dataModel.Ship.FSD.IsMassLocked = Has(StatusFlags.FSDMassLocked);
            dataModel.Ship.FSD.IsHyperdriveCharging = Has2(StatusFlags2.FSDHyperdriveCharging);

            // Ship fuel
            dataModel.Ship.Fuel.UpdateMainFuel(status.Fuel.FuelMain);
            dataModel.Ship.Fuel.FuelReservoir = status.Fuel.FuelReservoir;
            dataModel.Ship.Fuel.IsLow = Has(StatusFlags.LowFuel);
            dataModel.Ship.Fuel.IsScooping = Has(StatusFlags.FuelScooping);

            // SRV
            dataModel.SRV.HandbrakeActive = Has(StatusFlags.SRVHandbrake);
            dataModel.SRV.TurretViewActive = Has(StatusFlags.SRVTurretView);
            dataModel.SRV.TurretRetracted = Has(StatusFlags.SRVTurretRetracted);
            dataModel.SRV.DriveAssistActive = Has(StatusFlags.SRVDriveAssist);
            dataModel.SRV.LightsActive = Has(StatusFlags.PilotingSRV) && Has(StatusFlags.LightsOn);
            dataModel.SRV.HighBeamActive = Has(StatusFlags.SRVHighBeam);
        }
    }
}
