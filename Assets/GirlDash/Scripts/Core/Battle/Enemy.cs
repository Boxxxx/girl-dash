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

        public void Reset(EnemyData enemy_data) {
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

            StartCoroutine(Logic());
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

        void Start() {
            EnemyData enemy_data = new EnemyData();
            enemy_data.enemyType = EnemyData.EnemyType.Dog;

            Reset(enemy_data);
        }
    }
}