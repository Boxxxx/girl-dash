using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class VespidEnemy : Enemy {
        public const float kActiveDistance = 10;

        protected override void Action() {
            StartCoroutine(FireLogic());
        }

        protected override bool CheckActive() {
            return LocationUtils.GetDistToPlayer(transform) <= kActiveDistance;
        }

        private IEnumerator FireLogic() {
            while (LocationUtils.InFrontOfPlayer(transform)) {
                Fire();
                // Waits for jump down
                yield return new WaitForSeconds(2.0f);
            }
        }
    }
}