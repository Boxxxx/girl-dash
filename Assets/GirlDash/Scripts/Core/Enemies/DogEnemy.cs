using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class DogEnemy : Enemy {
        public const float kActiveDistance = 10;

        protected override void Action() {
            Debug.Log(name + " is in action!");
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