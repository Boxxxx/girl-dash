using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class GuardEnemy : Enemy {
        public float kActiveDistance = 10;
        public int reflectNum = 1;
        public ParticleSystem shieldParticle;

        private int remain_reflect_num_ = 0;

        protected override void Action() {
            // no-op
        }

        protected override bool CheckActive() {
            return LocationUtils.GetDistToPlayer(transform) <= kActiveDistance;
        }

        protected override void HitByDamageArea(DamageArea damage_area) {
            Bullet bullet = damage_area.GetComponent<Bullet>();
            if (bullet == null || remain_reflect_num_ <= 0) {
                base.HitByDamageArea(damage_area);
            } else {
                if ((--remain_reflect_num_) == 0) {
                    shieldParticle.Stop();
                }
                bullet.Reflect(characterData.atk, DamageArea.DamageGroup.Enemy);
            }
        }

        void OnEnable() {
            remain_reflect_num_ = reflectNum;
            shieldParticle.Play();
        }
    }
}