using UnityEngine;
using System.Collections.Generic;

namespace GirlDash {
    public class EnemyQueue {
        // sorted enemies, in ascending order of x
        private List<Enemy> sorted_enemies_ = new List<Enemy>();

        public void Clear() {
            sorted_enemies_.Clear();
        }

        public void NewEnemy(Enemy enemy) {
            for (int i = sorted_enemies_.Count - 1; i >= 0; i--) {
                if (enemy.transform.position.x >= sorted_enemies_[i].transform.position.x) {
                    sorted_enemies_.Insert(i + 1, enemy);
                    return;
                }
            }
            sorted_enemies_.Insert(0, enemy);
        }

        public void RemoveEnemy(Enemy enemy) {
            sorted_enemies_.Remove(enemy);
        }

        public void Update(float progress) {
            // Remove s the enemies that is behind the player.
            while (sorted_enemies_.Count > 0) {
                if (sorted_enemies_[0].position < progress) {
                    sorted_enemies_.RemoveAt(0);
                } else {
                    break;
                }
            }
        }

        public Enemy GetClosestEnemy() {
            return sorted_enemies_.Count > 0 ? sorted_enemies_[0] : null;
        }

        public Enemy GetClosestVisibleEnemy() {
            if (sorted_enemies_.Count > 0) {
                if (sorted_enemies_[0].IsVisible) {
                    return sorted_enemies_[0];
                }
            }
            return null;
        }
    }
}