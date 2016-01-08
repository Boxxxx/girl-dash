using UnityEngine;
using GirlDash.Map;

namespace GirlDash {
    public class CameraController : MonoBehaviour {
        public Transform hero;

        private void Track(Transform target) {
            var now_position = transform.position;
            now_position.x = target.position.x - 5.68f;
            transform.position = now_position;
        }

        void FixedUpdate() {
            Track(hero);
        }
    }
}
