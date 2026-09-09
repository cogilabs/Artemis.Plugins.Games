using Artemis.Plugins.Games.EliteDangerous.DataModels;
using System;

namespace Artemis.Plugins.Games.EliteDangerous.Journal.Other
{
    internal sealed class RebootRepairEvent : IJournalEvent
    {
        public string[] Modules = Array.Empty<string>();

        public void ApplyUpdate(EliteDangerousDataModel model)
        {
            model.Ship.RebootRepair.Trigger(new RebootRepairEventArgs
            {
                Modules = Modules ?? Array.Empty<string>()
            });
        }
    }

    internal sealed class SystemsShutdownEvent : IJournalEvent
    {
        public void ApplyUpdate(EliteDangerousDataModel model) => model.Ship.SystemsShutdown.Trigger();
    }
}
