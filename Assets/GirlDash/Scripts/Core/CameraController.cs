using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class CameraController : MonoBehaviour, IGameComponent {
        public Transform target;

        public void GameStart() { }
        public void GameOver() { }

        /// <summary>
        /// Tracks targets, the current implementation is to track the exact x axis. 
        /// Y axis is forzen to initial settings.
        /// </summary>
        private void Track(Transform target) {
            var now_position = transform.position;
            now_position.x = target.position.x;
            transform.position = now_position;
        }

        void Update() {
            Track(target);
        }
    }
}
