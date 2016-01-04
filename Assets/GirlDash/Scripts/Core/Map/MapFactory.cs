using UnityEngine;
using System.Collections;

namespace GirlDash.Map {
    public class MapFactory : MonoBehaviour {
        public GroundComponent ground;

        public TerrianComponent CreateTerrian(TerrainData data) {
            TerrianComponent comp;
            switch (data.terrianType) {
                case TerrainData.TerrianType.Ground:
                    return comp = PoolManager.Allocate(ground);
                default:
                    throw new System.NotImplementedException("Not implemented terrian type: " + data.terrianType);
            }
        }
    }
}