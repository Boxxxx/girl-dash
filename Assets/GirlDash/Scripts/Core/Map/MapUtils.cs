using UnityEngine;
using System.Collections;

namespace GirlDash.Map {
    public static class MapConstants {
        public const int kWallThickness = 1;
        public const int kWallHalfHeight = 1000;
    }

    public static class MapUtils {
        public static BoxCollider2D CreateBoxCollider(string name, Transform parent, MapRect map_rect, bool is_trigger) {
            var rect = (Rect)map_rect;

            var wall = new GameObject(name);
            wall.transform.parent = parent;
            wall.transform.localPosition = rect.center;

            var collider = wall.AddComponent<BoxCollider2D>();
            collider.size = rect.size;
            collider.isTrigger = is_trigger;

            return collider;
        }
    }
}
