using UnityEngine;
using System.Collections;
using GirlDash.Map;

namespace GirlDash {
    public class Enemy : CharacterController {
        public new float moveSpeed = 5f;
        public new float jumpForce = 500f;
        public DamageArea damageArea;

        private Renderer renderer_ = null;
        protected bool IsVisible {
            get {
                if (renderer_ == null) {
                    renderer_ = GetComponent<Renderer>();
                }
                return renderer_.isVisible;
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
            character_data.jumpForce = jumpForce;
            character_data.atk = enemy_data.fire_atk;
            character_data.hp = enemy_data.hp;

            damageArea.Reset(enemy_data.hit_atk);

            Reset(character_data);

            // Faces left
            SetFaceRight(false, true /* force set */);
            transform.parent = parent_transform;
            transform.localPosition = new Vector2(
                // the center x axis of a unit length from (x to x + 1).
                MapValue.RealValue(enemy_data.spawnPosition.x + enemy_data.spawnPosition.x + 1) * 0.5f,
                MapValue.RealValue(enemy_data.spawnPosition.y));

            StartCoroutine(Logic());
        }

        public void RecycleSelf() {
            StopAllCoroutines();
            rigidbody2D_.isKinematic = true;
            PoolManager.Deallocate(this);
        }

        private IEnumerator Logic() {
            while (true) {
                // Delay 1 seconds
                yield return new WaitForSeconds(1);

                if (IsVisible) {
                    Jump();

                    // Waits for jump down
                    yield return new WaitForSeconds(2);
                }
            }
        }
    }
}