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

        [DataModelProperty(Description = "Internal name of the currently selected on-foot weapon or tool.")]
        public string SelectedWeapon { get; internal set; }

        [DataModelProperty(Description = "Gravity relative to 1G.")]
        public float? Gravity { get; internal set; }

        [DataModelProperty(Description = "Body name reported by Status.json; does not replace journal navigation state.")]
        public string BodyName { get; internal set; }
    }
}
