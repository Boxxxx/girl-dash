using UnityEngine;
using System.Collections;
using GirlDash.Map;
using System;

namespace GirlDash {
    public abstract class Enemy : CharacterController {
        public new float moveSpeed = 5f;
        public new float jumpForce = 500f;
        public DamageArea damageArea;

        private bool is_action_ = false;
        private bool is_visible_ = false;

        public  bool IsVisible {
            get {
                return CameraController.Instance.CheckInView(transform, false /* cached bounds is ok */);
            }
        }

        protected override bool is_right_major {
            get {
                return false;
            }
        }

        public void SpawnSelf(EnemyData enemy_data, Transform parent_transform) {
            CharacterData character_data = new CharacterData();

            character_data.name = enemy_data.enemyType.ToString();
            character_data.moveSpeed = moveSpeed;
            character_data.jumpInitForce = jumpForce;
            character_data.maxJumpCnt = 1;
            character_data.atk = enemy_data.fire_atk;
            character_data.hp = enemy_data.hp;

            if (damageArea != null) {
                damageArea.Reset(enemy_data.hit_atk, DamageArea.DamageGroup.Enemy);
            }

            Reset(character_data);
            if (muzzle_ != null) {
                muzzle_.Reset();
            }

            Clear();
            // Faces left
            SetFaceRight(false, true /* force set */);
            transform.parent = parent_transform;
            transform.localPosition = new Vector2(
                // the center x axis of a unit length from (x to x + 1).
                MapValue.RealValue(enemy_data.spawnPosition.x + enemy_data.spawnPosition.x + 1) * 0.5f,
                MapValue.RealValue(enemy_data.spawnPosition.y));
            transform.localScale = Vector3.one;

            is_action_ = false;
            is_visible_ = false;

            EventPool.Instance.Emit(Events.OnEnemyBorn, this);
        }

        public void RecycleSelf() {
            Die();

            StopAllCoroutines();
            rigidbody2D_.isKinematic = true;
            PoolManager.Deallocate(this);
        }

        protected override void HitByDamageArea(DamageArea damage_area) {
            if (!is_visible_) {
                // Invincible when not visible
                return;
            }
            base.HitByDamageArea(damage_area);
        }

        protected abstract void Action();
        protected virtual void OnVisible() {
            EventPool.Instance.Emit(Events.OnEnemyInView, this);
        }

        // The action logic of an enemy, this should be replaced with a behaviour machine.
        protected virtual bool CheckActive() {
            return true;
        }

        protected override bool CheckTakeDamage(DamageArea damage_area) {
            return damage_area.damageGroup == DamageArea.DamageGroup.Player;
        }

        protected override void OnDied() {
            EventPool.Instance.Emit(Events.OnEnemyDestroy, this);
            gameObject.SetActive(false);
        }

        protected override void OnNewBullet(Bullet bullet) {
            bullet.InitDamage(atk, DamageArea.DamageGroup.Enemy);
        }

        protected override void Update() {
            base.Update();

            if (!is_visible_ && IsVisible) {
                is_visible_ = true;
                OnVisible();
            }
            if (!is_action_ && CheckActive()) {
                EventPool.Instance.Emit(Events.OnEnemyAction, this);

                is_action_ = true;
                Action();
            }
        }
    }
}