using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class DogEnemy : Enemy {
        public float kActiveDistance = 5;

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