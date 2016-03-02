using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class DeadArea : MonoBehaviour {
        private PlayerController player_;
        private bool is_triggered_ = false;

        void OnEnable() {
            player_ = GameController.Instance.playerController;
            is_triggered_ = false;
        }

        void OnCollisionEnter2D(Collision2D other) {
            if (other.gameObject == player_.gameObject) {
                Trigger();
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject == player_.gameObject) {
                Trigger();
            }
        }

        void Trigger() {
            if (is_triggered_) {
                return;
            }
            is_triggered_ = true;
            player_.Die();
        }
    }
}