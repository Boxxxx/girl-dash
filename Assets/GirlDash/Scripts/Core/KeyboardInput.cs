using UnityEngine;

namespace GirlDash {
    [RequireComponent(typeof(CharacterController))]
    public class KeyboardInput : MonoBehaviour {
        public bool enableJump = true;
        public bool enableFire = true;
        public bool enableMove = false;

        private PlayerController controller_;
        private float last_horiz_axis_ = 0;

        void Start() {
            controller_ = GetComponent<PlayerController>();
        }

        void Update() {
            var debug_mode = GameController.Instance.debugMode;
            if (enableFire || debug_mode) {
                // Fire logic
                if (Input.GetButtonDown(InputEvents.kFire)) {
                    controller_.Fire();
                } else {
                    controller_.HoldFire(Input.GetButton(InputEvents.kFire));
                }
            }

            if (enableJump || debug_mode) {
                // Jump logic
                if (Input.GetButtonDown(InputEvents.kJump)) {
                    controller_.Jump();
                }
            }

            if (enableMove || debug_mode)  {
                // Move Logic
                float horiz_axis = Input.GetAxis(InputEvents.kHorizontal);
                controller_.Move(horiz_axis);
            }
        }
    }
}
