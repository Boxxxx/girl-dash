using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class BackgroundRenderer : MonoBehaviour {
        public SpriteRenderer sprite1;
        public SpriteRenderer sprite2;
        public float moveRatio = 0.1f;

        private float sprite_width_;

        public void UpdateProgress(float progress) {
            float background_offset = progress * moveRatio;
            background_offset = background_offset - Mathf.Floor(background_offset / sprite_width_) * sprite_width_;
            sprite1.transform.localPosition = new Vector2(-background_offset, 0);
            sprite2.transform.localPosition = new Vector2(sprite1.bounds.size.x - background_offset, 0);
        }

        void Awake() {
            sprite_width_ = sprite1.bounds.size.x;
        }
    }
}