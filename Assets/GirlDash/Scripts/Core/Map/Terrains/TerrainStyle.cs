using UnityEngine;

namespace GirlDash.Map {
    public class TerrainStyle : ScriptableObject {
        [System.Serializable]
        public class GroundStyle {
            public SpriteRenderer leftTile;
            public SpriteRenderer rightTile;
            // Notice all sprite in 'loopTiles' should be the same width, otherwise,
            // we will get the first one's width as the width of others.
            public SpriteRenderer[] loopTiles;

            // See comments of method BuildGroundSmart() in GroundComponent.
            public float minimumValidLoopTile = 0.5f;

            public Vector2 scale = new Vector2(1, 1);

            public bool isValid {
                get { return leftTile != null && rightTile != null && loopTiles.Length > 0; }
            }

            public float edgeSize {
                get {
                    return leftEdgeSize + rightEdgeSize;
                }
            }

            public float leftEdgeSize {
                get {
                    // Assume the pivot is at the topleft corner of real image, we must minus it out from out size.
                    return (leftTile.sprite.rect.width - leftTile.sprite.pivot.x) / leftTile.sprite.pixelsPerUnit * scale.x;
                }
            }

            public float rightEdgeSize {
                get {
                    return rightTile.sprite.rect.width / rightTile.sprite.pixelsPerUnit * scale.x;
                }
            }

            // Just return the  first loopTile's width.
            public float loopSize {
                get {
                    return loopTiles[0].sprite.rect.width / loopTiles[0].sprite.pixelsPerUnit * scale.x;
                }
            }

            public SpriteRenderer RandomSelectLoopTile() {
                return loopTiles[Random.Range(0, loopTiles.Length)];
            }
        }
        public GroundStyle groundStyle = new GroundStyle();
    }
}
