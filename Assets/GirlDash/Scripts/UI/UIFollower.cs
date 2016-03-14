using UnityEngine;
using System.Collections;

namespace GirlDash.UI {
    public class UIFollower : MonoBehaviour {
        public Transform followee;
        public Vector2 offset = Vector2.zero;
        public Vector2 moveThreshold = new Vector2(0, 50);

        public virtual void Follow(Transform followee, Vector2 offset) {
            this.followee = followee;
            this.offset = offset;
        }

        protected virtual void Update() {
            Vector2 new_pos = ScreenUtil.WorldPosToUI(followee.transform.position);
            transform.position = ScreenUtil.UILocalToWorld(new Vector2(
                GetNewValue(new_pos.x + offset.x, transform.position.x, moveThreshold.x),
                GetNewValue(new_pos.y + offset.y, transform.position.y, moveThreshold.y)));
        }

        float GetNewValue(float new_val, float old_val, float threshold) {
            if (Mathf.Abs(new_val - old_val) <= threshold) {
                return old_val;
            } else {
                return new_val;
            }
        }
    }
}