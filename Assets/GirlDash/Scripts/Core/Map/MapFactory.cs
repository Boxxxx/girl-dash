using UnityEngine;
using System.Collections;

namespace GirlDash.Map {
    public class MapFactory : MonoBehaviour {
        [System.Serializable]
        public struct Terrains {
            public TerrainStyle style;
            public GroundComponent ground;
        }
        [System.Serializable]
        public struct Enemies {
            public Enemy dog;
            public Enemy scout;
            public Enemy pioneer;
            public Enemy shield;
            public Enemy bomber;
        }

        public Terrains terrains;
        public Enemies enemies;

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
            Enemy new_enemy;
            switch (data.enemyType) {
                case EnemyData.EnemyType.Dog:
                    new_enemy = PoolManager.Allocate(enemies.dog);
                    break;
                case EnemyData.EnemyType.Scout:
                    new_enemy = PoolManager.Allocate(enemies.scout);
                    break;
                case EnemyData.EnemyType.Pioneer:
                    new_enemy = PoolManager.Allocate(enemies.pioneer);
                    break;
                case EnemyData.EnemyType.Shield:
                    new_enemy = PoolManager.Allocate(enemies.shield);
                    break;
                case EnemyData.EnemyType.Bomber:
                    new_enemy = PoolManager.Allocate(enemies.bomber);
                    break;
                default:
                    throw new System.NotImplementedException("Not implemented enemy type: " + data.enemyType);
            }
            new_enemy.SpawnSelf(data, parent_transform);
            return new_enemy;
        }
    }
}