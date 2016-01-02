using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class Bullet : ReuseableObject {
        public float speed = 5f;
        public float lifeTime = 10f;

        public float atk {
            get; private set;
        }
        public Vector2 direction {
            get; private set;
        }

        private float current_time_ = 0;

        public void Init(float atk, Vector2 direction) {
            this.atk = atk;
            this.direction = direction.normalized;

            transform.rotation = Quaternion.FromToRotation(Vector2.right, direction);
            current_time_ = 0;
        }

        public void InitWithTarget(float atk, Vector2 target_position) {
            Init(atk, target_position - (Vector2)transform.position);
        }

        public void Update() {
            current_time_ += Time.deltaTime;

            if (current_time_ > lifeTime) {
                PoolManager.Deallocate(gameObject);
                return;
            }

            transform.position = transform.position + (Vector3)(direction * speed * Time.deltaTime);
        }
    }
}