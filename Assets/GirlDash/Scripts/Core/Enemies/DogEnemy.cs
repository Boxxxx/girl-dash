using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class DogEnemy : Enemy {
        public const float kActiveDistance = 10;

        protected override void Action() {
            StartCoroutine(JumpLogic());
        }

        protected override bool CheckActive() {
            return LocationUtils.GetDistToPlayer(transform) <= kActiveDistance;
        }

        private IEnumerator JumpLogic() {
            while (LocationUtils.InFrontOfPlayer(transform)) {
                Jump();
                
                // Waits for jump down
                yield return new WaitForSeconds(2.0f);
            }
        }
    }
}