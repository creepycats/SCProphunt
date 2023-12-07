using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Events;
using Exiled.Events.EventArgs;
using Exiled.Events.EventArgs.Server;
using InventorySystem.Items.Pickups;
using SCProphunt.Types;
using System.Collections.Generic;

namespace SCProphunt.handlers
{
    public class serverHandler
    {
        public void WaitingForPlayers() {
            SCProphunt.Instance.PropDictionary = new Dictionary<string, PropItemComponent>(); // Resets list after each round to hopefully minimize cross round lag
            SCProphunt.Instance.PropHuntRound = null;
            SCProphunt.Instance.RandomlySpawnedItems = new List<ItemPickupBase>();
            SCProphunt.Instance.LastRoundTeams = null;
            SoundHandler.StopAudio();
        }

        public void RespawningTeam(RespawningTeamEventArgs args)
        {
            if (SCProphunt.Instance.PropHuntRound == null) return;
            args.IsAllowed = false;
        }
    }
}
