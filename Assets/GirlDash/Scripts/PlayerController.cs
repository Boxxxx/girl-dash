using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public float maxSpeed = 5f;
    public float moveForce = 365f;
    public float jumpForce = 1000f;
    public float moveEps = 1e-3f;

    // Cached variables
    private int ground_layermask_;
    private Transform ground_checker_;
    private Animator animator_;
    private Rigidbody2D rigidbody2D_;

    // Status variables
    private bool grounded_ = false;
    private bool facing_right_ = true;

    // Cached trigger
    bool jump_trigger_ = false;
    bool grounded_trigger_ = false;

	void Awake() {
        ground_checker_ = transform.Find("groundChecker");
        ground_layermask_ = 1 << LayerMask.NameToLayer("Ground");
        animator_ = GetComponent<Animator>();
        rigidbody2D_ = GetComponent<Rigidbody2D>();
    }

	void Update () {
        bool new_grounded = Physics2D.Linecast(transform.position, ground_checker_.position, ground_layermask_);
        if (new_grounded != grounded_) {
            grounded_ = new_grounded;
            grounded_trigger_ = true;
        }

        if (Input.GetButtonDown("Jump") && grounded_)
            jump_trigger_ = true;
    }

    void FixedUpdate() {
        if (grounded_trigger_) {
            animator_.SetTrigger("Grounded");
            grounded_trigger_ = false;
        }

        float horiz_axis = Input.GetAxis("Horizontal");
        horiz_axis = Mathf.Abs(horiz_axis) < moveEps ? 0 : Mathf.Sign(horiz_axis);
        Debug.Log(horiz_axis);

        animator_.SetFloat("Speed", Mathf.Abs(horiz_axis));

        rigidbody2D_.velocity = new Vector2(
            horiz_axis * maxSpeed, rigidbody2D_.velocity.y);

        if (horiz_axis > 0 && !facing_right_ ||
            horiz_axis < 0 && facing_right_) {
            Flip();
        }

        if (jump_trigger_) {
            animator_.SetTrigger("Jump");
            rigidbody2D_.AddForce(new Vector2(0f, jumpForce));

            jump_trigger_ = false;
        }
    }

    void Flip() {
        facing_right_ = !facing_right_;

        Vector3 local_scale = transform.localScale;
        local_scale.x *= -1;
        transform.localScale = local_scale;
    }
}
