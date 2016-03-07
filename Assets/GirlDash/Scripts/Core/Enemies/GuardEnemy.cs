using UnityEngine;
using System.Collections;

namespace GirlDash {
    public class GuardEnemy : Enemy {
        public const float kActiveDistance = 10;

        protected override void Action() {
            // no-op
        }

        protected override bool CheckActive() {
            return LocationUtils.GetDistToPlayer(transform) <= kActiveDistance;
        }

        protected override void HitByDamageArea(DamageArea damage_area) {
            Bullet bullet = damage_area.GetComponent<Bullet>();
            if (bullet == null) {
                base.HitByDamageArea(damage_area);
            } else {
                bullet.Reflect(characterData.atk, DamageArea.DamageGroup.Enemy);
            }
        }
    }
}