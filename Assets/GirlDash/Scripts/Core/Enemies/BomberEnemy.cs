﻿using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class BomberEnemy : Enemy {
        public float kActiveDistance = 5;
        public float bombChargeTime = 1f;

        private Renderer renderer_;
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
            renderer_ = GetComponent<Renderer>();
            material_color_ = renderer_.material.color;
        }

        void OnEnable() {
            is_boomed_ = false;
        }

        void OnDisable() {
            LeanTween.cancel(gameObject);
            renderer_.material.color = material_color_;
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