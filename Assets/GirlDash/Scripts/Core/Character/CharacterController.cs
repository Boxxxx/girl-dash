using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class CharacterController : MonoBehaviour {
        public class AnimatorParameters {
            public static readonly string Jump = "jump";
            public static readonly string Fall = "fall";
            public static readonly string Grounded = "grounded";
            public static readonly string Die = "die";
            public static readonly string Victory = "victory";
            public static readonly string Fire = "fire";
            public static readonly string IsMove = "is_move";
        }

        public Transform groundChecker;
        public Transform muzzle;

        public Vector2 firePositionFluctuation = new Vector2(0, 0.1f);
        public float fireDirectionFluctuation = 5f;

        public bool isGrounded {
            get;
            private set;
        }

        public bool isFaceRight {
            get {
                return is_face_right_;
            }
            private set {
                if (is_face_right_ != value) {
                    is_face_right_ = value;

                    Vector3 local_scale = transform.localScale;
                    local_scale.x = is_face_right_ ? 1 : -1;
                    transform.localScale = local_scale;
                }
            }
        }

        public bool isAlive {
            get; private set;
        }

        public CharacterData characterData {
            get {
                return character_data_;
            }
        }

        public float moveSpeed {
            get { return character_data_.moveSpeed; }
        }

        public float jumpForce {
            get { return character_data_.jumpForce; }
        }

        public int atk {
            get { return character_data_.atk; }
        }

        public int hp {
            get { return character_data_.hp; }
        }

        // Cached variables
        private int ground_layermask_;
        private Animator animator_;
        private Rigidbody2D rigidbody2D_;
        private CharacterData character_data_;

        private bool is_face_right_ = false;
        private bool cached_jump_trigger_ = false;
        private float last_y_speed_ = 0f;
        private float move_axis_ = 0f;

        #region Public Methods
        public void Fire() {
            if (!isAlive) {
                return;
            }

            var bullet = PoolManager.Allocate<Bullet>(ResourceNames.kRifleBullet);
            if (bullet == null) {
                return;
            }
            bullet.transform.position = muzzle.position + RandomVector(firePositionFluctuation);

            Vector2 direction = isFaceRight ? Vector2.right : Vector2.left;
            if (bullet.transform.position.y >= muzzle.position.y) {
                direction = Quaternion.Euler(0, 0, Random.Range(0, fireDirectionFluctuation)) * direction;
            } else {
                direction = Quaternion.Euler(0, 0, Random.Range(-fireDirectionFluctuation, 0)) * direction;
            }
            
            bullet.Init(0, direction);

            animator_.SetTrigger(AnimatorParameters.Fire);
        }

        public void Jump() {
            if (!isAlive) {
                return;
            }

            if (isGrounded) {
                cached_jump_trigger_ = true;
            }
        }

        public void Move(float axis) {
            if (!isAlive) {
                return;
            }

            move_axis_ = axis;
        }

        public void Die() {
            if (!isAlive) {
                return;
            }

            isAlive = false;
            animator_.SetTrigger(AnimatorParameters.Die);
        }

        public void Reset(CharacterData character_data) {
            isAlive = true;

            isGrounded = false;
            isFaceRight = true;

            character_data_ = character_data;
        }
        #endregion

        #region Private Methods
        private bool GroundedTest() {
            return Physics2D.Linecast(transform.position, groundChecker.position, ground_layermask_);
        }

        private void Flip() {
            isFaceRight = !isFaceRight;
        }

        private void GroundedUpdate() {
            bool new_grounded = GroundedTest();
            if (new_grounded != isGrounded) {
                isGrounded = new_grounded;
                animator_.SetBool(AnimatorParameters.Grounded, new_grounded);
            }
        }

        private void MoveUpdate() {
            int direction = MathUtil.ToIntSign(move_axis_);
            animator_.SetBool(AnimatorParameters.IsMove, direction != 0);

            rigidbody2D_.velocity = new Vector2(
                move_axis_ * moveSpeed, rigidbody2D_.velocity.y);

            if (move_axis_ > 0 && !isFaceRight ||
                move_axis_ < 0 && isFaceRight) {
                Flip();
            }
        }

        private void JumpUpdate() {
            if (cached_jump_trigger_) {
                animator_.SetTrigger(AnimatorParameters.Jump);
                rigidbody2D_.AddForce(new Vector2(0f, jumpForce));

                cached_jump_trigger_ = false;
            }
        }

        private void FallUpdate() {
            if (isGrounded) {
                return;
            }
            if (last_y_speed_ >= 0 && rigidbody2D_.velocity.y < 0) {
                animator_.SetTrigger(AnimatorParameters.Fall);
            }
            last_y_speed_ = rigidbody2D_.velocity.y;
        }

        private Vector3 RandomVector(Vector2 fluctuation) {
            return new Vector3(Random.Range(-firePositionFluctuation.x, firePositionFluctuation.x), Random.Range(-firePositionFluctuation.y, firePositionFluctuation.y), 0);
        }
        #endregion

        #region Unity Functions
        protected virtual void Awake() {
            ground_layermask_ = 1 << LayerMask.NameToLayer(Consts.kGroundLayer);
            animator_ = GetComponent<Animator>();
            rigidbody2D_ = GetComponent<Rigidbody2D>();
        }

        protected virtual void FixedUpdate() {
            if (character_data_ == null) {
                return;
            }
            GroundedUpdate();
            MoveUpdate();
            JumpUpdate();
            FallUpdate();
        }
        #endregion
    }
}