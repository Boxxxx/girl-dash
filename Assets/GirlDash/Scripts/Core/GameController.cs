using UnityEngine;
using GirlDash.Map;

namespace GirlDash {
    public class GameController : MonoBehaviour {
        public StartingLine startingLine;
        public CharacterController heroController;
        public CameraController cameraController;
        public MapManager mapManager;

        public float progress;
        public bool move = true;

        void Update() {
            progress = startingLine.GetOffset(heroController.transform).x;

            mapManager.UpdateProgress(progress);

            if (move) {
                // Force to move right.
                heroController.Move(1);
            } else {
                heroController.Move(0);
            }
        }
    }
}