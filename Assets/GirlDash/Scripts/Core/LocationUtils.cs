using UnityEngine;
using System.Collections;

namespace GirlDash {
    // Used to get the location info for creatures, mainly used by enemies.
    public static class LocationUtils {
        public static PlayerController player {
            get { return GameController.Instance.playerController; }
        }
        public static float GetDistToPlayer(Transform transform) {
            return Mathf.Abs(transform.position.x - player.transform.position.x);
        }

        public static bool InFrontOfPlayer(Transform transform) {
            return transform.position.x >= player.transform.position.x;
        }
    }
}