using Artemis.Core.Modules;
using System.Collections.Generic;

namespace Artemis.Plugins.Games.EliteDangerous.DataModels
{
    public class Fighter
    {
        private readonly HashSet<int> deployedFighterIds = new();

        [DataModelProperty(Description = "Whether at least one ship-launched fighter is deployed outside the mothership.")]
        public bool IsDeployed => deployedFighterIds.Count > 0;

        internal void Launch(int fighterId) => deployedFighterIds.Add(fighterId);

        internal void EndDeployment(int fighterId) => deployedFighterIds.Remove(fighterId);

        internal void Reset() => deployedFighterIds.Clear();
    }
}
