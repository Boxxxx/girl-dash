using UnityEngine;

namespace GirlDash {
    public static class Utils {
        public static Vector3 RandomVector(Vector2 fluctuation) {
            return new Vector3(Random.Range(-fluctuation.x, fluctuation.x), Random.Range(-fluctuation.y, fluctuation.y), 0);
        }
    }
}