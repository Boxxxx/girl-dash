using UnityEngine;
using System.Collections;

namespace GirlDash.UI {
    public class UIFollower : MonoBehaviour {
        public Transform followee;
        public Vector2 offset = Vector2.zero;
        public Vector2 moveThreshold = new Vector2(0, 50);

        private Vector2 last_pos;

        public virtual void Follow(Transform followee, Vector2 offset) {
            this.followee = followee;
            this.offset = offset;
        }

        protected virtual void Update() {
            if (followee == null) {
                return;
            }
            Vector2 new_pos = ScreenUtil.WorldPosToUI(followee.transform.position);
            new_pos = new Vector2(
                GetNewValue(new_pos.x + offset.x, last_pos.x, moveThreshold.x),
                GetNewValue(new_pos.y + offset.y, last_pos.y, moveThreshold.y));
            last_pos = new_pos;
            transform.position = ScreenUtil.UILocalToWorld(new_pos);

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