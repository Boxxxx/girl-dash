using UnityEngine;
using System.Collections;

namespace GirlDash.UI {
    public class UIStartPanel : MonoBehaviour {
        public UILabel maxProgress;
        public UICountdown countdown;

        public void Show() {
            gameObject.SetActive(true);
            maxProgress.text = ((int)RuntimeData.maxProgress).ToString("0000000000");  // 10 digits
        }

        public void GetOff() {
            GameController.Instance.Reset();
            gameObject.SetActive(false);
            countdown.finishCB = () => {
                GameController.Instance.GetOff();
            };
            countdown.gameObject.SetActive(true);
        }

        void Start() {
            Show();
        }
    }
}