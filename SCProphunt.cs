using Exiled.API.Features;
using Player = Exiled.Events.Handlers.Player;
using Server = Exiled.Events.Handlers.Server;
using System;
using System.Collections.Generic;
using MEC;
using HarmonyLib;
using SCProphunt.Types;
using UnityEngine;
using Exiled.API.Features.Items;
using InventorySystem.Items.Pickups;

namespace SCProphunt
{
    public class SCProphunt : Plugin<Config.Config>
    {
        public override string Name => "SCProphunt";
        public override string Author => "creepycats";
        public override Version Version => new Version(1, 1, 0);

        public static SCProphunt Instance { get; set; }

        // Event Handling
        public PropHuntRoundInfo PropHuntRound = null;

        // Prop Handling
        public Dictionary<string, PropItemComponent> PropDictionary { get; set; } = new Dictionary<string, PropItemComponent>();

        // Switching Teams
        public Dictionary<string, string> LastRoundTeams { get; set; } = null;

        public List<ItemPickupBase> RandomlySpawnedItems { get; set; } = new List<ItemPickupBase>();

        private Harmony _harmony;

        public override void OnEnabled()
        {
            Instance = this;
            Log.Info($"{Name} v{Version} - made for v13.1.1 by {Author}");
            if (Config.Debug)
                Log.Info("Registering events...");
            RegisterEvents();
            PropDictionary = new Dictionary<string, PropItemComponent>();
            RandomlySpawnedItems = new List<ItemPickupBase>();

            Timing.RunCoroutine(Props.LockProps());
            Timing.RunCoroutine(Game.HandleGameUpdate());
            Timing.RunCoroutine(Game.HandleHints());

            //_harmony = new("SCPCosmetics");
            //_harmony.PatchAll();
            Log.Info("Plugin Enabled!");
        }
        public override void OnDisabled()
        {
            if (Config.Debug)
                Log.Info("Unregistering events...");
            UnregisterEvents();
            PropDictionary = new Dictionary<string, PropItemComponent>();
            RandomlySpawnedItems = new List<ItemPickupBase>();

            //_harmony.UnpatchAll();
            //_harmony = null;
            Log.Info("Disabled Plugin Successfully");
        }

        // NotesToSelf
        // OBJECT.EVENT += FUNCTION > Add Function to Callback
        // OBJECT.EVENT -= FUNCTION > Remove Function from Callback

        private handlers.serverHandler ServerHandler;
        private handlers.playerHandler PlayerHandler;

        public void RegisterEvents() 
        {
            ServerHandler = new handlers.serverHandler();
            PlayerHandler = new handlers.playerHandler();

            Server.WaitingForPlayers += ServerHandler.WaitingForPlayers;
            Server.RespawningTeam += ServerHandler.RespawningTeam;

            Player.ChangingRole += PlayerHandler.ChangingRole;
            Player.SearchingPickup += PlayerHandler.SearchingPickup;
            Player.Died += PlayerHandler.Died;
            Player.TogglingNoClip += PlayerHandler.TogglingNoClip;
            Player.Shooting += PlayerHandler.Shooting;
            Player.DroppingAmmo += PlayerHandler.DroppingAmmo;
            Player.DroppingItem += PlayerHandler.DroppingItem;
            Player.Left += PlayerHandler.Left;
            Player.ChangingItem += PlayerHandler.ChangingItem;
            Player.TriggeringTesla += PlayerHandler.TriggeringTesla;
        }

        public void UnregisterEvents()
        {
            Server.WaitingForPlayers -= ServerHandler.WaitingForPlayers;
            Server.RespawningTeam -= ServerHandler.RespawningTeam;

            Player.ChangingRole -= PlayerHandler.ChangingRole;
            Player.SearchingPickup -= PlayerHandler.SearchingPickup;
            Player.Died -= PlayerHandler.Died;
            Player.TogglingNoClip -= PlayerHandler.TogglingNoClip;
            Player.Shooting -= PlayerHandler.Shooting;
            Player.DroppingAmmo -= PlayerHandler.DroppingAmmo;
            Player.DroppingItem -= PlayerHandler.DroppingItem;
            Player.Left -= PlayerHandler.Left;
            Player.ChangingItem -= PlayerHandler.ChangingItem;
            Player.TriggeringTesla -= PlayerHandler.TriggeringTesla;
        }
    }
}