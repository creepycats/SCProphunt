using InventorySystem.Items.Pickups;
using UnityEngine;

namespace SCProphunt.Types
{
    public class PropItemComponent : MonoBehaviour
    {
        internal PropPlayerComponent player;
        internal string ownerUserId;
        internal Vector3 pos;
        internal Vector3 itemOffset;
        internal Quaternion rot;
        internal ItemPickupBase item;
        internal bool rotationLock;
        internal ItemType itemType;
    }
}