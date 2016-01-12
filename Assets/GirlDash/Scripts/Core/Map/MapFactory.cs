using UnityEngine;
using System.Collections;

namespace GirlDash.Map {
    public class MapFactory : MonoBehaviour {
        public GroundComponent ground;
        public Enemy dog;

        public TerrainComponent CreateTerrain(TerrainData data) {
            switch (data.terrainType) {
                case TerrainData.TerrainType.Ground:
                    return PoolManager.Allocate(ground);
                default:
                    throw new System.NotImplementedException("Not implemented terrain type: " + data.terrainType);
            }
        }

        public Enemy CreateEnemy(EnemyData data) {
            Enemy new_enemy;
            switch (data.enemyType) {
                case EnemyData.EnemyType.Dog:
                    new_enemy = PoolManager.Allocate(dog);
                    break;
                default:
                    throw new System.NotImplementedException("Not implemented enemy type: " + data.enemyType);
            }
            new_enemy.Reset(data);
            return new_enemy;
        }
    }
}