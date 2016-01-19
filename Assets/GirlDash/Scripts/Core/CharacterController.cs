using UnityEngine;
using System.Collections.Generic;

namespace GirlDash {
    [RequireComponent(typeof(Animator), typeof(SkeletonAnimator))]
    public abstract class CharacterController : MonoBehaviour {
        public class AnimatorParameters {
            public static readonly string Jump = "jump";
            public static readonly string Fall = "fall";
            public static readonly string Grounded = "grounded";
            public static readonly string Die = "die";
            public static readonly string Victory = "victory";
            public static readonly string Fire = "fire";
            public static readonly string IsMove = "is_move";
        }

        public bool invincible = false;
        public Transform groundChecker;
        public Transform targetPosition;

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
            get { return hp_; }
            set {
                if (hp_ != value) {
                    hp_ = value;
                    if (hp_ <= 0) {
                        hp_ = 0;
                        Die();
                    }
                }
            }
        }

        // local position
        public float position {
            get { return StartingLine.Instance.GetOffset(transform).x; }
        }

        /// <summary>
        /// This field indicates whether the right direction is major direction (local_scale.x == 1).
        /// </summary>
        protected virtual bool is_right_major {
            get {
                return true;
            }
        }

        // Cached variables
        protected int ground_layermask_;
        protected Animator animator_;
        protected SkeletonAnimator skeleton_animator_;
        protected Rigidbody2D rigidbody2D_;
        protected CharacterData character_data_;
        protected Muzzle muzzle_;

        private int hp_ = 0;
        private bool is_face_right_ = false;
        private bool cached_jump_trigger_ = false;
        private float last_y_speed_ = 0f;
        private float move_axis_ = 0f;

        private HashSet<int> hit_damagearea_ids = new HashSet<int>();
        private int damagearea_layer_mask_;

        #region Public Methods
        public void Fire() {
            if (!isAlive || muzzle_ == null /* must have a gun */) {
                return;
            }

            muzzle_.Fire();
            SetActionTrigger(AnimatorParameters.Fire);
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

            OnDied();
        }

        public void Reset(CharacterData character_data) {
            isAlive = true;

            isGrounded = false;
            rigidbody2D_.isKinematic = false;

            character_data_ = character_data;
            hp_ = character_data_.hp;

            hit_damagearea_ids.Clear();

            SetFaceRight(true, false /* force set*/);
        }
        #endregion

        #region Private & Protected Methods
        protected void SetFaceRight(bool value, bool force_set) {
            if (force_set || is_face_right_ != value) {
                is_face_right_ = value;

                Vector3 local_scale = transform.localScale;
                local_scale.x = is_face_right_ == is_right_major ? 1 : -1;
                transform.localScale = local_scale;
            }
        }

        private bool GroundedTest() {
            return Physics2D.Linecast(transform.position, groundChecker.position, ground_layermask_);
        }

        private void Flip() {
            SetFaceRight(!isFaceRight, false /* not force */);
        }

        protected void SetActionTrigger(string trigger) {
            animator_.SetTrigger(trigger);
            OnActionTrigger(trigger);
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
                SetActionTrigger(AnimatorParameters.Jump);
                rigidbody2D_.AddForce(new Vector2(0f, jumpForce));

                cached_jump_trigger_ = false;
            }
        }

        private void FallUpdate() {
            if (isGrounded) {
                return;
            }
            if (last_y_speed_ >= 0 && rigidbody2D_.velocity.y < 0) {
                SetActionTrigger(AnimatorParameters.Fall);
            }
            last_y_speed_ = rigidbody2D_.velocity.y;
        }

        private void SyncSkeletonPositions() {
            if (muzzle_ != null) {
                var bone = skeleton_animator_.skeleton.Data.FindBone("muzzle");
                Debug.Log(string.Format("position: {0}, {1}", bone.x, bone.y));
            }
        }

        protected void HitByDamageArea(DamageArea damage_area) {
            if (hit_damagearea_ids.Contains(damage_area.uniqueId)) {
                // Saint Seiya will never be hit by the same damage twice!
                return;
            }

            hit_damagearea_ids.Add(damage_area.uniqueId);
            hp -= damage_area.damage;
            damage_area.OnTakeDamage(this);
        }

        protected abstract void OnNewBullet(Bullet bullet);
        protected abstract void OnDied();
        protected virtual void OnActionTrigger(string trigger) {}

        // Returns whether it should take this damge or not.
        protected virtual bool CheckTakeDamage(DamageArea damage_area) { return false; }
        #endregion

        #region Unity Functions
        protected virtual void Awake() {
            ground_layermask_ = 1 << LayerMask.NameToLayer(Consts.kGroundLayer);
            animator_ = GetComponent<Animator>();
            skeleton_animator_ = GetComponent<SkeletonAnimator>();
            rigidbody2D_ = GetComponent<Rigidbody2D>();
            muzzle_ = GetComponentInChildren<Muzzle>();
            muzzle_.Register(this, OnNewBullet);

            damagearea_layer_mask_ = LayerMask.NameToLayer(Consts.kDamageAreaLayer);
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

        protected virtual void Update() {
            SyncSkeletonPositions();
        }

        protected virtual void OnTriggerEnter2D(Collider2D other) {
            if (invincible) {
                return;
            }
            if (other.gameObject.layer == damagearea_layer_mask_) {
                var damage_area = other.gameObject.GetComponent<DamageArea>();
                if (damage_area == null) {
                    Debug.LogError(other.name + " should have a damage area.");
                }
                if (CheckTakeDamage(damage_area)) {
                    HitByDamageArea(damage_area);
                }
            }
        }
        #endregion
    }
}