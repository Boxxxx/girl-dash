using UnityEngine;
using System.Collections;

namespace GirlDash {
    [RequireComponent(typeof(LineRenderer))]
    public class CrossHairs : MonoBehaviour {
        private const float maxRotate = 30.0f * Mathf.Deg2Rad;

        [Tooltip("The radiance to rotate in 1 sec.")]
        public float rotateSpeed = 10f;
        private LineRenderer ray_;
        private Vector2 target_direction_ = Vector2.right;
        private float last_angle_ = 0;

        public Vector2 GetDirection(bool is_face_right) {
            // Ignore the face direction, just return the direction calculated by angle.
            return new Vector2(Mathf.Cos(last_angle_), Mathf.Sin(last_angle_));
        }

        void Awake() {
            ray_ = GetComponent<LineRenderer>();
        }

        void Update() {
            Enemy enemy = GameController.Instance.enemyQueue.GetClosestVisibleEnemy();
            if (enemy == null || enemy.transform.position == transform.position) {
                target_direction_ = Vector2.right;
            }
            else {
                target_direction_ = enemy.targetPosition.position - transform.position;
                target_direction_.Normalize();
                if (target_direction_.x < 0) {
                    target_direction_ = Vector2.right;
                }
            }

            float angle = Mathf.Atan2(target_direction_.y, target_direction_.x);
            angle = Mathf.Clamp(angle, -maxRotate, maxRotate);
            if (Mathf.Abs(last_angle_ - angle) > Consts.kSoftEps) {
                // If they are not close enough, rotate smoothly with rotateSpeed.
                angle = Mathf.Lerp(last_angle_, angle, rotateSpeed * Time.deltaTime / Mathf.Abs(angle - last_angle_));
            }
            ray_.transform.transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * angle);
            last_angle_ = angle;
        }
    }
}
