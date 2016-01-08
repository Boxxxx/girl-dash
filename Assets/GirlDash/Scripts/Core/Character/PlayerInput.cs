using UnityEngine;

namespace GirlDash {
    [RequireComponent(typeof(CharacterController))]
    public class PlayerInput : MonoBehaviour {
        public float moveEps = 1e-3f;
        public float moveGravity = 10;
        public float moveSensitivity = 3;

        private CharacterController controller_;
        private float last_horiz_axis_ = 0;

        void Start() {
            controller_ = GetComponent<CharacterController>();
        }

        void Update() {
            {
                // Fire logic
                if (Input.GetButtonDown("Fire1")) {
                    controller_.Fire();
                }
            }

            {
                // Jump logic
                if (Input.GetButtonDown("Jump")) {
                    controller_.Jump();
                }
            }

            {
                // Move logic
                //
                // There is a special case: when turn around immedately, the Input.GetAxis() will return a zero in one frame,
                // which may lead to weird animation (right -> idle -> left).
                // However, if we use not-so-large moveGravity, that frame the horiz_axis won't reduce to 0, so the horiz_axis != 0.
                // And even more interesting is, the next frame this character will turn around, and we also have a non-zero horiz_axis.
                // Wow, It solves the special case perfectly!

                float horiz_axis = Input.GetAxis("Horizontal");
                int horiz_direction = MathUtil.ToIntSign(horiz_axis);

                if (last_horiz_axis_ * horiz_direction < 0) {
                    // If the direction changed, we ignore the last horiz axis.
                    // Which means, turn around immediately.
                    last_horiz_axis_ = 0;
                }

                if (horiz_direction != 0) {
                    horiz_axis = Mathf.Clamp(
                        last_horiz_axis_ + Time.deltaTime * horiz_direction * moveSensitivity, -1f, 1f);
                }
                else {
                    // If the axis == 0, then we gradually reduce the horiz axis until stopped.
                    if (last_horiz_axis_ > 0) {
                        horiz_axis = Mathf.Max(
                            0, last_horiz_axis_ - Time.deltaTime * moveGravity);
                    }
                    else {  // last_horiz_axis_ < 0
                        horiz_axis = Mathf.Min(
                            0, last_horiz_axis_ + Time.deltaTime * moveGravity);
                    }
                }
                last_horiz_axis_ = horiz_axis;

                //controller_.Move(horiz_axis);
            }
        }
    }
}
