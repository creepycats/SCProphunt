using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using System.Collections.Generic;
using SCProphunt.Types;
using Mirror;
using PlayerRoles;
using System;
using System.Linq;
using PlayerRoles.FirstPersonControl;
using MEC;
using UnityEngine;
using Exiled.API.Features.Items;
using InventorySystem.Items.Pickups;
using Exiled.API.Features.DamageHandlers;
using PluginAPI.Core.Interfaces;
using Exiled.API.Enums;

namespace SCProphunt.handlers
{
    public class playerHandler
    {
        public void ChangingRole(ChangingRoleEventArgs args)
        {
            if (SCProphunt.Instance.PropHuntRound == null) return;
            if (args.Player.UserId == null) return;
            if (args.Player.IsNPC) return;
            if (args.Reason == Exiled.API.Enums.SpawnReason.Died) return;

            try
            {
                if (Props.ShouldRemovePropItem(args.NewRole))
                {
                    // Remove from game
                    if (SCProphunt.Instance.PropHuntRound.PropHuntTeam.ContainsKey(args.Player.UserId))
                    {
                        SCProphunt.Instance.PropHuntRound.PropHuntTeam.Remove(args.Player.UserId);
                    }

                    if (SCProphunt.Instance.PropDictionary.TryGetValue(args.Player.UserId, out PropItemComponent _foundProp))
                    {
                        SCProphunt.Instance.PropDictionary.Remove(args.Player.UserId);
                        NetworkServer.Destroy(_foundProp.item.gameObject);
                    }
                }
            }
            catch (Exception err)
            {
                if (SCProphunt.Instance.Config.Debug) Log.Error(err);
            }
        }

        public void Died(DiedEventArgs args)
        {
            if (SCProphunt.Instance.PropHuntRound == null) return;
            if (args.Player.UserId == null) return;
            if (args.Player.IsNPC) return;

            try
            {
                // Remove from game
                if (SCProphunt.Instance.PropHuntRound.PropHuntTeam.ContainsKey(args.Player.UserId))
                {
                    if (!SCProphunt.Instance.PropHuntRound.KilledPlayers.Contains(args.Player.UserId))
                    {
                        long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                        if (args.Attacker != null)
                        {
                            if (args.Attacker.Id == args.Player.Id)
                            {
                                SCProphunt.Instance.PropHuntRound.KillFeed.Add(Game.ProcessTranslationKillfeed(SCProphunt.Instance.Config.PluginTranslations.killfeed_explosion_self, args.Player), CurrentTime);
                            }
                            else {
                                SCProphunt.Instance.PropHuntRound.KillFeed.Add(Game.ProcessTranslationKillfeed(SCProphunt.Instance.Config.PluginTranslations.killfeed_found_player, args.Player, args.Attacker), CurrentTime);
                            }
                        } else
                        {
                            SCProphunt.Instance.PropHuntRound.KillFeed.Add(Game.ProcessTranslationKillfeed(SCProphunt.Instance.Config.PluginTranslations.killfeed_death, args.Player), CurrentTime);
                        }
                        SCProphunt.Instance.PropHuntRound.KilledPlayers.Add(args.Player.UserId);
                    }
                    SCProphunt.Instance.PropHuntRound.PropHuntTeam.Remove(args.Player.UserId);
                }

                // Remove any props
                if (SCProphunt.Instance.PropDictionary.TryGetValue(args.Player.UserId, out PropItemComponent _foundProp))
                {
                    SCProphunt.Instance.PropDictionary.Remove(args.Player.UserId);
                    NetworkServer.Destroy(_foundProp.item.gameObject);
                }
            }
            catch (Exception err)
            {
                if (SCProphunt.Instance.Config.Debug) Log.Error(err);
            }
        }

        // Prop Switching Mechanic
        public void SearchingPickup(SearchingPickupEventArgs args)
        {
            if (SCProphunt.Instance.PropHuntRound == null) return;
            if (args.Player.UserId == null) return;
            if (args.Player.IsNPC) return;

            args.IsAllowed = false;
            if (SCProphunt.Instance.PropHuntRound.PropHuntTeam.ContainsKey(args.Player.UserId))
            {
                if (SCProphunt.Instance.PropHuntRound.PropHuntTeam[args.Player.UserId] == "prop")
                {
                    if (SCProphunt.Instance.Config.ProhibitedPropTypes.Contains(args.Pickup.Type) || args.Pickup.Type == ItemType.Coin)
                    {
                        long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                        if (!SCProphunt.Instance.PropHuntRound.DeniedPropPlayers.ContainsKey(args.Player.UserId))
                        {
                            SCProphunt.Instance.PropHuntRound.DeniedPropPlayers.Add(args.Player.UserId, CurrentTime);
                        }
                    } else
                    {
                        Props.SwitchToProp(args.Player, args.Pickup.Type);
                    }
                }
            }
        }

        // Prevent Hunters from dropping items and Props from dropping Special Keybinds
        public void DroppingAmmo(DroppingAmmoEventArgs args)
        {
            if (SCProphunt.Instance.PropHuntRound == null) return;
            if (args.Player.UserId == null) return;
            if (args.Player.IsNPC) return;
            args.IsAllowed = false;
        }
        public void DroppingItem(DroppingItemEventArgs args)
        {
            if (SCProphunt.Instance.PropHuntRound == null) return;
            if (args.Player.UserId == null) return;
            if (args.Player.IsNPC) return;
            args.IsAllowed = false;
        }

        // Rotation lock for props
        public void TogglingNoClip(TogglingNoClipEventArgs args)
        {
            if (SCProphunt.Instance.PropHuntRound == null) return;
            if (args.Player.UserId == null) return;
            if (args.Player.IsNPC) return;

            if (SCProphunt.Instance.PropHuntRound.PropHuntTeam.ContainsKey(args.Player.UserId))
            {
                if (SCProphunt.Instance.PropHuntRound.PropHuntTeam[args.Player.UserId] == "prop" && SCProphunt.Instance.PropDictionary.TryGetValue(args.Player.UserId, out PropItemComponent propItem))
                {
                    propItem.rotationLock = !propItem.rotationLock;
                }
            }
        }

        // Calculating hits for props
        public void Shooting(ShootingEventArgs args)
        {
            if (SCProphunt.Instance.PropHuntRound == null) return;
            if (args.Player.UserId == null) return;
            if (args.Player.IsNPC) return;

            // Check what's the player shooting at with a raycast
            Physics.Raycast(args.Player.CameraTransform.position, args.Player.CameraTransform.forward, out RaycastHit raycastHit, 70f, ~(1 << 13 | 1 << 16));

            // Return if the raycast doesn't hit anything (If the collider is null)
            if (raycastHit.collider is null)
                return;

            // Get the GameObject associated to the raycast
            ItemPickupBase itemPickup = raycastHit.transform.gameObject.GetComponentInParent<ItemPickupBase>();
            if (itemPickup != null)
            {
                long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                Item hitItem = Item.Get(itemPickup.NetworkInfo.Serial);
                if (raycastHit.transform.gameObject.TryGetComponent<PropItemComponent>(out PropItemComponent propItemComponent))
                {
                    Player hitPlayer = Player.Get(propItemComponent.ownerUserId);
                    args.Player.ShowHitMarker(1f);
                    if (!hitPlayer.IsGodModeEnabled) hitPlayer.Health -= 5; // Base Damage for any gun
                    if (hitPlayer.Health < 1) {
                        SCProphunt.Instance.PropHuntRound.KilledPlayers.Add(hitPlayer.UserId);
                        args.Player.Health += SCProphunt.Instance.Config.PropKillHP;
                        SCProphunt.Instance.PropHuntRound.KillFeed.Add(Game.ProcessTranslationKillfeed(SCProphunt.Instance.Config.PluginTranslations.killfeed_found_player, hitPlayer, args.Player), CurrentTime);
                        hitPlayer.Kill(Game.ProcessTranslationKillfeed(SCProphunt.Instance.Config.PluginTranslations.killfeed_found_player, hitPlayer, args.Player));
                    }
                } else
                {
                    if (!args.Player.IsGodModeEnabled) args.Player.Health -= 5; // Base Damage for Missing
                    if (args.Player.Health < 1) {
                        SCProphunt.Instance.PropHuntRound.KilledPlayers.Add(args.Player.UserId);
                        SCProphunt.Instance.PropHuntRound.KillFeed.Add(Game.ProcessTranslationKillfeed(SCProphunt.Instance.Config.PluginTranslations.killfeed_cant_find, args.Player), CurrentTime);
                        args.Player.Kill(Game.ProcessTranslationKillfeed(SCProphunt.Instance.Config.PluginTranslations.killfeed_cant_find, args.Player));
                    }
                }
            }
        }

        // Replace the grenade with a coin when hunter throws it
        public void Left(LeftEventArgs args)
        {
            if (SCProphunt.Instance.PropHuntRound == null) return;
            if (args.Player.UserId == null) return;
            if (args.Player.IsNPC) return;

            try
            {
                // Remove from game
                if (SCProphunt.Instance.PropHuntRound.PropHuntTeam.ContainsKey(args.Player.UserId))
                {
                    long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                    SCProphunt.Instance.PropHuntRound.KillFeed.Add(Game.ProcessTranslationKillfeed(SCProphunt.Instance.Config.PluginTranslations.killfeed_disconnect, args.Player), CurrentTime);
                    SCProphunt.Instance.PropHuntRound.PropHuntTeam.Remove(args.Player.UserId);
                }

                // Remove any props
                if (SCProphunt.Instance.PropDictionary.TryGetValue(args.Player.UserId, out PropItemComponent _foundProp))
                {
                    SCProphunt.Instance.PropDictionary.Remove(args.Player.UserId);
                    NetworkServer.Destroy(_foundProp.item.gameObject);
                }
            }
            catch (Exception err)
            {
                if (SCProphunt.Instance.Config.Debug) Log.Error(err);
            }
        }

        // Prevent players from holding coins during the event + Handle Prop Taunting
        public void ChangingItem(ChangingItemEventArgs args)
        {
            if (SCProphunt.Instance.PropHuntRound == null) return;
            if (args.Player.UserId == null) return;
            if (args.Player.IsNPC) return;
            if (args.Item == null) return;

            try
            {
                if (SCProphunt.Instance.PropHuntRound.StillInLobby) {
                    long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                    if (CurrentTime < SCProphunt.Instance.PropHuntRound.VotingTimeFinish)
                    {
                        // Voting
                        if (args.Item.Type == ItemType.KeycardScientist)
                        {
                            // Light Containment
                            if (!SCProphunt.Instance.PropHuntRound.MapChoice.Keys.Contains(args.Player.UserId))
                            {
                                SCProphunt.Instance.PropHuntRound.MapChoice.Add(args.Player.UserId, ZoneType.LightContainment);
                            } else
                            {
                                SCProphunt.Instance.PropHuntRound.MapChoice[args.Player.UserId] = ZoneType.LightContainment;
                            }
                        }
                        if (args.Item.Type == ItemType.KeycardGuard)
                        {
                            // Heavy Containment
                            if (!SCProphunt.Instance.PropHuntRound.MapChoice.Keys.Contains(args.Player.UserId))
                            {
                                SCProphunt.Instance.PropHuntRound.MapChoice.Add(args.Player.UserId, ZoneType.HeavyContainment);
                            }
                            else
                            {
                                SCProphunt.Instance.PropHuntRound.MapChoice[args.Player.UserId] = ZoneType.HeavyContainment;
                            }
                        }
                        if (args.Item.Type == ItemType.KeycardResearchCoordinator)
                        {
                            // Entrance Zone
                            if (!SCProphunt.Instance.PropHuntRound.MapChoice.Keys.Contains(args.Player.UserId))
                            {
                                SCProphunt.Instance.PropHuntRound.MapChoice.Add(args.Player.UserId, ZoneType.Entrance);
                            }
                            else
                            {
                                SCProphunt.Instance.PropHuntRound.MapChoice[args.Player.UserId] = ZoneType.Entrance;
                            }
                        }
                    }
                } else
                {
                    // Prevent Coins from being held
                    if (args.Item.Type == ItemType.Coin)
                    {
                        args.IsAllowed = false;
                    }
                    else
                    {
                        if (SCProphunt.Instance.PropHuntRound.PropHuntTeam.TryGetValue(args.Player.UserId, out string playerTeam))
                        {
                            if (playerTeam == "prop")
                            {
                                // PROPS CANNOT HOLD ITEMS
                                args.IsAllowed = false;

                                // Taunt - PROP ONLY
                                if (args.Item.Type == ItemType.GrenadeFlash)
                                {
                                    long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                                    Props.Taunt(args.Player);
                                }
                            }
                            if (playerTeam == "hunter")
                            {
                                // PREVENTS PLAYERS FROM BLOWING THEMSELVES UP PRE-GAME
                                if (!SCProphunt.Instance.PropHuntRound.HuntersReleased)
                                {
                                    args.IsAllowed = false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                if (SCProphunt.Instance.Config.Debug) Log.Error(err);
            }
        }


        // Props wont trigger Teslas in Heavy
        public void TriggeringTesla(TriggeringTeslaEventArgs args)
        {
            if (SCProphunt.Instance.PropHuntRound == null) return;
            if (args.Player.IsNPC || args.Player.UserId == null) {
                args.IsAllowed = false;
                return;
            }

            if (SCProphunt.Instance.PropHuntRound.PropHuntTeam.Keys.Contains(args.Player.UserId))
            {
                if (SCProphunt.Instance.PropHuntRound.PropHuntTeam[args.Player.UserId] == "prop")
                {
                    args.IsAllowed = false;
                }
            }
        }
    }
}
