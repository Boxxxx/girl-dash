using UnityEngine;
using System.Collections.Generic;

namespace GirlDash.Map {
    [RequireComponent(typeof(BoxCollider2D))]
    public class GroundComponent : TerrainComponent {
        private BoxCollider2D collider_;
        private List<SpriteRenderer> tiles_ = new List<SpriteRenderer>();
        
        protected override Collider2D BuildCollider(TerrainData data, float real_width) {
            Rect region_rect = (Rect)data.region;
            collider_.offset = new Vector2(real_width * 0.5f, -region_rect.height * 0.5f);
            collider_.size = new Vector2(real_width, region_rect.height);
            return collider_;
        }

        // This building method will firstly try to join N loop tiles and a left and right tile,
        // so that the total width is close but smaller than the actual size.
        // If the rest size is equal to or greater than 'minimumValidLoopTile', we will put another loop tile.
        protected override float BuildGraphics(TerrainData data, TerrainStyle style) {
            var ground_style = style.groundStyle;
            if (!ground_style.isValid) {
                Debug.LogError("GroundStyle is not valid!");
                return 0;
            }

            float loop_ratio = (data.region.width - ground_style.edgeSize) / ground_style.loopSize;
            int loop_cnt = (int)Mathf.Ceil(loop_ratio - ground_style.minimumValidLoopTile);

            float accumulate_size = 0;
            tiles_.Clear();

            tiles_.Add(CreateGroundTile(ground_style.leftTile, ground_style, accumulate_size));
            accumulate_size += ground_style.leftEdgeSize;
            for (int i = 0; i < loop_cnt; i++) {
                tiles_.Add(CreateGroundTile(ground_style.RandomSelectLoopTile(), ground_style, accumulate_size));
                accumulate_size += ground_style.loopSize;
            }
            tiles_.Add(CreateGroundTile(ground_style.rightTile, ground_style, accumulate_size));
            accumulate_size += ground_style.rightEdgeSize;

            return accumulate_size;
        }

        protected override void Init() {
            // Sets to topleft corner
            Rect rect = (Rect)data.region;
            transform.localPosition = new Vector2(rect.xMin, rect.yMax);
        }
        protected override void Cleanup() {
            base.Cleanup();

            for (int i = 0; i < tiles_.Count; i++) {
                PoolManager.Deallocate(tiles_[i]);
            }
            tiles_.Clear();
        }

        protected override void Awake() {
            base.Awake();
            collider_ = GetComponent<BoxCollider2D>();
        }

        private SpriteRenderer CreateGroundTile(SpriteRenderer tile, TerrainStyle.GroundStyle style, float offset) {
            var new_tile = PoolManager.Allocate(tile);
            new_tile.transform.parent = graphics;
            new_tile.transform.localScale = style.scale;
            new_tile.transform.localPosition = new Vector3(offset, 0, 0);
            return new_tile;
        }
    }
}