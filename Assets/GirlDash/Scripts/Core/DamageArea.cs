using UnityEngine;
using System.Collections;

namespace GirlDash {
    [RequireComponent(typeof(Collider2D))]
    public class DamageArea : MonoBehaviour {
        public enum DamageGroup {
            Player,
            Enemy
        }
        private static int id_counter = 0;

        public int uniqueId {
            get; private set;
        }
        public int damage {
            get; private set;
        }
        public DamageGroup damageGroup {
            get; protected set;
        }

        public void Reset(int damage, DamageGroup group) {
            uniqueId = ++id_counter;
            this.damage = damage;
            this.damageGroup = group;
        }

        public virtual void OnTakeDamage(CharacterController character) {}
    }
}