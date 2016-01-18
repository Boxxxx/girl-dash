using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class Bullet : DamageArea {
        public float speed = 5f;
        public float lifeTime = 10f;
        public bool isPenetrate = false;

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

        public void Update() {
            current_time_ += Time.deltaTime;

            if (current_time_ > lifeTime) {
                DestroySelf();
                return;
            }

            transform.position = transform.position + (Vector3)(direction * speed * Time.deltaTime);
        }

        public override void OnTakeDamage(CharacterController character) {
            DestroySelf();
        }

        private void DestroySelf() {
            PoolManager.Deallocate(gameObject);
        }
    }
}