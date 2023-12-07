using UnityEngine;

namespace SCProphunt.Types
{
    public struct PropInfo
    {
        public ItemType Item { get; }
        public Vector3 Scale { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Vector3 PlayerScale { get; }
        public float PlayerHP { get; }

        public PropInfo(ItemType item, Vector3 scale = default, Vector3 position = default, Quaternion rotation = default, Vector3 playerScale = default, int playerHP = default)
        {
            Item = item;
            Scale = scale == default ? Vector3.one : scale;
            PlayerScale = playerScale == default ? Vector3.one : playerScale;
            Position = position == default ? Vector3.zero : position;
            Rotation = rotation == default ? Quaternion.identity : rotation;
            PlayerHP = playerHP == default ? 50 : playerHP;
        }
    }
}