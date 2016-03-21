using UnityEngine;
using System;

namespace GirlDash.UI {
    public class UICountdown : MonoBehaviour {
        public float countdownTime = 5f;
        public UILabel countdownLabel;
        public Action finishCB;

        private float current_time_;

        void OnEnable() {
            current_time_ = countdownTime;
        }

        void Update() {
            current_time_ -= Time.unscaledDeltaTime;
            countdownLabel.text = Mathf.CeilToInt(current_time_).ToString();
            if (current_time_ <= 0) {
                if (finishCB != null) {
                    finishCB();
                }
                gameObject.SetActive(false);
            }
        }
    }
}