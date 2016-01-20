using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace GirlDash {
    public class PlayerController : CharacterController, IGameComponent {
        private bool running_ = false;
        private bool cached_fire_ = false;

        public IEnumerator Load(CharacterData character_data) {
            Reset(character_data);
            yield return null;
        }

        public void GameStart() {
            running_ = true;
        }

        public void GameOver() {
            Move(0);
            running_ = false;
        }

        public override void Fire() {
            if (!isAlive || muzzle_ == null /* must have a gun */) {
                return;
            }

            // Do not fire here, just trigger the animation.
            // We will do actually fire in the event handler of 'fire' spine event.
            cached_fire_ = true;
            SetActionTrigger(AnimatorParameters.Fire);
        }

        protected override void OnNewBullet(Bullet bullet) {
            bullet.InitDamage(atk, DamageArea.DamageGroup.Player);
        }

        protected override void OnDied() {
            Move(0);
            rigidbody2D_.velocity = Vector2.zero;

            SetActionTrigger(AnimatorParameters.Die);

            GameController.Instance.OnPlayerDie();
        }

        protected override bool CheckTakeDamage(DamageArea damage_area) {
            return damage_area.damageGroup == DamageArea.DamageGroup.Enemy;
        }

        protected virtual void OnSpineEvent(SkeletonAnimator animator, int layer_index, Spine.Event ev) {
            if (ev.Data.name == AnimatorParameters.FireEvent) {
                OnFire();
            }
        }

        protected virtual void OnFire() {
            if (cached_fire_) {
                cached_fire_ = false;
                muzzle_.Fire();
            }
        }

        #region Unity Callbacks
        protected void Start() {
            if (muzzle_ != null) {
                muzzle_.Reset();
            }
            skeleton_animator_.Event += OnSpineEvent;
        }

        protected override void FixedUpdate() {
            if (running_) {
                base.FixedUpdate();
            }
        }
        #endregion
    }
}
