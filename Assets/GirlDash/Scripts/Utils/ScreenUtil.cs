using UnityEngine;
using GirlDash.UI;

namespace GirlDash {
    public static class ScreenUtil {
        public static Bounds CalculateOrthographicBounds(Camera camera) {
            float screen_aspect = Screen.width / (float)Screen.height;
            float camera_height = camera.orthographicSize * 2;
            Bounds bounds = new Bounds() {
                center = new Vector3(camera.transform.position.x, camera.transform.position.y, 0),
                size = new Vector3(camera_height * screen_aspect, camera_height, 0)
            };
            return bounds;
        }

        public static Vector2 WorldPosToUI(Vector3 position) {
            Vector2 pos = GameController.Instance.cameraController.camera
                .WorldToScreenPoint(position);
            return BattleUIController.Instance.transform.InverseTransformPoint(
                BattleUIController.Instance.camera.ScreenToWorldPoint(pos));
        }

        public static Vector3 UILocalToWorld(Vector2 ui_pos) {
            return BattleUIController.Instance.transform.TransformPoint(ui_pos);
        }
    }
}