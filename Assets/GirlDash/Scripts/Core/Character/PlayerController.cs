using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class PlayerController : CharacterController, IGameComponent {
        private bool running_ = false;

        public IEnumerator Load(CharacterData character_data) {
            Reset(character_data);
            yield return null;
        }

        public void GameStart() {
            running_ = true;
        }

        public void GameOver() {
            Move(0);
            running_ = false;
        }

        protected override void FixedUpdate() {
            if (running_) {
                base.FixedUpdate();
            }
        }
    }
}
