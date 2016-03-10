using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class Bullet : DamageArea {
        public float speed = 5f;
        public float lifeTime = 10f;
        public bool destroyWhenHit = true;

        public Vector2 direction {
            get; private set;
        }

        private float current_time_ = 0;

        public void InitTransform(Vector2 position, Vector2 direction) {
            transform.position = position;
            this.direction = direction.normalized;

            transform.rotation = Quaternion.FromToRotation(Vector2.right, direction);
            current_time_ = 0;
        }

        public void InitDamage(int atk, DamageGroup group) {
            Reset(atk, group);
        }

        public void Reflect(int damage, DamageGroup group) {
            Reset(damage, group);
            InitTransform(transform.position, -direction);
        }

        public void Update() {
            current_time_ += Time.deltaTime;

            if (current_time_ > lifeTime) {
                DestroySelf();
                return;
            }

            transform.position = transform.position + (Vector3)(direction * speed * Time.deltaTime * RuntimeConsts.mapScale);
        }

        public void Release() {
            DestroySelf();
        }

        public override void OnTakeDamage(CharacterController character) {
            if (destroyWhenHit) {
                DestroySelf();
            }
        }

        protected virtual void OnHitGround() {
            DestroySelf();
        }

        private void DestroySelf() {
            PoolManager.Deallocate(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.layer == RuntimeConsts.groundLayer) {
                OnHitGround();
            }
        }
    }
}