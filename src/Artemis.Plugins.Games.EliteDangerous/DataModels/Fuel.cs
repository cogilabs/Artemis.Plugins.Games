using Artemis.Core.Modules;

namespace Artemis.Plugins.Games.EliteDangerous.DataModels
{
    public class Fuel
    {
        [DataModelProperty(Name = "Fuel (Main)", Description = "Amount of fuel in the main fuel tank (thick bar on the HUD).")]
        public float FuelMain { get; internal set; }

        [DataModelProperty(Name = "Fuel (Reservoir)", Description = "Amount of fuel in the reservoir fuel tank (thin bar on HUD).")]
        public float FuelReservoir { get; internal set; }

        [DataModelProperty(Description = "Capacity of the main fuel tank in tons.")]
        public float? MainCapacity { get; internal set; }

        [DataModelProperty(Description = "Capacity of the reservoir fuel tank in tons.")]
        public float? ReserveCapacity { get; internal set; }

        [DataModelProperty(Description = "Main fuel tank fill level, normalized between 0 and 1.", MinValue = 0f, MaxValue = 1f)]
        public float? FuelPercent { get; internal set; }

        [DataModelProperty(Description = "If the ship currently has less than 25% fuel.")]
        public bool IsLow { get; internal set; }

        [DataModelProperty(Description = "If the fuel scoop is currently deployed and gathering fuel from a star.")]
        public bool IsScooping { get; internal set; }

        internal void UpdateMainFuel(float fuelMain)
        {
            FuelMain = fuelMain;
            RecalculateFuelPercent();
        }

        internal void UpdateCapacity(float? mainCapacity, float? reserveCapacity)
        {
            MainCapacity = mainCapacity > 0f ? mainCapacity : null;
            ReserveCapacity = reserveCapacity >= 0f ? reserveCapacity : null;
            RecalculateFuelPercent();
        }

        private void RecalculateFuelPercent()
        {
            FuelPercent = MainCapacity is > 0f
                ? System.Math.Clamp(FuelMain / MainCapacity.Value, 0f, 1f)
                : null;
        }
    }
}
