using UnityEngine;
using UnityEditor;

namespace GirlDash.Map {
    public class TerrainStyle : ScriptableObject {
        public Transform groundUnitSprite;

        [MenuItem("Assets/Create/GirlDash/terrainStyle")]
        public static void CreateAsset() {
            ScriptableObjectUtility.CreateAsset<TerrainStyle>();
        }
    }
}
