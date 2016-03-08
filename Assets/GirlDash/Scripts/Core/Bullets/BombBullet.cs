using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class BombBullet : Bullet {
        public new ParticleSystem particleSystem;

        protected override void OnHitGround() {
            // no-op
        }

        void OnEnable() {
            particleSystem.Play();
        }
    }
}