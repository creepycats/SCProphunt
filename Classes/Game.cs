using CustomPlayerEffects;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Pickups;
using Interactables.Interobjects;
using InventorySystem.Items.Pickups;
using LightContainmentZoneDecontamination;
using MEC;
using Mirror;
using PlayerRoles;
using PluginAPI.Core.Zones.Light;
using SCProphunt.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using static LightContainmentZoneDecontamination.DecontaminationController;

namespace SCProphunt
{
    public class Game
    {
        /// <summary>
        /// Parses variables from Translations
        /// </summary>
        /// <param name="inputTranslation"></param>
        /// <returns></returns>
        public static string ProcessTranslation(string inputTranslation, Player player = null, ZoneType zone = ZoneType.Unspecified) {
            long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            string output = inputTranslation.Replace("{props}", SCProphunt.Instance.PropHuntRound.PropHuntTeam.Values.Count(n => n == "prop").ToString())
                .Replace("{hunters}", SCProphunt.Instance.PropHuntRound.PropHuntTeam.Values.Count(n => n == "hunter").ToString())
                .Replace("{releaseTime}", BeautifyTime(CurrentTime, SCProphunt.Instance.PropHuntRound.TimeStarted + (30 * 1000)))
                .Replace("{timeLeft}", BeautifyTime(CurrentTime, SCProphunt.Instance.PropHuntRound.TimeFinished))
                .Replace("{votingTime}", BeautifyTime(CurrentTime, SCProphunt.Instance.PropHuntRound.VotingTimeFinish))
                .Replace("{nextTaunt}", BeautifyTime(CurrentTime, SCProphunt.Instance.PropHuntRound.LastTaunt + (long)(SCProphunt.Instance.Config.TauntCooldown * 1000)))
                .Replace("{lightVotes}", SCProphunt.Instance.PropHuntRound.MapChoice.Values.Count(e => e == ZoneType.LightContainment).ToString())
                .Replace("{heavyVotes}", SCProphunt.Instance.PropHuntRound.MapChoice.Values.Count(e => e == ZoneType.HeavyContainment).ToString())
                .Replace("{entranceVotes}", SCProphunt.Instance.PropHuntRound.MapChoice.Values.Count(e => e == ZoneType.Entrance).ToString())
                .Replace("{startingIn}", BeautifyTime(CurrentTime, SCProphunt.Instance.PropHuntRound.TimeStarted))
                .Replace("{chosenMap}", (SCProphunt.Instance.PropHuntRound.ChosenMap == ZoneType.LightContainment ? $"<color=#ffef63>[{SCProphunt.Instance.Config.PluginTranslations.voting_light_label}]</color>" : (SCProphunt.Instance.PropHuntRound.ChosenMap == ZoneType.HeavyContainment ? $"<color=#4a4a4a>[{SCProphunt.Instance.Config.PluginTranslations.voting_heavy_label}]</color>" : $"<color=#ffac63>[{SCProphunt.Instance.Config.PluginTranslations.voting_entrance_label}]</color>")));

            if (player != null) {
                ZoneType playerZone = ZoneType.Unspecified;
                if (SCProphunt.Instance.PropHuntRound.MapChoice.ContainsKey(player.UserId)) playerZone = SCProphunt.Instance.PropHuntRound.MapChoice[player.UserId];

                output = output.Replace("{voteZone}", (playerZone == ZoneType.LightContainment ? $"<color=#ffef63>[{SCProphunt.Instance.Config.PluginTranslations.voting_light_label}]</color>" : (playerZone == ZoneType.HeavyContainment ? $"<color=#4a4a4a>[{SCProphunt.Instance.Config.PluginTranslations.voting_heavy_label}]</color>" : (playerZone == ZoneType.Entrance ? $"<color=#ffac63>[{SCProphunt.Instance.Config.PluginTranslations.voting_entrance_label}]</color>" : $"<color=#ff2e2e>[{SCProphunt.Instance.Config.PluginTranslations.voting_invalid_label}]</color>"))));
            }
            if (zone != ZoneType.Unspecified) {
                output = output.Replace("{voteCount}", SCProphunt.Instance.PropHuntRound.MapChoice.Values.Count(e => e == zone).ToString());
            }

            return output;
        }

        public static string ProcessTranslationKillfeed(string inputTranslation, Player victim, Player attacker = null)
        {
            string playerColor = SCProphunt.Instance.PropHuntRound.PropHuntTeam[victim.UserId] == "hunter" ? "#f7c331" : "#31bcf7";
            string attackerColor = attacker != null ? (SCProphunt.Instance.PropHuntRound.PropHuntTeam[attacker.UserId] == "hunter" ? "#f7c331" : "#31bcf7") : "";
            return ProcessTranslation(inputTranslation).Replace("{victim}", victim != null ? $"<color={playerColor}>{victim.Nickname}</color>" : "").Replace("{attacker}", attacker != null ? $"<color={attackerColor}>{attacker.Nickname}</color>" : "");
        }

        /// <summary>
        /// Handles most of the Game Logic behind Prophunt
        /// </summary>
        /// <returns></returns>
        public static IEnumerator<float> HandleGameUpdate()
        {
            System.Random random = new System.Random();
            for (; ; )
            {
                long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                if (SCProphunt.Instance.PropHuntRound != null)
                {
                    if (SCProphunt.Instance.PropHuntRound.StillInLobby)
                    {
                        if (CurrentTime < SCProphunt.Instance.PropHuntRound.TimeStarted)
                        {
                            string bcMessage = $"- <b>{SCProphunt.Instance.Config.PluginTranslations.prophunt}</b> -";
                            // Voting Here
                            if (CurrentTime < SCProphunt.Instance.PropHuntRound.VotingTimeFinish)
                            {
                                // Broadcast Current Standings
                                bcMessage += $"\n<size=24><color=#ffef63>[{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_light_label)} - {ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_vote_count, zone: ZoneType.LightContainment)}]</color> | <color=#4a4a4a>[{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_heavy_label)} - {ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_vote_count, zone: ZoneType.HeavyContainment)}]</color> | <color=#ffac63>[{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_entrance_label)} - {ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_vote_count, zone: ZoneType.Entrance)}]</color></size>\n<size=30>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_broadcast_time_left)}</size>";
                            } else
                            {
                                // Broadcast Result
                                if (SCProphunt.Instance.PropHuntRound.ChosenMap == ZoneType.Unspecified)
                                {
                                    int lightVotes = SCProphunt.Instance.PropHuntRound.MapChoice.Values.Count(e => e == ZoneType.LightContainment);
                                    int heavyVotes = SCProphunt.Instance.PropHuntRound.MapChoice.Values.Count(e => e == ZoneType.HeavyContainment);
                                    int entranceVotes = SCProphunt.Instance.PropHuntRound.MapChoice.Values.Count(e => e == ZoneType.Entrance);
                                    bool isTie = false;
                                    ZoneType leadingZone = ZoneType.LightContainment;
                                    if (lightVotes > heavyVotes)
                                    {
                                        if (lightVotes > entranceVotes)
                                        {
                                            isTie = false;
                                            leadingZone = ZoneType.LightContainment;
                                            // Light
                                        }
                                        else
                                        {
                                            if (lightVotes == entranceVotes)
                                            {
                                                isTie = true;
                                                // Tie - Light + Entrance
                                            }
                                            else
                                            {
                                                // Entrance
                                                isTie = false;
                                                leadingZone = ZoneType.Entrance;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (lightVotes == heavyVotes)
                                        {
                                            isTie = true;
                                        }
                                        else
                                        {
                                            isTie = false;
                                            leadingZone = ZoneType.HeavyContainment;
                                        }
                                        if (heavyVotes > entranceVotes)
                                        {
                                            // Heavy
                                            isTie = false;
                                            leadingZone = ZoneType.HeavyContainment;
                                        }
                                        else
                                        {
                                            if (lightVotes == entranceVotes)
                                            {
                                                if (isTie)
                                                {
                                                    // Tie - Entrance + Heavy
                                                    isTie = true;
                                                }
                                                else
                                                {
                                                    // Tie - Light + Entrance + Heavy
                                                    isTie = true;
                                                }
                                            }
                                            else
                                            {
                                                // Entrance
                                                isTie = false;
                                                leadingZone = ZoneType.Entrance;
                                            }
                                        }
                                    }

                                    SCProphunt.Instance.PropHuntRound.ChosenMap = leadingZone;
                                    SCProphunt.Instance.PropHuntRound.VoteWasTied = isTie;
                                }
                                
                                bcMessage += $"\n<size=30>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_broadcast_chosen_map)}</size>";
                                bcMessage += $"\n<size=30>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_broadcast_starting_in)}</size>";
                            }
                            Exiled.API.Features.Broadcast _bc = new Exiled.API.Features.Broadcast(bcMessage, 3);
                            Map.ClearBroadcasts();
                            Map.Broadcast(_bc, true);
                        } else
                        {
                            // START ROUND HERE AFTER VOTING
                            SCProphunt.Instance.PropHuntRound.StillInLobby = false;
                            SCProphunt.Instance.PropHuntRound.LastTaunt = SCProphunt.Instance.PropHuntRound.TimeStarted;
                            Log.Debug($"from {SCProphunt.Instance.PropHuntRound.TimeStarted} to {SCProphunt.Instance.PropHuntRound.TimeFinished}");

                            // Divide players into teams
                            List<Player> playerList = Player.List.ToList();
                            playerList.ShuffleList();

                            if (SCProphunt.Instance.LastRoundTeams != null)
                            {
                                foreach (string _playerId in SCProphunt.Instance.LastRoundTeams.Keys) // Assign Each Player to a team
                                {
                                    Player _player = Player.Get(_playerId);
                                    if (_player == null) continue;
                                    if (_player.IsNPC) continue;
                                    if (SCProphunt.Instance.LastRoundTeams[_playerId] == "prop")
                                    {
                                        SCProphunt.Instance.PropHuntRound.PropHuntTeam.Add(_player.UserId, "hunter");
                                    }
                                    else
                                    {
                                        SCProphunt.Instance.PropHuntRound.PropHuntTeam.Add(_player.UserId, "prop");
                                    }
                                    HandlePlayerTeamChange(_player, SCProphunt.Instance.PropHuntRound.PropHuntTeam[_player.UserId]);
                                    playerList.Remove(_player);
                                }
                            }

                            bool teamToggle = false;
                            foreach (Player _player in playerList) // Assign Each Player to a team
                            {
                                if (_player == null) continue;
                                if (_player.IsNPC) continue;
                                if (teamToggle)
                                {
                                    SCProphunt.Instance.PropHuntRound.PropHuntTeam.Add(_player.UserId, "hunter");
                                }
                                else
                                {
                                    SCProphunt.Instance.PropHuntRound.PropHuntTeam.Add(_player.UserId, "prop");
                                }
                                HandlePlayerTeamChange(_player, SCProphunt.Instance.PropHuntRound.PropHuntTeam[_player.UserId]);
                                teamToggle = !teamToggle;
                            }

                            // Lock Doors
                            foreach (Door door in Door.List)
                            {
                                if (door is Exiled.API.Features.Doors.BreakableDoor breakDoor)
                                {
                                    breakDoor.MaxHealth = 999999;
                                    breakDoor.Health = 999999;
                                }
                                // Lock Checkpoints, 096, Elevators
                                if (door is Exiled.API.Features.Doors.ElevatorDoor elevDoor)
                                {
                                    if (!elevDoor.IsLocked) elevDoor.ChangeLock(Exiled.API.Enums.DoorLockType.AdminCommand);
                                    if (elevDoor.IsOpen) elevDoor.IsOpen = false;
                                } else if (door is Exiled.API.Features.Doors.CheckpointDoor chkptDoor)
                                {
                                    if (!chkptDoor.IsLocked) chkptDoor.ChangeLock(Exiled.API.Enums.DoorLockType.AdminCommand);
                                    if (chkptDoor.IsOpen) chkptDoor.IsOpen = false;
                                } else if (door is Exiled.API.Features.Doors.Gate gateDoor)
                                {
                                    if (!gateDoor.IsLocked) gateDoor.ChangeLock(Exiled.API.Enums.DoorLockType.AdminCommand);
                                    if (!gateDoor.IsOpen) gateDoor.IsOpen = true;
                                } else if (door is Exiled.API.Features.Doors.BasicDoor basicDoor)
                                {
                                    if (!basicDoor.KeycardPermissions.Equals(KeycardPermissions.None) && !(basicDoor.Rooms.ToList().Count(r => r.Type == RoomType.Hcz096 || r.Type == RoomType.HczEzCheckpointA || r.Type == RoomType.HczEzCheckpointB) > 0))
                                    {
                                        if (!basicDoor.IsLocked) basicDoor.ChangeLock(Exiled.API.Enums.DoorLockType.AdminCommand);
                                        if (!basicDoor.IsOpen) basicDoor.IsOpen = true;
                                    } else
                                    {
                                        try
                                        {
                                            List<RoomType> lockDoorRooms = new List<RoomType>(){
                                                RoomType.HczEzCheckpointA,
                                                RoomType.HczEzCheckpointB
                                            };
                                            List<RoomType> lockInternalDoorRooms = new List<RoomType>(){
                                                RoomType.Hcz096,
                                                RoomType.Hcz079
                                            };
                                            List<RoomType> lockDoorOpenRooms = new List<RoomType>(){
                                                RoomType.LczClassDSpawn
                                            };
                                            foreach (RoomType lockRoom in lockDoorRooms)
                                            {
                                                if (basicDoor.Rooms.ToList().Count(r => r.Type == lockRoom) > 0)
                                                {
                                                    if (!basicDoor.IsLocked) basicDoor.ChangeLock(Exiled.API.Enums.DoorLockType.AdminCommand);
                                                    if (basicDoor.IsOpen) basicDoor.IsOpen = false;
                                                }
                                            }
                                            foreach (RoomType lockRoom in lockInternalDoorRooms)
                                            {
                                                if (basicDoor.Rooms.ToList().Count(r => r.Type == lockRoom) > 0 && basicDoor.Rooms.ToList().Count() < 2)
                                                {
                                                    if (!basicDoor.IsLocked) basicDoor.ChangeLock(Exiled.API.Enums.DoorLockType.AdminCommand);
                                                    if (basicDoor.IsOpen) basicDoor.IsOpen = false;
                                                }
                                            }
                                            foreach (RoomType lockRoom in lockDoorOpenRooms)
                                            {
                                                if (basicDoor.Rooms.ToList().Count(r => r.Type == lockRoom) > 0 && basicDoor.Rooms.ToList().Count() < 2)
                                                {
                                                    if (!basicDoor.IsLocked) basicDoor.ChangeLock(Exiled.API.Enums.DoorLockType.AdminCommand);
                                                    if (!basicDoor.IsOpen) basicDoor.IsOpen = true;
                                                }
                                            }
                                        }
                                        catch (Exception err) { }
                                    }
                                }
                            }

                            // Disable decontam
                            DecontaminationController.Singleton.NetworkDecontaminationOverride = DecontaminationStatus.Disabled;

                            // Generate random items in each room
                            UniqueItemGeneration();

                            // TEST SOUND
                            SoundHandler.PlayAudio("music.ogg", 5, true, $"{SCProphunt.Instance.Config.PluginTranslations.prophunt}", Vector3.zero);
                        }
                    } 
                    if (!SCProphunt.Instance.PropHuntRound.StillInLobby) {
                        string bcContent = $"<u><size=30><color=#31bcf7>{ProcessTranslation("{props} " +SCProphunt.Instance.Config.PluginTranslations.props)}</color></size> - <b>{SCProphunt.Instance.Config.PluginTranslations.prophunt}</b> -  <size=30><color=#f7c331>{ProcessTranslation("{hunters} " + SCProphunt.Instance.Config.PluginTranslations.hunters)}</color></size></u> ";

                        // Round End / Winners
                        if (SCProphunt.Instance.PropHuntRound.TimeFinished < CurrentTime || ((SCProphunt.Instance.PropHuntRound.PropHuntTeam.Values.Count(n => n == "prop") == 0 || SCProphunt.Instance.PropHuntRound.PropHuntTeam.Values.Count(n => n == "hunter") == 0) && !SCProphunt.Instance.Config.Debug))
                        {
                            foreach (string userId in SCProphunt.Instance.PropHuntRound.PropHuntTeam.Keys)
                            {
                                Player player = Player.Get(userId);
                                if (player == null) continue;
                                if (player.IsNPC) continue;
                                if (!player.IsGodModeEnabled)
                                {
                                    player.IsGodModeEnabled = true;
                                }
                            }
                            if (SCProphunt.Instance.PropHuntRound.TimeFinished > CurrentTime)
                            {
                                SCProphunt.Instance.PropHuntRound.TimeFinished = CurrentTime;
                            }
                            if (SCProphunt.Instance.PropHuntRound.PropHuntTeam.Values.Count(n => n == "prop") == 0)
                            {
                                bcContent += $"\n<size=30><color=#f7c331>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.broadcast_hunter_win)}</color></size>";
                            }
                            else
                            {
                                bcContent += $"\n<size=30><color=#31bcf7>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.broadcast_prop_win)}</color></size>";
                            }
                            if (!SCProphunt.Instance.PropHuntRound.RoundEnded)
                            {
                                SCProphunt.Instance.PropHuntRound.RoundEnded = true;
                                SoundHandler.PlayAudio($"win.ogg", 15, false, ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.round_end), Vector3.zero, 5f);
                            }
                        }
                        else
                        {
                            if (CurrentTime - SCProphunt.Instance.PropHuntRound.TimeStarted < (30 * 1000))
                            {
                                bcContent += $"\n <size=30>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.broadcast_until_hunter_release)}</size>";
                            }
                            else
                            {
                                // RELEASE THE HUNTERRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRRS
                                if (!SCProphunt.Instance.PropHuntRound.HuntersReleased)
                                {
                                    SCProphunt.Instance.PropHuntRound.KillFeed.Add($"<size=25>{SCProphunt.Instance.Config.PluginTranslations.killfeed_hunters_released}</size>", CurrentTime);
                                    SCProphunt.Instance.PropHuntRound.HuntersReleased = true;
                                    foreach (KeyValuePair<string, string> playerEntry in SCProphunt.Instance.PropHuntRound.PropHuntTeam)
                                    {
                                        if (playerEntry.Value == "hunter")
                                        {
                                            Player.Get(playerEntry.Key).Position = Room.Get(SCProphunt.Instance.PropHuntRound.ChosenMap == ZoneType.LightContainment ? RoomType.LczClassDSpawn : (SCProphunt.Instance.PropHuntRound.ChosenMap == ZoneType.HeavyContainment ? RoomType.Hcz939 : RoomType.EzGateA)).Position + new UnityEngine.Vector3(0f, 1.5f, 0f);
                                        }
                                    }
                                }
                                bcContent += $"\n <size=30>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.broadcast_time_left)}</size>";
                            }
                        }

                        // Taunts
                        if (SCProphunt.Instance.Config.EnableTaunts)
                        {
                            if (CurrentTime - SCProphunt.Instance.PropHuntRound.LastTaunt > (long)(SCProphunt.Instance.Config.TauntCooldown * 1000))
                            {
                                SCProphunt.Instance.PropHuntRound.LastTaunt = CurrentTime;
                                // Do taunt
                                SCProphunt.Instance.PropHuntRound.KillFeed.Add($"<size=25>[ ℹ ] Props have taunted!</size>", CurrentTime);
                                foreach (KeyValuePair<string, string> user in SCProphunt.Instance.PropHuntRound.PropHuntTeam)
                                {
                                    if (user.Value == "prop")
                                    {
                                        Player player = Player.Get(user.Key);
                                        Props.Taunt(player);
                                    }
                                }
                            }
                            bcContent += $"\n <size=28>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.broadcast_next_taunt)}</size>";
                        }

                        // Infinite Ammo for Hunters + Replace grenade with coin
                        foreach (Player player in Player.List)
                        {
                            if (player == null) continue;
                            if (player.IsNPC) continue;
                            if (SCProphunt.Instance.PropHuntRound.PropHuntTeam.ContainsKey(player.UserId))
                            {
                                if (SCProphunt.Instance.PropHuntRound.PropHuntTeam[player.UserId] == "hunter")
                                {
                                    player.SetAmmo(Exiled.API.Enums.AmmoType.Nato9, 128);

                                    if (player.Inventory.UserInventory.Items.Values.Count() < 8)
                                    {
                                        player.AddItem(ItemType.Coin);
                                    }
                                }
                            }
                        }

                        // After 10 seconds of round ending, go back to lobby
                        if (CurrentTime > SCProphunt.Instance.PropHuntRound.TimeFinished + (10 * 1000))
                        {
                            EndProphuntRound();
                        }

                        Exiled.API.Features.Broadcast _bc = new Exiled.API.Features.Broadcast(bcContent, 3);
                        Map.ClearBroadcasts();
                        Map.Broadcast(_bc);
                    }
                }
                yield return Timing.WaitForSeconds(1f);
            }
        }

        /// <summary>
        /// Displays the proper Hints to players during a Prophunt Round.
        /// </summary>
        /// <returns></returns>
        public static IEnumerator<float> HandleHints()
        {
            for (; ; )
            {
                if (SCProphunt.Instance.PropHuntRound != null)
                {
                    if (SCProphunt.Instance.PropHuntRound.StillInLobby) {
                        long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

                        // Hint System
                        foreach (Player player in Player.List)
                        {
                            if (player == null) continue;
                            if (player.IsNPC) continue;
                            string hintContent = "";

                            hintContent += $"\n<size=180>\n</size>\n<size=30>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_instruction)}</size>";
                            hintContent += $"\n<size=27>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_current_selection, player)}</size>";
                            hintContent += $"\n<size=24><color=#ffef63>[{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_light_label)} - {ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_vote_count, zone: ZoneType.LightContainment)}]</color> | <color=#4a4a4a>[{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_heavy_label)} - {ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_vote_count, zone: ZoneType.HeavyContainment)}]</color> | <color=#ffac63>[{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_entrance_label)} - {ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_vote_count, zone: ZoneType.Entrance)}]</color></size>";
                            hintContent += $"\n<size=24><color=#ffef63>[{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_light_keycard)}]</color> -> <color=#ffef63>[{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_light_label)}]</color></size>";
                            hintContent += $"\n<size=24><color=#4a4a4a>[{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_heavy_keycard)}]</color> -> <color=#4a4a4a>[{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_heavy_label)}]</color></size>";
                            hintContent += $"\n<size=24><color=#ff8f2e>[{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_entrance_keycard)}]</color> -> <color=#ffac63>[{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.voting_entrance_label)}]</color></size>";

                            player.ShowHint(hintContent, 1f);
                        }
                    } else
                    {
                        long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

                        // KillFeed
                        List<string> kfKeys = SCProphunt.Instance.PropHuntRound.KillFeed.Keys.ToList();
                        Dictionary<string, long> kfSorted = SCProphunt.Instance.PropHuntRound.KillFeed.OrderBy(x => x.Value).Take(10).ToDictionary(x => x.Key, x => x.Value);
                        string kfContent = "";

                        foreach (KeyValuePair<string, long> feedItem in kfSorted)
                        {
                            kfContent += $"{feedItem.Key}\n";
                        }
                        foreach (string feedItem in kfKeys)
                        {
                            if (SCProphunt.Instance.PropHuntRound.KillFeed[feedItem] + (5 * 1000) < CurrentTime)
                            {
                                SCProphunt.Instance.PropHuntRound.KillFeed.Remove(feedItem);
                            }
                        }

                        List<string> deniedKeys = SCProphunt.Instance.PropHuntRound.DeniedPropPlayers.Keys.ToList();
                        foreach (string deniedItem in deniedKeys)
                        {
                            if (SCProphunt.Instance.PropHuntRound.DeniedPropPlayers[deniedItem] + (5 * 1000) < CurrentTime)
                            {
                                SCProphunt.Instance.PropHuntRound.DeniedPropPlayers.Remove(deniedItem);
                            }
                        }

                        // Hint System
                        foreach (Player player in Player.List)
                        {
                            if (player.IsNPC) continue;
                            string hintContent = $"<size=25><align=\"right\">{kfContent}\n";

                            hintContent += $"</size><align=\"center\">\n<size={475 - (SCProphunt.Instance.PropHuntRound.KillFeed.Count() * 30)}>\n</size>\n";

                            // Prop UI
                            if (SCProphunt.Instance.PropHuntRound.PropHuntTeam.ContainsKey(player.UserId))
                            {
                                if (SCProphunt.Instance.PropHuntRound.PropHuntTeam[player.UserId] == "prop" && SCProphunt.Instance.PropDictionary.TryGetValue(player.UserId, out PropItemComponent propItem))
                                {
                                    // Denied Prop Message
                                    if (SCProphunt.Instance.PropHuntRound.DeniedPropPlayers.ContainsKey(player.UserId))
                                    {
                                        hintContent += $"<size=40><color=#f54842>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.prop_ui_prohibited)}</color></size>\n";
                                    }
                                    else
                                    {
                                        hintContent += "<size=40> </size>\n";
                                    }
                                    hintContent += $"<size=30>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.prop_ui_rotation_control_hint)}</size>";
                                    // Show Prop Hint for Rotation
                                    if (propItem.rotationLock)
                                    {
                                        hintContent += $"\n<size=20><color=#f54842>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.prop_ui_rotation_locked)}</color></size>";
                                    }
                                    else
                                    {
                                        hintContent += $"\n<size=20><color=#63f542>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.prop_ui_rotation_unlocked)}</color></size>";
                                    }

                                    // Take care of Taunt Timing
                                    if (SCProphunt.Instance.PropHuntRound.TauntedPropPlayers.ContainsKey(player.UserId))
                                    {
                                        if (CurrentTime > SCProphunt.Instance.PropHuntRound.TauntedPropPlayers[player.UserId] + (long)(SCProphunt.Instance.Config.ManualTauntCooldown * 1000))
                                        {
                                            SCProphunt.Instance.PropHuntRound.TauntedPropPlayers.Remove(player.UserId);
                                        }
                                    }

                                    // TAUNT MESSAGE
                                    if (SCProphunt.Instance.PropHuntRound.TauntedPropPlayers.ContainsKey(player.UserId))
                                    {
                                        hintContent += $"\n<size=20>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.prop_ui_taunt_hint)} (<color=#f54842>{BeautifyTime(CurrentTime, SCProphunt.Instance.PropHuntRound.TauntedPropPlayers[player.UserId] + (long)(SCProphunt.Instance.Config.ManualTauntCooldown * 1000))}!</color>)</size>\n";
                                    }
                                    else
                                    {
                                        hintContent += $"\n<size=20>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.prop_ui_taunt_hint)} (<color=#63f542>{ProcessTranslation(SCProphunt.Instance.Config.PluginTranslations.prop_ui_taunt_ready)}</color>)</size>\n";
                                    }
                                }
                            }
                            hintContent += $"</size>\n<size=180>\n</size>\n";

                            player.ShowHint(hintContent, 1f);
                        }
                    }
                }
                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        /// <summary>
        /// Converts the timer to a more readible format based on the current time and the end time
        /// </summary>
        /// <param name="curTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        private static string BeautifyTime(long curTime, long endTime)
        {
            long timeLeft = endTime - curTime;
            TimeSpan t = TimeSpan.FromMilliseconds(timeLeft);
            return string.Format("{0:D2}:{1:D2}s",
                                    t.Minutes + (t.Hours * 60),
                                    t.Seconds);
        }

        /// <summary>
        /// Starts a Prophunt Round with the given round length
        /// </summary>
        /// <param name="lengthSeconds"></param>
        public static void StartProphuntRound(int lengthSeconds)
        {
            // Create new Round Object
            SCProphunt.Instance.PropHuntRound = new PropHuntRoundInfo();

            // Set all players to tutorial 
            foreach (Player _player in Player.List)
            {
                if (_player == null) continue;
                if (_player.IsNPC) continue;
                _player.Role.Set(RoleTypeId.Tutorial);
                _player.Scale = Vector3.one;
                _player.Health = 100;

                if (_player.TryGetEffect(EffectType.Disabled, out StatusEffectBase statusEffect))
                {
                    byte newValue = (byte)Mathf.Min(255, 0);

                    _player.ReferenceHub.playerEffectsController.ChangeState(statusEffect.GetType().Name, newValue, 0);
                }

                Timing.CallDelayed(0.1f, delegate
                {
                    _player.ClearInventory();
                });
                Timing.CallDelayed(0.2f, delegate
                {
                    _player.AddItem(ItemType.KeycardScientist);
                });
                Timing.CallDelayed(0.3f, delegate
                {
                    _player.AddItem(ItemType.KeycardGuard);
                });
                Timing.CallDelayed(0.4f, delegate
                {
                    _player.AddItem(ItemType.KeycardResearchCoordinator);
                });
            }

            // Set the Target Round Time
            SCProphunt.Instance.PropHuntRound.VotingTimeStart = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            SCProphunt.Instance.PropHuntRound.VotingTimeFinish = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds + (30 * 1000);

            SCProphunt.Instance.PropHuntRound.TimeStarted = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds + (35 * 1000);
            SCProphunt.Instance.PropHuntRound.TimeFinished = SCProphunt.Instance.PropHuntRound.TimeStarted + (lengthSeconds * 1000);
        }

        /// <summary>
        /// Ends the current ongoing Prophunt Round
        /// </summary>
        public static void EndProphuntRound()
        {
            // Remove Broadcast
            Map.ClearBroadcasts();

            // Bring Back items
            RestoreItems();

            // Remove all Prop Objects
            foreach (PropItemComponent _pic in SCProphunt.Instance.PropDictionary.Values)
            {
                NetworkServer.Destroy(_pic.item.gameObject);
            }
            SCProphunt.Instance.PropDictionary = new Dictionary<string, PropItemComponent>();

            // Store the Last Round Teams so that the Players can run two rounds and play both roles
            SCProphunt.Instance.LastRoundTeams = SCProphunt.Instance.PropHuntRound.PropHuntTeam;

            // Remove the Round Object
            SCProphunt.Instance.PropHuntRound = null;

            // Set all players to tutorial 
            foreach (Player _player in Player.List)
            {
                if (_player == null) continue;
                if (_player.IsNPC) continue;
                _player.Role.Set(RoleTypeId.Tutorial);
                _player.Scale = Vector3.one;
                _player.Health = 100;

                if (_player.TryGetEffect(EffectType.Disabled, out StatusEffectBase statusEffect))
                {
                    byte newValue = (byte)Mathf.Min(255, 0);

                    _player.ReferenceHub.playerEffectsController.ChangeState(statusEffect.GetType().Name, newValue, 0);
                }
            }

            // Stop all sounds
            SoundHandler.StopAudio();
        }

        /// <summary>
        /// Spawns random Items around the Map meant to confuse Hunters
        /// </summary>
        public static void UniqueItemGeneration()
        {
            // Take Care of Original Items
            SCProphunt.Instance.PropHuntRound.CachedProps.Clear();
            foreach (Pickup item in Pickup.List.ToList())
            {
                SCProphunt.Instance.PropHuntRound.CachedProps.Add(new NonPropCache(){
                    Position = item.Position,
                    Rotation = item.Rotation,
                    Scale = item.Scale,
                    PickupSyncInfo = item.Info,
                    ItemType = item.Type
                });
            }

            // Add New Items
            System.Random random = new System.Random();

            List<RoomType> LczRooms = new List<RoomType> {
                RoomType.Lcz173,
                RoomType.Lcz914,
                RoomType.Lcz330,
                RoomType.LczArmory,
                RoomType.LczCafe,
                RoomType.LczCheckpointA,
                RoomType.LczCheckpointB,
                RoomType.LczCrossing,
                RoomType.LczCurve,
                RoomType.LczGlassBox,
                RoomType.LczPlants,
                RoomType.LczStraight,
                RoomType.LczTCross,
                RoomType.LczToilets,
            };

            List<RoomType> HczRooms = new List<RoomType> {
                RoomType.Hcz939,
                RoomType.Hcz079,
                RoomType.Hcz106,
                RoomType.HczArmory,
                RoomType.HczCrossing,
                RoomType.HczCurve,
                RoomType.HczElevatorA,
                RoomType.HczElevatorB,
                RoomType.HczEzCheckpointA,
                RoomType.HczEzCheckpointB,
                RoomType.HczHid,
                RoomType.HczNuke,
                RoomType.HczServers,
                RoomType.HczStraight,
                RoomType.HczTCross,
                RoomType.HczTesla,
                RoomType.HczTestRoom
            };

            List<RoomType> EntranceRooms = new List<RoomType> {
                RoomType.EzCheckpointHallway,
                RoomType.EzCollapsedTunnel,
                RoomType.EzCrossing,
                RoomType.EzCurve,
                RoomType.EzDownstairsPcs,
                RoomType.EzGateA,
                RoomType.EzGateB,
                RoomType.EzIntercom,
                RoomType.EzPcs,
                RoomType.EzShelter,
                RoomType.EzStraight,
                RoomType.EzTCross,
                RoomType.EzUpstairsPcs,
                RoomType.EzVent
            };

            foreach (Room room in Room.List)
            {
                if ((SCProphunt.Instance.PropHuntRound.ChosenMap == ZoneType.LightContainment && !LczRooms.Contains(room.Type)) || (SCProphunt.Instance.PropHuntRound.ChosenMap == ZoneType.HeavyContainment && !HczRooms.Contains(room.Type)) || (SCProphunt.Instance.PropHuntRound.ChosenMap == ZoneType.Entrance && !EntranceRooms.Contains(room.Type))) continue;
                int numItems = random.Next(12) + 4;
                int playerCount = SCProphunt.Instance.PropHuntRound.PropHuntTeam.Count();
                if (playerCount > 8)
                {
                    numItems += (int)Math.Round((playerCount - 8) * SCProphunt.Instance.Config.PropCompensationRatio);
                }
                for (int i = 0; i < numItems; i++)
                {
                    Vector3 offsetPos = new Vector3((float)(random.NextDouble() * 10) - 5, 1.5f, (float)(random.NextDouble() * 10) - 5);
                    ItemPickupBase prop = Props.CreateRandomPropItem(room.Position + offsetPos, Quaternion.Euler((float)(random.NextDouble() * 360), (float)(random.NextDouble() * 360), (float)(random.NextDouble() * 360)));
                    SCProphunt.Instance.RandomlySpawnedItems.Add(prop);
                }
            }
        }

        /// <summary>
        /// Handles items after a game ends. Removes prop items, and at some point should return original items to their position.
        /// </summary>
        public static void RestoreItems()
        {
            foreach (ItemPickupBase item in SCProphunt.Instance.RandomlySpawnedItems)
            {
                try
                {
                    NetworkServer.Destroy(item.gameObject);
                }
                catch (Exception e) { }
            }
            SCProphunt.Instance.RandomlySpawnedItems.Clear();

            Map.CleanAllRagdolls();
            Map.CleanAllItems();

            // Take Care of Original Items
            foreach (NonPropCache item in SCProphunt.Instance.PropHuntRound.CachedProps)
            {
                Pickup newItem = Pickup.CreateAndSpawn(item.ItemType,item.Position,item.Rotation);
                newItem.Scale = item.Scale;
                newItem.Position = item.Position;
                newItem.Rotation = item.Rotation;
                newItem.Info = item.PickupSyncInfo;
            }
            SCProphunt.Instance.PropHuntRound.CachedProps.Clear();
        }

        /// <summary>
        /// Deals with Prophunt Team status changing, giving each role their items
        /// </summary>
        /// <param name="player"></param>
        /// <param name="team"></param>
        public static void HandlePlayerTeamChange(Player player, string team)
        {
            long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

            if (SCProphunt.Instance.PropHuntRound.KilledPlayers.Contains(player.UserId)) SCProphunt.Instance.PropHuntRound.KilledPlayers.Remove(player.UserId);

            switch (team)
            {
                case "prop":
                    player.Role.Set(RoleTypeId.ClassD, RoleSpawnFlags.AssignInventory);

                    Timing.CallDelayed(0.5f, delegate
                    {
                        player.ClearInventory();
                        player.AddItem(ItemType.GrenadeFlash);

                    });

                    player.ChangeAppearance(RoleTypeId.Spectator, true, 0);

                    player.Position = Room.Get(SCProphunt.Instance.PropHuntRound.ChosenMap == ZoneType.LightContainment ? RoomType.LczClassDSpawn : (SCProphunt.Instance.PropHuntRound.ChosenMap == ZoneType.HeavyContainment ? RoomType.Hcz939 : RoomType.EzGateA)).Position + new UnityEngine.Vector3(0f, 1.5f, 0f);
                    player.IsGodModeEnabled = false;

                    if (player.TryGetEffect(EffectType.Disabled, out StatusEffectBase statusEffect))
                    {
                        byte newValue = (byte)Mathf.Min(255, 3);

                        player.ReferenceHub.playerEffectsController.ChangeState(statusEffect.GetType().Name, newValue, 0);
                    }

                    Props.SwitchToProp(player, ItemType.Medkit);

                    //if (!SCProphunt.Instance.PropDictionary.ContainsKey(player.UserId))
                    //{
                    //    SwitchToProp(Player player, ItemType itemType);
                    //    //SCProphunt.Instance.PropDictionary.Add(player.UserId, Props.SpawnPropItem(player, new Types.PropInfo(ItemType.Medkit)));
                    //}
                    break;
                case "hunter":
                    player.Role.Set(PlayerRoles.RoleTypeId.FacilityGuard, RoleSpawnFlags.AssignInventory);

                    Timing.CallDelayed(0.5f, delegate
                    {
                        player.ClearInventory();
                        player.AddItem(ItemType.GunCOM18);
                        player.AddItem(ItemType.GunFSP9);
                        player.AddItem(ItemType.GrenadeHE);
                        player.AddItem(ItemType.ArmorCombat);
                        player.AddItem(ItemType.Flashlight);
                        player.AddItem(ItemType.Coin);
                        player.AddItem(ItemType.Coin);
                        player.AddItem(ItemType.Coin);
                        player.SetAmmo(Exiled.API.Enums.AmmoType.Nato9, 128);
                    });

                    player.Health = SCProphunt.Instance.Config.HunterHP;

                    if (CurrentTime - SCProphunt.Instance.PropHuntRound.TimeStarted > (30 * 1000))
                    {
                        player.Position = Room.Get(SCProphunt.Instance.PropHuntRound.ChosenMap == ZoneType.LightContainment ? RoomType.LczClassDSpawn : (SCProphunt.Instance.PropHuntRound.ChosenMap == ZoneType.HeavyContainment ? RoomType.Hcz939 : RoomType.EzGateA)).Position + new UnityEngine.Vector3(0f,1.5f,0f);
                    } else
                    {
                        player.Position = Room.Get(Exiled.API.Enums.RoomType.Hcz096).Position + new UnityEngine.Vector3(0f, 1.5f, 0f);
                    }

                    player.IsGodModeEnabled = false;

                    player.Scale = Vector3.one;

                    if (player.TryGetEffect(EffectType.Disabled, out StatusEffectBase statusEffectDos))
                    {
                        byte newValue = (byte)Mathf.Min(255, 0);

                        player.ReferenceHub.playerEffectsController.ChangeState(statusEffectDos.GetType().Name, newValue, 0);
                    }

                    if (SCProphunt.Instance.PropDictionary.ContainsKey(player.UserId))
                    {
                        NetworkServer.Destroy(SCProphunt.Instance.PropDictionary[player.UserId].item.gameObject);
                        SCProphunt.Instance.PropDictionary.Remove(player.UserId);
                    }
                    break;
                default:
                    player.Scale = Vector3.one;

                    if (player.TryGetEffect(EffectType.Disabled, out StatusEffectBase statusEffectTres))
                    {
                        byte newValue = (byte)Mathf.Min(255, 0);

                        player.ReferenceHub.playerEffectsController.ChangeState(statusEffectTres.GetType().Name, newValue, 0);
                    }

                    player.Role.Set(PlayerRoles.RoleTypeId.Spectator);
                    if (SCProphunt.Instance.PropDictionary.ContainsKey(player.UserId))
                    {
                        NetworkServer.Destroy(SCProphunt.Instance.PropDictionary[player.UserId].item.gameObject);
                        SCProphunt.Instance.PropDictionary.Remove(player.UserId);
                    }
                    break;
            }
        }
    }
}
