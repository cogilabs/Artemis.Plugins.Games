using Artemis.Core;
using Artemis.Core.Modules;

namespace Artemis.Plugins.Games.EliteDangerous.DataModels
{
    public class OnFoot
    {
        [DataModelProperty(Description = "Whether the player is currently on foot.")]
        public bool IsOnFoot { get; internal set; }

        [DataModelProperty(Description = "Whether the player is travelling in an Apex Interstellar taxi.")]
        public bool IsInTaxi { get; internal set; }

        [DataModelProperty(Description = "Whether the player is participating in a multicrew session.")]
        public bool IsInMulticrew { get; internal set; }

        [DataModelProperty(Description = "Whether the player is on foot inside a station.")]
        public bool IsInStation { get; internal set; }

        [DataModelProperty(Description = "Whether the player is on foot on a planetary surface.")]
        public bool IsOnPlanet { get; internal set; }

        [DataModelProperty(Description = "Whether the player is aiming down the sights of the selected weapon.")]
        public bool IsAimingDownSights { get; internal set; }

        [DataModelProperty(Description = "Whether the suit oxygen level is low.")]
        public bool IsLowOnOxygen { get; internal set; }

        [DataModelProperty(Description = "Whether the player's health is low.")]
        public bool IsLowOnHealth { get; internal set; }

        [DataModelProperty(Description = "Whether the player is in a cold environment.")]
        public bool IsCold { get; internal set; }

        [DataModelProperty(Description = "Whether the player is in a hot environment.")]
        public bool IsHot { get; internal set; }

        [DataModelProperty(Description = "Whether the player is in a dangerously cold environment.")]
        public bool IsVeryCold { get; internal set; }

        [DataModelProperty(Description = "Whether the player is in a dangerously hot environment.")]
        public bool IsVeryHot { get; internal set; }

        [DataModelProperty(Description = "Whether the player is on foot inside a hangar.")]
        public bool IsInHangar { get; internal set; }

        [DataModelProperty(Description = "Whether the player is on foot in a social space.")]
        public bool IsInSocialSpace { get; internal set; }

        [DataModelProperty(Description = "Whether the player is on foot outside a pressurized interior.")]
        public bool IsExterior { get; internal set; }

        [DataModelProperty(Description = "Whether the surrounding atmosphere is breathable.")]
        public bool HasBreathableAtmosphere { get; internal set; }

        [DataModelProperty(Description = "Whether the player is participating in multicrew through telepresence.")]
        public bool IsInTelepresenceMulticrew { get; internal set; }

        [DataModelProperty(Description = "Whether the player is physically present in another commander's ship.")]
        public bool IsInPhysicalMulticrew { get; internal set; }

        [DataModelProperty(Description = "Remaining suit oxygen, normalized between 0 and 1.", MinValue = 0f, MaxValue = 1f)]
        public float? Oxygen { get; internal set; }

        [DataModelProperty(Description = "Player health, normalized between 0 and 1.", MinValue = 0f, MaxValue = 1f)]
        public float? Health { get; internal set; }

        [DataModelProperty(Description = "Temperature in Kelvin.")]
        public float? Temperature { get; internal set; }

        private string selectedWeapon;

        [DataModelProperty(Description = "Internal name of the currently selected on-foot weapon or tool.")]
        public string SelectedWeapon
        {
            get => selectedWeapon;
            internal set
            {
                selectedWeapon = value;
                SelectedWeaponType = ClassifySelectedWeapon(value);
            }
        }

        [DataModelProperty(Description = "Category of the currently selected on-foot weapon or tool.")]
        public SelectedWeaponType SelectedWeaponType { get; private set; }

        [DataModelProperty(Description = "Gravity relative to 1G.")]
        public float? Gravity { get; internal set; }

        [DataModelProperty(Description = "Body name reported by Status.json; does not replace journal navigation state.")]
        public string BodyName { get; internal set; }

        [DataModelProperty(Description = "Occurs when the player boards a ship or SRV.")]
        public DataModelEvent<OnFootTransitionEventArgs> Embark { get; } = new();

        [DataModelProperty(Description = "Occurs when the player exits a ship or SRV on foot.")]
        public DataModelEvent<OnFootTransitionEventArgs> Disembark { get; } = new();

        internal static SelectedWeaponType ClassifySelectedWeapon(string selectedWeapon)
        {
            if (string.IsNullOrWhiteSpace(selectedWeapon))
                return SelectedWeaponType.None;

            var normalized = selectedWeapon.Trim();
            if (normalized.StartsWith("$", System.StringComparison.Ordinal))
                normalized = normalized[1..];
            if (normalized.EndsWith("_name;", System.StringComparison.OrdinalIgnoreCase))
                normalized = normalized[..^6];

            if (normalized.StartsWith("wpn_m_", System.StringComparison.OrdinalIgnoreCase))
                return SelectedWeaponType.Primary;
            if (normalized.StartsWith("wpn_s_", System.StringComparison.OrdinalIgnoreCase))
                return SelectedWeaponType.Secondary;
            if (normalized.Equals("humanoid_fists", System.StringComparison.OrdinalIgnoreCase))
                return SelectedWeaponType.Fists;
            if (normalized.Equals("humanoid_rechargetool", System.StringComparison.OrdinalIgnoreCase))
                return SelectedWeaponType.RechargeTool;
            if (normalized.Equals("humanoid_companalyser", System.StringComparison.OrdinalIgnoreCase))
                return SelectedWeaponType.CompAnalyser;
            if (normalized.Equals("humanoid_sampletool", System.StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("humanoid_repairtool", System.StringComparison.OrdinalIgnoreCase) ||
                normalized.Equals("humanoid_cuttertool", System.StringComparison.OrdinalIgnoreCase))
                return SelectedWeaponType.SuitTool;

            return SelectedWeaponType.Unknown;
        }
    }

    public class OnFootTransitionEventArgs : DataModelEventArgs
    {
        public bool SRV { get; init; }
        public bool Taxi { get; init; }
        public bool Multicrew { get; init; }
        public int? ShipID { get; init; }
        public bool OnStation { get; init; }
        public bool OnPlanet { get; init; }
        public string StarSystem { get; init; }
        public string Body { get; init; }
        public string StationName { get; init; }
    }
}
