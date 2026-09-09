using Artemis.Plugins.Games.EliteDangerous.DataModels;
using Newtonsoft.Json;

namespace Artemis.Plugins.Games.EliteDangerous.Journal.Travel
{
    internal abstract class OnFootTransitionEvent : IJournalEvent
    {
        public bool SRV;
        public bool Taxi;
        public bool Multicrew;
        [JsonProperty("ID")]
        public int? ShipID;
        public bool OnStation;
        public bool OnPlanet;
        public string StarSystem;
        public string Body;
        public string StationName;

        protected OnFootTransitionEventArgs CreateArguments() => new()
        {
            SRV = SRV,
            Taxi = Taxi,
            Multicrew = Multicrew,
            ShipID = ShipID,
            OnStation = OnStation,
            OnPlanet = OnPlanet,
            StarSystem = StarSystem,
            Body = Body,
            StationName = StationName
        };

        public abstract void ApplyUpdate(EliteDangerousDataModel model);
    }

    internal sealed class EmbarkEvent : OnFootTransitionEvent
    {
        public override void ApplyUpdate(EliteDangerousDataModel model) =>
            model.Player.OnFoot.Embark.Trigger(CreateArguments());
    }

    internal sealed class DisembarkEvent : OnFootTransitionEvent
    {
        public override void ApplyUpdate(EliteDangerousDataModel model) =>
            model.Player.OnFoot.Disembark.Trigger(CreateArguments());
    }
}
