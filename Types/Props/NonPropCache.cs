using InventorySystem.Items.Pickups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCProphunt.Types
{
    public class NonPropCache
    {
        public Vector3 Scale { get; set; } = Vector3.one;
        public Vector3 Position { get; set; } = Vector3.zero;
        public Quaternion Rotation { get; set; } = Quaternion.identity;
        public ItemType ItemType { get; set; } = ItemType.None;
        public PickupSyncInfo PickupSyncInfo { get; set; } = new PickupSyncInfo();
    }
}
