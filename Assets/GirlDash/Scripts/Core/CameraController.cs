using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class CameraController : SingletonObject<MonoBehaviour>, IGameComponent {
        public Transform target;
        public float offsetX = 4f;

        public void GameStart() { }
        public void GameOver() { }

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
        }
    }
}
