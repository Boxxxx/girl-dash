using UnityEngine;
using System.Collections;

namespace GirlDash {
    [RequireComponent(typeof(Collider2D))]
    public class DamageArea : MonoBehaviour {
        private static int id_counter = 0;

        public int uniqueId {
            get; private set;
        }
        public int damage {
            get; private set;
        }

        public void Reset(int damage) {
            uniqueId = ++id_counter;
            this.damage = damage;
        }
    }
}