using UnityEngine;
using System;

namespace GirlDash {
    using Random = UnityEngine.Random;

    public class Muzzle : MonoBehaviour {
        public CrossHairs crossHairs;
        public Bullet bullet;
        public int numBulletsPerShoot = 1;
        public float fireTimePeriod = 0.25f;
        public Vector2 firePositionFluctuation = new Vector2(0, 0.1f);
        public float fireDirectionFluctuation = 5f;

        private CharacterController controller_;
        private int num_bullets_to_fire_ = 0;
        private float current_fire_cooldown_ = 0;
        private Action<Bullet> on_new_bullet_;

        public float fireCooldown {
            get { return current_fire_cooldown_; }
        }

        public void Fire() {
            if (numBulletsPerShoot == 0) {
                return;
            }
            Shoot();
            if (numBulletsPerShoot > 1) {
                num_bullets_to_fire_ = numBulletsPerShoot - 1;
                current_fire_cooldown_ = fireTimePeriod;
            } else {
                num_bullets_to_fire_ = 0;
                current_fire_cooldown_ = 0;
            }
        }

        protected virtual void Shoot() {
            var new_bullet = PoolManager.Allocate(bullet);
            if (new_bullet == null) {
                return;
            }

            Vector3 position = transform.position + Utils.RandomVector(firePositionFluctuation);
            Vector2 direction;
            
            if (crossHairs != null) {
                direction = crossHairs.GetDirection(controller_.isFaceRight);
            } else {
                direction = controller_.isFaceRight? Vector2.right: Vector2.left;
            }

            if (position.y >= transform.position.y) {
                direction = Quaternion.Euler(0, 0, Random.Range(0, fireDirectionFluctuation)) * direction;
            }
            else {
                direction = Quaternion.Euler(0, 0, Random.Range(-fireDirectionFluctuation, 0)) * direction;
            }

            new_bullet.InitTransform(position, direction);

            if (on_new_bullet_ != null) {
                on_new_bullet_(new_bullet);
            }
        }

        public void Register(CharacterController controller, Action<Bullet> on_new_bullet) {
            controller_ = controller;
            on_new_bullet_ = on_new_bullet;
        }

        public void Reset() {
            num_bullets_to_fire_ = 0;
            current_fire_cooldown_ = 0;
        }

        void Update() {
            while (num_bullets_to_fire_ > 0) {
                current_fire_cooldown_ -= Time.deltaTime;
                if (current_fire_cooldown_ <= 0) {
                    Shoot();
                    num_bullets_to_fire_--;
                    current_fire_cooldown_ += fireTimePeriod;
                } else {
                    break;
                }
            }
        }
    }
}
