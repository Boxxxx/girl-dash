using UnityEngine;

namespace GirlDash {
    public static class MathUtil {
        public const float kLargeEps = 1e-5f;

        public static int ToIntSign(float value) {
            return Mathf.Abs(value) < kLargeEps ? 0 : (value > 0 ? 1 : -1);
        }
    }
}