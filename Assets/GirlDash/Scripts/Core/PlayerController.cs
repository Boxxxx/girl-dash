using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace GirlDash {
    public class PlayerController : CharacterController, IGameComponent {
        private bool running_ = false;

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

        #region Unity Callbacks
        protected void Start() {
            if (muzzle_ != null) {
                muzzle_.Reset();
            }
        }

        protected override void FixedUpdate() {
            if (running_) {
                base.FixedUpdate();
            }
        }
        #endregion
    }
}
