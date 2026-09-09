using Artemis.Plugins.Games.EliteDangerous.DataModels;
using Artemis.Plugins.Games.EliteDangerous.Utils;

namespace Artemis.Plugins.Games.EliteDangerous.Journal.Startup
{
    internal class LoadoutEvent : IJournalEvent
    {
        public string Ship;
        public string ShipName;
        public string ShipIdent;
        public float MaxJumpRange;
        public FuelCapacity FuelCapacity;

        public void ApplyUpdate(EliteDangerousDataModel model)
        {
            model.Ship.Name = ShipName;
            model.Ship.Ident = ShipIdent;
            (model.Ship.Type, model.Ship.Size) = ShipTypeDefinitions.GetById(Ship);
            model.Navigation.MaximumUnladenJumpRange = MaxJumpRange;
            model.Ship.Fuel.UpdateCapacity(FuelCapacity?.Main, FuelCapacity?.Reserve);
            model.Ship.Fighter.Reset();
        }
    }

    internal class FuelCapacity
    {
        public float Main;
        public float Reserve;
    }
}
