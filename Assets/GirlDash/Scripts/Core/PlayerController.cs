using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace GirlDash {
    public class PlayerController : CharacterController, IGameComponent {
        public Animator animator;

        private bool running_ = false;
        private bool is_firing_ = false;
        private float current_fire_cooldown_ = 0;
        // Since we want firing just begins at event time, so we must cache this bool,
        // when recevied a fire event, we shoot bullet.
        // This will be refresh every 'fire_cooldown' if the player keeps hold the fire button.
        private bool ready_for_next_shoot_ = true;

        public float fireCooldown {
            get { return current_fire_cooldown_; }
        }

        public IEnumerator Load(CharacterData character_data) {
            Reset(character_data);
            yield return null;
        }

        public void GameStart() {
            running_ = true;
            is_firing_ = false;
            current_fire_cooldown_ = 0;
            ready_for_next_shoot_ = true;
            animator.Rebind();
        }

        public void GameOver() {
            Move(0);
            running_ = false;
        }

        public override void Fire() {
            if (!isAlive || muzzle_ == null /* must have a gun */ || current_fire_cooldown_ > Consts.kSoftEps /* gun must be cooldown */) {
                return;
            }

            // Do not fire here, just trigger the animation.
            // We will do actually fire in the event handler of 'fire' spine event.
            ready_for_next_shoot_ = true;
            is_firing_ = true;
            animator_.SetBool(AnimatorParameters.IsFiring, is_firing_);
            SetActionTrigger(AnimatorParameters.Fire);
        }

        public void HoldFire(bool is_firing) {
            is_firing_ = is_firing;
            animator_.SetBool(AnimatorParameters.IsFiring, is_firing_);
        }

        protected override void OnNewBullet(Bullet bullet) {
            bullet.InitDamage(atk, DamageArea.DamageGroup.Player);
        }

        protected override void OnDied() {
            Move(0);
            rigidbody2D_.velocity = Vector2.zero;
            rigidbody2D_.isKinematic = true;

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
            if (ready_for_next_shoot_) {
                current_fire_cooldown_ = character_data_.fireCooldown;
                ready_for_next_shoot_ = false;
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

        protected override void Update() {
            if (!running_) {
                return;
            }

            base.Update();
            if (current_fire_cooldown_ > 0) {
                current_fire_cooldown_ = Mathf.Max(0, current_fire_cooldown_ - Time.deltaTime);
            }
            if (is_firing_ && current_fire_cooldown_ < Consts.kSoftEps) {
                ready_for_next_shoot_ = true;
            }
        }
        #endregion
    }
}
