using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using MEC;
using Mirror;
using PlayerRoles;
using SCProphunt.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.Core.Tokens;

namespace SCProphunt
{
    public class Props
    {
        /// <summary>
        /// Fixes a bug where certain Exiled Updates breaks PickupSyncInfo Locked property
        /// </summary>
        /// <returns></returns>
        public static IEnumerator<float> LockProps()
        {
            for (; ; )
            {
                foreach (PropItemComponent _propItem in SCProphunt.Instance.PropDictionary.Values)
                {
                    try
                    {
                        var pickupInfo = _propItem.item.NetworkInfo;
                        pickupInfo.Locked = true;
                        _propItem.item.NetworkInfo = pickupInfo;

                        _propItem.player.MoveProp();
                    }
                    catch (Exception e) { }
                }
                yield return Timing.WaitForSeconds(0.025f);
            }
        }

        /// <summary>
        /// Spawns the Prop Item and attaches it to the player passed through
        /// </summary>
        /// <param name="player"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static PropItemComponent SpawnPropItem(Player player, PropInfo prop)
        {
            if (prop.Item == ItemType.None) return null;

            var itemOffset = new Vector3(0,-0.25f);
            var rot = Quaternion.Euler(0, 0, 0);
            var scale = Vector3.one;
            var item = prop.Item;

            // TODO: Fix this when whatever NW's change is figured out.
            //if (item == ItemType.MicroHID || item == ItemType.Ammo9x19 || item == ItemType.Ammo12gauge ||
            //    item == ItemType.Ammo44cal || item == ItemType.Ammo556x45 || item == ItemType.Ammo762x39)
            //{
            //    return null;
            //}

            //switch (item)
            //{
            //    case ItemType.KeycardScientist:
            //        scale += new Vector3(1.5f, 20f, 1.5f);
            //        rot = Quaternion.Euler(0, 90, 0);
            //        itemOffset = new Vector3(0, .1f, 0);
            //        break;

            //    case ItemType.KeycardNTFCommander:
            //        scale += new Vector3(1.5f, 200f, 1.5f);
            //        rot = Quaternion.Euler(0, 90, 0);
            //        itemOffset = new Vector3(0, .9f, 0);
            //        break;

            //    case ItemType.SCP268:
            //        scale += new Vector3(-.1f, -.1f, -.1f);
            //        rot = Quaternion.Euler(-90, 0, 90);
            //        itemOffset = new Vector3(0, 0, .1f);
            //        break;

            //    case ItemType.Adrenaline:
            //    case ItemType.Medkit:
            //    case ItemType.Coin:
            //    case ItemType.SCP018:
            //        itemOffset = new Vector3(0, .1f, 0);
            //        break;

            //    case ItemType.SCP500:
            //        itemOffset = new Vector3(0, .075f, 0);
            //        break;

            //    case ItemType.SCP207:
            //        itemOffset = new Vector3(0, .225f, 0);
            //        rot = Quaternion.Euler(-90, 0, 0);
            //        break;
            //}

            if (prop.Scale != Vector3.one) scale = prop.Scale;
            if (prop.Position != Vector3.zero) itemOffset = prop.Position;
            if (prop.Rotation != Quaternion.identity) rot = prop.Rotation;
            if (prop.Scale != Vector3.one || prop.Position != Vector3.zero || prop.Rotation != Quaternion.identity) item = prop.Item;

            var itemModel = InventoryItemLoader.AvailableItems[item];

            var psi = new PickupSyncInfo()
            {
                ItemId = item,
                WeightKg = itemModel.Weight,
                Serial = ItemSerialGenerator.GenerateNext(),
                Locked = true
            };

            var pickup = UnityEngine.Object.Instantiate(itemModel.PickupDropModel, Vector3.zero, Quaternion.identity);
            pickup.transform.localScale = scale;
            pickup.NetworkInfo = psi;

            NetworkServer.Spawn(pickup.gameObject);
            pickup.gameObject.transform.localScale = Vector3.one * 2.5f;
            return SpawnPropItem(player, pickup, itemOffset, rot, prop.Item);
        }

        /// <summary>
        /// Secondary method used for attaching the Prop to the Player
        /// </summary>
        /// <param name="player"></param>
        /// <param name="pickup"></param>
        /// <param name="posOffset"></param>
        /// <param name="rotOffset"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public static PropItemComponent SpawnPropItem(Player player, ItemPickupBase pickup, Vector3 posOffset, Quaternion rotOffset, ItemType itemType)
        {
            PropPlayerComponent playerComponent;

            if (!player.GameObject.TryGetComponent(out playerComponent))
            {
                playerComponent = player.GameObject.AddComponent<PropPlayerComponent>();
            }

            if (playerComponent.item != null)
            {
                UnityEngine.Object.Destroy(playerComponent.item.gameObject);
                playerComponent.item = null;
            }

            var rigidbody = pickup.gameObject.GetComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;

            playerComponent.item = pickup.gameObject.AddComponent<PropItemComponent>();
            playerComponent.item.item = pickup;
            playerComponent.item.player = playerComponent;
            playerComponent.item.pos = new Vector3(0f, 0f, 0f);
            playerComponent.item.itemOffset = posOffset;
            playerComponent.item.rot = rotOffset;
            playerComponent.item.rotationLock = false;
            playerComponent.item.ownerUserId = player.UserId;
            playerComponent.item.itemType = itemType;

            return playerComponent.item;
        }

        /// <summary>
        /// Switches a player's prop to a new item of the given type.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemType"></param>
        public static void SwitchToProp(Player player, ItemType itemType)
        {
            float maxHealth;
            if (SCProphunt.Instance.PropDictionary.ContainsKey(player.UserId))
            {
                maxHealth = GetPropInfoConfig(SCProphunt.Instance.PropDictionary[player.UserId].itemType).PlayerHP;
                NetworkServer.Destroy(SCProphunt.Instance.PropDictionary[player.UserId].item.gameObject);
                SCProphunt.Instance.PropDictionary.Remove(player.UserId);
            } else
            {
                maxHealth = 100;
                player.Health = 100;
            }

            // Get Prop Info
            PropInfo propInfo = GetPropInfoConfig(itemType);

            player.Health = (player.Health / maxHealth) * propInfo.PlayerHP;

            player.ReferenceHub.transform.localScale = new Vector3(0f,0f,0f);
            foreach (Player item in Player.List)
            {
                if (player.UserId != item.UserId)
                {
                    Server.SendSpawnMessage?.Invoke(null, new object[2] { player.NetworkIdentity, item.Connection });
                }
            }
            player.ReferenceHub.transform.localScale = propInfo.PlayerScale;
            Server.SendSpawnMessage?.Invoke(null, new object[2] { player.NetworkIdentity, player.Connection });
            //player.Scale = propInfo.PlayerScale;

            SCProphunt.Instance.PropDictionary.Add(player.UserId, Props.SpawnPropItem(player, propInfo));
        }

        /// <summary>
        /// Get the Prop Positioning Offset relative to the player based on Item -> Category -> Default in Config
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        public static PropInfo GetPropInfoConfig(ItemType itemType)
        {
            // Get Prop Info
            PropInfo propInfo;
            if (SCProphunt.Instance.Config.SpecificPropInformation.TryGetValue(itemType, out PropConfig _specProp))
            {
                // Specific Prop
                propInfo = new PropInfo(itemType, _specProp.Scale, _specProp.Position, Quaternion.Euler(_specProp.Rotation), _specProp.PlayerScale, _specProp.PlayerHP);
            }
            else
            {
                if (SCProphunt.Instance.Config.CategoryPropInformation.TryGetValue(itemType.GetCategory(), out PropConfig _specCatProp))
                {
                    // Category of Prop
                    propInfo = new PropInfo(itemType, _specCatProp.Scale, _specCatProp.Position, Quaternion.Euler(_specCatProp.Rotation), _specCatProp.PlayerScale, _specCatProp.PlayerHP);
                }
                else
                {
                    // Default Prop
                    PropConfig _defProp = SCProphunt.Instance.Config.DefaultPropInformation;
                    propInfo = new PropInfo(itemType, _defProp.Scale, _defProp.Position, Quaternion.Euler(_defProp.Rotation), _defProp.PlayerScale, _defProp.PlayerHP);
                }
            }

            return propInfo;
        }

        /// <summary>
        /// Removes props from the Non-FPC roles
        /// </summary>
        /// <param name="_rtid"></param>
        /// <returns></returns>
        public static bool ShouldRemovePropItem(RoleTypeId _rtid)
        {
            return PlayerRoles.RoleTypeId.Scp079 == _rtid || _rtid == PlayerRoles.RoleTypeId.None || _rtid == PlayerRoles.RoleTypeId.Spectator || _rtid == PlayerRoles.RoleTypeId.Overwatch || _rtid == PlayerRoles.RoleTypeId.Filmmaker;
        }

        /// <summary>
        /// Spawn a random Prop Item into the map. Prop items are tracked and monitored.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static ItemPickupBase CreateRandomPropItem(Vector3 position, Quaternion rotation)
        {
            Array values = Enum.GetValues(typeof(ItemType));
            System.Random random = new System.Random();
            ItemType item = ItemType.None;

            while (!(item != ItemType.None && item != ItemType.Coin && !SCProphunt.Instance.Config.ProhibitedPropTypes.Contains(item)))
            {
                item = (ItemType)values.GetValue(random.Next(values.Length));
            }

            var itemModel = InventoryItemLoader.AvailableItems[item];

            var psi = new PickupSyncInfo()
            {
                ItemId = item,
                WeightKg = itemModel.Weight,
                Serial = ItemSerialGenerator.GenerateNext()
            };

            var pickup = UnityEngine.Object.Instantiate(itemModel.PickupDropModel, position, rotation);
            pickup.transform.localScale = Vector3.one;
            pickup.NetworkInfo = psi;

            NetworkServer.Spawn(pickup.gameObject);
            return pickup;
        }

        /// <summary>
        /// Plays a Prop Taunt sound at the player's location
        /// </summary>
        /// <param name="player"></param>
        public static void Taunt(Player player) {
            if (SCProphunt.Instance.PropHuntRound.TauntedPropPlayers.ContainsKey(player.UserId)) { return; }
            long CurrentTime = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
            System.Random random = new System.Random();
            SoundHandler.PlayAudio($"taunts/{1 + random.Next(SCProphunt.Instance.Config.NumberOfTaunts)}.ogg", 15, false, "TauntSoundEffect", player.GameObject.transform.position, 5f);
            SCProphunt.Instance.PropHuntRound.TauntedPropPlayers.Add(player.UserId, CurrentTime);
        }
    }
}
