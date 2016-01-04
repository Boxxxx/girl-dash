using UnityEngine;
using UnityEditor;

namespace GirlDash.Map {
    public class TerrianStyle : ScriptableObject {
        public Transform groundUnitSprite;

        [MenuItem("Assets/Create/GirlDash/TerrianStyle")]
        public static void CreateAsset() {
            ScriptableObjectUtility.CreateAsset<TerrianStyle>();
        }
    }
}
