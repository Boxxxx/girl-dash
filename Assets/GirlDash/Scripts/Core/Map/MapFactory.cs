using UnityEngine;
using System.Collections.Generic;

namespace GirlDash.Map {
    public class MapFactory : MonoBehaviour {
        [System.Serializable]
        public struct EnemyBundle {
            public Enemy prefab;
            public EnemyData enemyData;
        }
        [System.Serializable]
        public struct Terrains {
            public TerrainStyle style;
            public GroundComponent ground;
        }

        public Terrains terrains;
        public List<EnemyBundle> enemies;
        private Dictionary<EnemyData.EnemyType, EnemyBundle> enemies_map_;

        public TerrainComponent CreateTerrain(TerrainData data, Transform parent_transform) {
            TerrainComponent terrain;
            switch (data.terrainType) {
                case TerrainData.TerrainType.Ground:
                    terrain = PoolManager.Allocate(terrains.ground);
                    break;
                default:
                    throw new System.NotImplementedException("Not implemented terrain type: " + data.terrainType);
            }

            terrain.BuildSelf(data, terrains.style, parent_transform);
            return terrain;
        }

        public Enemy CreateEnemy(EnemyData data, Transform parent_transform) {
            EnemyBundle enemy_bundle;
            if (enemies_map_.TryGetValue(data.enemyType, out enemy_bundle)) {
                Enemy new_enemy = PoolManager.Allocate(enemy_bundle.prefab);
                new_enemy.SpawnSelf(data, parent_transform);
                return new_enemy;
            } else {
                throw new System.NotImplementedException("Not implemented enemy type: " + data.enemyType);
            }
        }

        public EnemyData GetDefaultEnemyData(EnemyData.EnemyType enemy_type) {
            EnemyBundle enemy_bundle;
            if (enemies_map_.TryGetValue(enemy_type, out enemy_bundle)) {
                return enemy_bundle.enemyData;
            }
            else {
                throw new System.NotImplementedException("Not implemented enemy type: " + enemy_type);
            }
        }

        void Awake() {
            enemies_map_ = new Dictionary<EnemyData.EnemyType, EnemyBundle>();
            for (int i = 0; i < enemies.Count; i++) {
                enemies_map_.Add(enemies[i].enemyData.enemyType, enemies[i]);
            }
        }
    }
}