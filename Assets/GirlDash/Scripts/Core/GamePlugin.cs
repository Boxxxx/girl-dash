using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class GamePlugin : MonoBehaviour, IGameComponent {
        protected GameController controller_;

        public bool debugMode {
            get { return controller_.debugMode; }
        }
        public bool isPlaying {
            get { return controller_.isPlaying; }
        }
        public float progress {
            get { return controller_.realProgress; }
        }
        public PlayerController player {
            get { return controller_.playerController; }
        }

        protected virtual void Awake() {
            controller_ = GameController.Instance;
            controller_.Register(this);
        }

        protected virtual void OnDestroy() {
            controller_.Unregister(this);
        }

        public virtual void GameReady() {}
        public virtual void GameStart() {}
        public virtual void GameOver() {}
        public virtual void GameReset() {}
    }
}
