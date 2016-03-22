using UnityEngine;

namespace GirlDash.UI {
    public class UIStartPanel : MonoBehaviour {
        public UILabel maxProgress;
        public UILabel gameOver;
        public UICountdown countdown;

        private bool is_game_starting_ = false;

        public void ShowStart() {
            gameObject.SetActive(true);
            gameOver.gameObject.SetActive(false);
            maxProgress.text = ((int)RuntimeData.maxProgress).ToString("0000000000");  // 10 digits
            is_game_starting_ = false;
        }

        public void ShowGameOver() {
            gameObject.SetActive(true);
            gameOver.gameObject.SetActive(true);
            maxProgress.text = ((int)RuntimeData.maxProgress).ToString("0000000000");  // 10 digits
            is_game_starting_ = false;
        }

        public void StartGame() {
            // Only one shot in each show.
            if (is_game_starting_) {
                return;
            }
            is_game_starting_ = true;
            StartCoroutine(GameController.Instance.ResetGameAsync(() => {
                gameObject.SetActive(false);
                countdown.finishCB = () => {
                    GameController.Instance.StartGame();
                };
                countdown.gameObject.SetActive(true);
            }));
        }
    }
}