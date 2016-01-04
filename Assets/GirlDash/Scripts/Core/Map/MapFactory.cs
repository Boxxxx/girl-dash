using UnityEngine;
using System.Collections;

namespace GirlDash.Map {
    public class MapFactory : MonoBehaviour {
        public GroundComponent ground;

        public TerrainComponent CreateTerrain(TerrainData data) {
            TerrainComponent comp;
            switch (data.terrainType) {
                case TerrainData.TerrainType.Ground:
                    return comp = PoolManager.Allocate(ground);
                default:
                    throw new System.NotImplementedException("Not implemented terrain type: " + data.terrainType);
            }
        }
    }
}