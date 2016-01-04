using UnityEngine;
using System.Collections.Generic;

namespace GirlDash.Map {
    [System.Serializable]
    public class TerrainData {
        public enum TerrianType {
            Ground
        }
        public enum InteractiveType {
            Solid,
            Harmful,
            Oneshot
        }
        public TerrianType terrianType = TerrianType.Ground;
        public InteractiveType interactiveType = InteractiveType.Solid;

        public MapRect region;
    }

    [System.Serializable]
    public class EnemyData {
        public enum EnemyType {
            Scout,
            Pioneer,
            Shield,
            Dog,
            Bomber
        }
        public EnemyType enemyType = EnemyType.Scout;
        public int hp = 1;
        public int atk = 1;
        public MapVector spawnPosition;
    }

    [System.Serializable]
    public class WidgetData {
        public enum WidgetType {
            Decorator
        }
        public WidgetType widgetType = WidgetType.Decorator;
        public MapVector spawnPosition;
    }

    [System.Serializable]
    public class BlockData {
        public MapRect bound;
        public List<TerrainData> terrians = new List<TerrainData>();
        public List<EnemyData> enemies = new List<EnemyData>();
        public List<WidgetData> widgets = new List<WidgetData>();
    }

    /// <summary>
    /// The region of map:
    /// 1. left border: the left side of intial camera view.
    /// 2. right border: left border + width
    /// 3. 0 height: the default ground (although there is no ground by default)
    /// 4. top is unlimited.
    /// 5. bottom is limited by deadHeight
    /// </summary>
    [System.Serializable]
    public class MapData {
        /// <summary>
        /// sight range of this map, usually it's the upper bound of screen width.
        /// </summary>
        public MapValue sightRange;
        /// <summary>
        /// width of map area, the left and right border will be blocked by wall
        /// </summary>
        public MapValue width;
        /// <summary>
        /// deadHeight of map area, there will be a deadArea at this height
        /// </summary>
        public MapValue deadHeight;
        /// <summary>
        /// A series of blocks that forms this map, it's used to preload and recycle the resources.
        /// 
        /// For example, we load 5 blocks, and recycle a block when it's totally out of sight,
        /// and dynamically build a new block so that the total number of blocks are 5.
        /// Therefore, we can have unlimited map blocks.
        /// </summary>
        public List<BlockData> blocks = new List<BlockData>();
    }
}
