using UnityEngine;
using System.Collections.Generic;

namespace GirlDash.Map {
    [System.Serializable]
    public class TerrainData {
        public enum TerrainType {
            Ground,
            Widget
        }
        public enum InteractiveType {
            Solid,
            Harmful,
            Oneshot
        }
        public TerrainType terrainType = TerrainType.Ground;
        public InteractiveType interactiveType = InteractiveType.Solid;

        // Consider this terrain compoent take the whole region area, from topleft to bottomright.
        public MapRect region;
        public MapVector center {
            get { return new MapVector(region.x + (region.width >> 1), region.y + (region.height >> 1)); }
        }
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
        public int fire_atk = 1;
        public int hit_atk = 1;

        // Since the MapVector is always integer and the ground component takes 1x1 block from integer coordinate,
        // if we want to put a enemy in the center of a ground block, it must have decimal coordinate, which is difficult to maintain.
        // Thus, we only set spawnPosition to integer coordinate,
        // however what it really means is the center of the 1 unit start from this spawnPosition.
        public MapVector spawnPosition;
    }

    [System.Serializable]
    public class BlockData {
        public MapVector bound;
        public TerrainData[] terrains = new TerrainData[] { };
        public EnemyData[] enemies = new EnemyData[] { };
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
        /// Sight range of this map.
        /// Suppose the player is at position (0, 0), the sightRange shows the range which the player can see. both left and right border are inclusive.
        /// 
        /// For example, the sightRange is [-5, 5], and the player is at 3, total sight view is [-2, 8].
        /// To make it safer, we often set both left & right to the upper bound of width of screen.
        /// </summary>
        public MapVector sightRange;
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

        public MapValue rightmost {
            get { return blocks.Count > 0 ? blocks[blocks.Count - 1].bound.max : 0; }
        }

        public MapValue leftmost {
            get { return blocks.Count < 0 ? blocks[0].bound.min : 0; }
        }

        public MapValue width {
            get { return blocks.Count > 0 ? blocks[blocks.Count - 1].bound.max - blocks[0].bound.min : 0; }
        }
    }
}
