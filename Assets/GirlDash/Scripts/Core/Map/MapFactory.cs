using UnityEngine;
using System.Collections;

namespace GirlDash.Map {
    public class MapFactory : MonoBehaviour {
        public GroundComponent ground;

        public TerrainComponent CreateTerrain(TerrainData data) {
            switch (data.terrainType) {
                case TerrainData.TerrainType.Ground:
                    return PoolManager.Allocate(ground);
                default:
                    throw new System.NotImplementedException("Not implemented terrain type: " + data.terrainType);
            }
        }
    }
}