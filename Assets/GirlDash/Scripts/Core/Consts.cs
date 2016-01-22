using UnityEngine;
using System.Collections;

namespace GirlDash {
    public static class Consts {
        public const float kSoftEps = 1e-5f;
        public const int kFps = 60;
        public const float kScreenWidth = 11.36f;
        public static readonly string kGroundLayer = "Ground";
        public static readonly string kDamageAreaLayer = "DamageArea";
    }

    /// <summary>
    /// Consts that is filled runtime by GameController.
    /// </summary>
    public static class RuntimeConsts {
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