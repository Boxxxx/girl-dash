using UnityEngine;
using GirlDash.Map;

namespace GirlDash {
    public class StartingLine : SingletonObject<StartingLine> {
        public Vector2 GetOffset(Vector2 position) {
            return position - (Vector2)transform.position;
        }

        public Vector2 GetOffset(Transform transform) {
            return GetOffset(transform.position);
        }

        public MapVector GetMapPosition(Vector2 position) {
            var offset = GetOffset(transform);
            return new MapVector(MapValue.RoundValue(offset.x), MapValue.RoundValue(offset.y));
        }

        public MapVector GetMapPosition(Transform transform) {
            return GetMapPosition(transform.position);
        }

        // Convert the mapVector to global unity 2d position.
        public Vector2 GetGlobalPosition(MapVector vector) {
            return (Vector2)transform.position + (Vector2)vector;
        }

        public float ProgressToGlobalX(float progress) {
            return transform.position.x + progress;
        }
    }
}