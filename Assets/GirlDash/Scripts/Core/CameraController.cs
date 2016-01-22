using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class CameraController : SingletonObject<CameraController>, IGameComponent {
        public Transform target;
        public float offsetX = 4f;
        public new Camera camera;

        private Bounds cached_bounds_;

        public void GameStart() { }
        public void GameOver() { }

        public void RecalculateOrthographicBounds() {
            float screen_aspect = Screen.width / (float)Screen.height;
            float camera_height = camera.orthographicSize * 2;
            cached_bounds_.center = new Vector3(camera.transform.position.x, camera.transform.position.y, 0);
            cached_bounds_.size = new Vector3(camera_height * screen_aspect, camera_height, 0);
        }

        public Bounds GetCachedCameraBounds(bool force_to_refresh) {
            if (force_to_refresh) {
                RecalculateOrthographicBounds();
            }
            return cached_bounds_;
        }

        /// <summary>
        /// If 'latest_bounds' is false, then it will use cached bounds to calculate visibility.
        /// Otherwise, it will recalculate the bounds immediately.
        /// </summary>
        public bool CheckInView(Transform transform, bool force_to_refresh) {
            if (force_to_refresh) {
                RecalculateOrthographicBounds();
            }
            return cached_bounds_.Contains(transform.position);
        }

        /// <summary>
        /// Tracks targets, the current implementation is to track the exact x axis. 
        /// Y axis is forzen to initial settings.
        /// </summary>
        private void Track(Transform target) {
            var now_position = transform.position;
            now_position.x = target.position.x + offsetX;
            transform.position = now_position;
        }

        void Update() {
            Track(target);
            RecalculateOrthographicBounds();
        }
    }
}
