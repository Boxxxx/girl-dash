using UnityEngine;
using System;
using GirlDash;

namespace GirlDash.UI {
    public class UIGameProgress : MonoBehaviour {
        public UILabel maxProgress;
        public UILabel currentProgress;

        public void ResetAll() {
            Set(maxProgress, (int)RuntimeData.maxProgress);
            Set(currentProgress, (int)RuntimeData.currentProgress);
        }

        void Update() {
            Set(currentProgress, (int)RuntimeData.currentProgress);
        }

        void Set(UILabel label, int progress) {
            string num_progress = progress.ToString("0000000000");  // 10 digits
            label.text = num_progress;
        }
    }
}
