using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class DogEnemy : Enemy {
        public const float kActiveDistance = 10;

        // Overrides for avoiding warning in unit, since dog has no fire trigger.
        public override void Fire() {
            if (!isAlive || muzzle_ == null /* must have a gun */) {
                return;
            }

            muzzle_.Fire();
            // No fire trigger for dog.
        }

        protected override void Action() {
            StartCoroutine(JumpLogic());
        }

        protected override bool CheckActive() {
            return GameController.Instance.GetDistToPlayer(transform) <= kActiveDistance;
        }

        protected override void OnActionTrigger(string trigger) {
            if (trigger == AnimatorParameters.Fall) {
                Fire();
            }
        }

        private IEnumerator JumpLogic() {
            while (GameController.Instance.InFrontOfPlayer(transform)) {
                Jump();
                
                // Waits for jump down
                yield return new WaitForSeconds(2.0f);
            }
        }
    }
}