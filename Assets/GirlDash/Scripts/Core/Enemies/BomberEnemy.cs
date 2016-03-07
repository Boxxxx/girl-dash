using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class BomberEnemy : Enemy {
        public const float kActiveDistance = 5;
        public float bombChargeTime = 1f;

        // Overrides for avoiding warning in unit, since dog has no fire trigger.
        public override void Fire() {
            if (!isAlive || muzzle_ == null /* must have a gun */) {
                return;
            }

            muzzle_.Fire();
            // No fire trigger for dog.
        }

        protected override void Action() {
            StartCoroutine(BombLogic());
        }

        protected override bool CheckActive() {
            return LocationUtils.GetDistToPlayer(transform) <= kActiveDistance;
        }

        private IEnumerator BombLogic() {
            yield return new WaitForSeconds(bombChargeTime);

            // Emit a bomb bullet.
            Fire();

            Die();
        }
    }
}