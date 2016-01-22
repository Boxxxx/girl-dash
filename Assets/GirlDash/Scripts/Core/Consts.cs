using UnityEngine;
using System.Collections;

namespace GirlDash {
    public static class Consts {
        public const float kSoftEps = 1e-5f;
        public const int kFps = 60;
        public static readonly string kGroundLayer = "Ground";
        public static readonly string kDamageAreaLayer = "DamageArea";
    }

    /// <summary>
    /// Consts that is filled runtime by GameController.
    /// </summary>
    public static class RuntimeConsts {
        // The init screen width & height is calcualted at the begining of the game.
        // We assume the screen resolution won't change during gameplay, but you should know it's possible.
        public static Vector2 initScreenSize;
        public static int groundLayer;
        public static int groundLayerMask;
    }

    public static class ResourceNames {
        public static readonly string kRifleBullet = "RifleBullet";
    }

    public static class InputEvents {
        public static readonly string kJump = "Jump";
        public static readonly string kFire = "Fire1";
        public static readonly string kHorizontal = "Horizontal";
    }
}