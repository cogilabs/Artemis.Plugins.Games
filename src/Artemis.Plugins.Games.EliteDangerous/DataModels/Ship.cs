using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Games.EliteDangerous.Journal;

namespace Artemis.Plugins.Games.EliteDangerous.DataModels
{
    public class Ship
    {
        public string Name { get; internal set; } = "Unknown";
        public string Ident { get; internal set; } = "Unknown";
        public ShipType Type { get; internal set; } = ShipType.Unknown;
        public ShipSize Size { get; internal set; } = ShipSize.Unknown;

        public bool IsInSupercruise { get; internal set; }

        public bool IsInDanger { get; internal set; }
        public bool IsBeingInterdicted { get; internal set; }
        [DataModelProperty(Description = "Whether the ship is gliding towards a planetary surface.")]
        public bool IsInGlideMode { get; internal set; }

        [DataModelProperty(Description = "Whether supercruise overdrive is active.")]
        public bool IsSupercruiseOverdriveActive { get; internal set; }

        [DataModelProperty(Description = "Whether supercruise assist is active.")]
        public bool IsSupercruiseAssistActive { get; internal set; }

        [DataModelProperty(Description = "Event that occurs when the player successfully evades an interdiction or is interdicted by another ship. ")]
        public DataModelEvent<InterdictionEventArgs> Interdiction { get; } = new();

        [DataModelProperty(Description = "Occurs when Elite records a ship reboot/repair operation.")]
        public DataModelEvent<RebootRepairEventArgs> RebootRepair { get; } = new();

        [DataModelProperty(Description = "Occurs when the ship's systems shut down.")]
        public DataModelEvent SystemsShutdown { get; } = new();

        public ShipSystems Systems { get; } = new();
        public Fuel Fuel { get; } = new();
        public FSD FSD { get; } = new();
    }


    public class InterdictionEventArgs : DataModelEventArgs
    {
        public string Interdictor { get; init; }
        public bool InterdictorIsPlayer { get; init; }
        public bool Escaped { get; init; }
        public bool Submitted { get; init; }
    }

    public class RebootRepairEventArgs : DataModelEventArgs
    {
        public string[] Modules { get; init; } = System.Array.Empty<string>();
    }
}
