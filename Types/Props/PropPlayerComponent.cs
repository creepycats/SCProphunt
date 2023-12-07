﻿using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp096;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles;
using System;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.Items.Pickups;
using Mirror;

namespace SCProphunt.Types
{
    public class PropPlayerComponent : MonoBehaviour
    {
        internal PropItemComponent item;

        private bool _threw = false;
        private float LastVertVel = 0;
        private Vector3 LastPlayerPos = Vector3.zero;

        private void Start()
        {
            //Timing.RunCoroutine(MoveProp().CancelWith(this).CancelWith(gameObject));
        }

        public void MoveProp()//IEnumerator<float> MoveProp()
        {
            //while (true)
            //{
            //    yield return Timing.WaitForSeconds(.1f);

                try
                {
                if (item == null || item.gameObject == null) return;//continue;

                    var player = Player.Get(gameObject);
                    var pickup = item.item;
                    var pickupInfo = pickup.NetworkInfo;
                    var pickupType = pickup.GetType();

                    pickupInfo.Locked = true;

                    if (player.Role == RoleTypeId.None || player.Role == RoleTypeId.Spectator)
                    {
                        pickup.transform.position = Vector3.one * 6000f;
                        //pickupInfo.ServerSetPositionAndRotation(Vector3.one * 6000f, Quaternion.identity);

                        pickup.NetworkInfo = pickupInfo;

                    return;//continue;
                    }

                    var camera = player.CameraTransform;

                    var rotAngles = camera.rotation.eulerAngles;
                    rotAngles.x = 0;

                    var rotation = Quaternion.Euler(rotAngles);

                    var rot = rotation * item.rot;
                    var transform1 = pickup.transform;
                    var pos = (player.Role != RoleTypeId.Scp079 ? rotation * (item.pos + item.itemOffset) : (item.pos + item.itemOffset)) + player.Position;

                    if (player.RoleManager.CurrentRole is IFpcRole fpc)
                    {
                        //pickup.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(fpc.FpcModule.Motor.Velocity.x, player.Velocity.y, fpc.FpcModule.Motor.Velocity.z) * 1f;
                    }
                    transform1.position = pos;
                    if(!item.rotationLock) transform1.rotation = rot;

                //pickupInfo.ServerSetPositionAndRotation(pos, rot);

                    if (pickup.PhysicsModule is PickupStandardPhysics standardPhysics)
                    {
                        pickup.PhysicsModule.ServerSetSyncData(new Action<NetworkWriter>(standardPhysics.ServerWriteRigidbody));
                    }

                    var fakePickupInfo = pickup.NetworkInfo;
                    //fakePickupInfo.ServerSetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    fakePickupInfo.Locked = true;
                    var ownerPickupInfo = pickupInfo;

                    foreach (var player1 in Player.List)
                    {
                        // IsPlayerNPC is 2nd because it has a lot of null checks.
                        if (player1 == null || player1.UserId == null || player1.IsDead) continue;

                        if (player1 == player)
                        {
                            MirrorExtensions.SendFakeSyncVar(player1, pickup.netIdentity, pickupType, "NetworkInfo", ownerPickupInfo);
                        }
                        else if (PlayerRolesUtils.GetTeam(player1.Role) == PlayerRolesUtils.GetTeam(player.Role))
                        {
                            MirrorExtensions.SendFakeSyncVar(player1, pickup.netIdentity, pickupType, "NetworkInfo", pickupInfo);
                        }
                        else if (player1.ReferenceHub.roleManager.CurrentRole is FpcStandardRoleBase role)
                            switch (player1.Role.Type)
                            {
                                case RoleTypeId.Scp939 when role.VisibilityController is Scp939VisibilityController vision && !vision.ValidateVisibility(player.ReferenceHub):
                                    MirrorExtensions.SendFakeSyncVar(player1, pickup.netIdentity, pickupType, "NetworkInfo", fakePickupInfo);
                                    break;
                                case RoleTypeId.Scp096 when role.VisibilityController is Scp096VisibilityController vision && !vision.ValidateVisibility(player.ReferenceHub):
                                    MirrorExtensions.SendFakeSyncVar(player1, pickup.netIdentity, pickupType, "NetworkInfo", fakePickupInfo);
                                    break;
                                default:
                                    MirrorExtensions.SendFakeSyncVar(player1, pickup.netIdentity, pickupType, "NetworkInfo", pickupInfo);
                                    break;
                            }
                    }

                    //(pickup.DefaultPhysicsModule as PickupStandardPhysics).UpdateServer();
                }
                catch (Exception e)
                {
                    if (!_threw)
                    {
                        Log.Error(e.ToString());
                        _threw = true;
                    }
                }
            //}
        }

        //private void OnDestroy()
        //{

        //}
    }
}