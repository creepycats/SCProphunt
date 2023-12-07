using UnityEngine;

namespace SCProphunt.Types
{
    public class PropConfig
    {
        public Vector3 Scale { get; set; } = Vector3.one;
        public Vector3 Position { get; set; } = Vector3.zero;
        public Vector3 Rotation { get; set; } = Vector3.zero;
        public Vector3 PlayerScale { get; set; } = Vector3.one;
        public int PlayerHP { get; set; } = 50;

        //public PropConfig(Vector3 scale = default, Vector3 position = default, Quaternion rotation = default, Vector3 playerScale = default, int playerHP = default)
        //{
        //    Scale = scale == default ? Vector3.one : scale;
        //    PlayerScale = playerScale == default ? Vector3.one : scale;
        //    Position = position == default ? Vector3.zero : position;
        //    Rotation = rotation == default ? Quaternion.identity : rotation;
        //    PlayerHP = playerHP == default ? 50 : playerHP;
        //}
    }
}