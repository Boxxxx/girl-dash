using UnityEngine;
using System.Collections.Generic;

namespace GirlDash.Map {
    [RequireComponent(typeof(BoxCollider2D))]
    public class GroundComponent : TerrainComponent {
        private BoxCollider2D collider_;
        private List<Transform> sprites_ = new List<Transform>();
        
        protected override Collider2D BuildCollider(TerrainData data) {
            Rect region_rect = (Rect)data.region;
            collider_.offset = new Vector2(region_rect.width * 0.5f, -region_rect.height * 0.5f);
            collider_.size = region_rect.size;
            return collider_;
        }
        protected override void BuildGraphics(TerrainData data, TerrainStyle style) {
            for (int i = 0; i < data.region.width; i++) {
                for (int j = 0; j < data.region.height; j++) {
                    var sprite = PoolManager.Allocate(style.groundUnitSprite);
                    
                    sprites_.Add(sprite);
                    sprite.transform.parent = graphics;
                    sprite.transform.localPosition = (Vector2)(new MapVector(i, j));
                }
            }
        }

        protected override void Init() {
            // Sets to topleft corner
            Rect rect = (Rect)data.region;
            transform.localPosition = new Vector2(rect.xMin, rect.yMin);
        }
        protected override void Cleanup() {
            base.Cleanup();

            for (int i = 0; i < sprites_.Count; i++) {
                PoolManager.Deallocate(sprites_[i]);
            }
            sprites_.Clear();
        }

        protected override void Awake() {
            base.Awake();
            collider_ = GetComponent<BoxCollider2D>();
        }
    }
}