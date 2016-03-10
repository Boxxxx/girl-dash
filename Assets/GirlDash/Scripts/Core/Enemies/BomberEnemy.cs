using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class BomberEnemy : Enemy {
        public float kActiveDistance = 5;
        public float bombChargeTime = 1f;

        private Material material_;
        private Color material_color_;
        private bool is_boomed_ = false;

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

        protected override void OnDied() {
            base.OnDied();
            Boom();
        }

        private IEnumerator BombLogic() {
            LeanTween.color(gameObject, Color.red, bombChargeTime);

            yield return new WaitForSeconds(bombChargeTime);
            Boom();
        }

        protected override void Awake() {
            base.Awake();
            material_ = GetComponent<Renderer>().material;
            material_color_ = material_.color;
        }

        void OnEable() {
            is_boomed_ = false;
            material_.color = material_color_;
        }

        void Boom() {
            if (!is_boomed_) {
                is_boomed_ = true;

                // Emit a bomb bullet.
                Fire();
                Die();
            }
        }
    }
}