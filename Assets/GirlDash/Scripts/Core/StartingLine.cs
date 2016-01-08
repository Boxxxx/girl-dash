using UnityEngine;
using GirlDash.Map;

namespace GirlDash {
    public class StartingLine : MonoBehaviour {
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
    }
}