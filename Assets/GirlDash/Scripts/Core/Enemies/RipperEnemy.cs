using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class RipperEnemy : Enemy {
        public const float kActiveDistance = 3;
        public float moveDistance = 1;

        private float original_position_x_ = 0;

        protected override void Action() {
            original_position_x_ = transform.position.x;
            StartCoroutine(MoveLogic());
        }

        protected override bool CheckActive() {
            return LocationUtils.GetDistToPlayer(transform) <= kActiveDistance;
        }

        private IEnumerator MoveLogic() {
            while (original_position_x_ - transform.position.x < moveDistance) {
                Move(-1);
                yield return null;
            }
            Move(0);
            yield return null;
        }
    }
}