using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GirlDash {
    public class PlayerController : CharacterController, IGameComponent {
        public bool invincible = false;
        private bool running_ = false;
        private HashSet<int> hit_damagearea_ids = new HashSet<int>();
        private int damagearea_layer_mask_;

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

        protected override void FixedUpdate() {
            if (running_) {
                base.FixedUpdate();
            }
        }

        private void HitByDamageArea(DamageArea damage_area) {
            if (hit_damagearea_ids.Contains(damage_area.uniqueId)) {
                // Saint Seiya will never be hit by the same damage twice!
                return;
            }

            hit_damagearea_ids.Add(damage_area.uniqueId);
            hp -= damage_area.damage;
        }

        #region Unity Callbacks
        protected override void Awake() {
            base.Awake();
            damagearea_layer_mask_ = LayerMask.NameToLayer(Consts.kDamageAreaLayer);
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (invincible) {
                return;
            }
            if (other.gameObject.layer == damagearea_layer_mask_) {
                var damage_area = other.gameObject.GetComponent<DamageArea>();
                if (damage_area == null) {
                    Debug.LogError(other.name + " should have a damage area.");
                }
                HitByDamageArea(damage_area);
            }
        }
        #endregion
    }
}
