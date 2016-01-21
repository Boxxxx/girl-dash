using UnityEditor;

namespace GirlDash.Map {
    public static class TerrainStyleEditor {
        [MenuItem("Assets/Create/GirlDash/terrainStyle")]
        public static void CreateAsset() {
            ScriptableObjectUtility.CreateAsset<TerrainStyle>();
        }
    }
}