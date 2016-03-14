using UnityEngine;

namespace GirlDash.UI {
    public class UIHpBar : MonoBehaviour {
        public UIFollower follower;
        public UIProgressBar progressBar;

        private CharacterController character_;

        public void Reset(CharacterController character) {
            character_ = character;
            follower.Follow(character.uiPosition, Vector2.zero);
        }

        void Update() {
            progressBar.value = character_.hpRatio;
        }
    }
}