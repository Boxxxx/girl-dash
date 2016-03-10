using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class BackgroundRenderer : MonoBehaviour {
        public SpriteRenderer sprite1;
        public SpriteRenderer sprite2;
        public float moveRatio = 0.1f;

        public void UpdateProgress(float progress) {
            float background_offset = progress * moveRatio;
            background_offset = background_offset - Mathf.Floor(background_offset / RuntimeConsts.mapWidth) * RuntimeConsts.mapWidth;
            sprite1.transform.localPosition = new Vector2(-background_offset, 0);
            sprite2.transform.localPosition = new Vector2(sprite1.bounds.size.x - background_offset, 0);
        }
    }
}