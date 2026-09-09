using Artemis.Plugins.Games.EliteDangerous.DataModels;

namespace Artemis.Plugins.Games.EliteDangerous.Journal.Other
{
    internal sealed class LaunchFighterEvent : IJournalEvent
    {
        public int ID;
        public bool PlayerControlled;

        public void ApplyUpdate(EliteDangerousDataModel model) => model.Ship.Fighter.Launch(ID);
    }

    internal sealed class CrewLaunchFighterEvent : IJournalEvent
    {
        public int ID;

        public void ApplyUpdate(EliteDangerousDataModel model) => model.Ship.Fighter.Launch(ID);
    }

    internal sealed class DockFighterEvent : IJournalEvent
    {
        public int ID;

        public void ApplyUpdate(EliteDangerousDataModel model) => model.Ship.Fighter.EndDeployment(ID);
    }

    internal sealed class FighterDestroyedEvent : IJournalEvent
    {
        public int ID;

        public void ApplyUpdate(EliteDangerousDataModel model) => model.Ship.Fighter.EndDeployment(ID);
    }
}
