using UnityEngine;
using System.Collections;

namespace GirlDash {
    [System.Serializable]
    public class CharacterData {
        public string name = "StenMK2";
        public float hp = 5;

        public float moveSpeed = 4.0f;

        public int maxJumpCnt = 2;
        public float jumpInitForce = 700.0f;
        public float doubleJumpSpeed = 12.0f;
        public float jumpCooldown = 0f;

        public float atk = 0.2f;
        public float fireCooldown = 0.1f;
    }
}