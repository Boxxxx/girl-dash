using UnityEngine;
using System.Collections.Generic;

namespace GirlDash.Map {
    public class MapBlock {
        public List<TerrainComponent> terrains = new List<TerrainComponent>();

        public BlockData blockData {
            get; private set;
        }
        public MapVector bound {
            get { return blockData.bound; }
        }

        public MapBlock(BlockData block_data, Transform parent_transform, MapFactory factory) {
            blockData = block_data;

            for (int i = 0; i < blockData.terrains.Length; i++) {
                var terrain = factory.CreateTerrain(blockData.terrains[i]);
                terrain.BuildSelf(blockData.terrains[i], parent_transform);
                terrains.Add(terrain);
            }
        }

        public void RecycleSelf() {
            for (int i = 0; i < terrains.Count; i++) {
                terrains[i].RecycleSelf();
            }
            terrains.Clear();
        }
    }

    [RequireComponent(typeof(MapFactory))]
    public class MapManager : MonoBehaviour {
        [Tooltip("Minimum of number of blocks that is cached.")]
        public int minBlocksToCacheup = 5;
        public Transform terrainFolder;

        /// <summary>
        /// What's really needed here is a dequeue, but we have only a couple of blocks (less than 10) in this group,
        /// so we just use a list to simulate it.
        /// </summary>
        private List<MapBlock> blocks_ = new List<MapBlock>();

        public MapData mapData { get; private set; }

        public float progress { get; private set; }
        public float sightRange { get; private set; }

        private MapFactory map_factory;
        private Collider2D dead_area;
        private int next_block_index_;

        private void InitBoundingColliders() {
            MapUtils.CreateBoxCollider(
                "wallLeft", terrainFolder,
                new MapRect(-MapConstants.kWallThickness, -MapConstants.kWallHalfHeight, MapConstants.kWallThickness, MapConstants.kWallHalfHeight << 1),
                false /* is_trigger */);
            MapUtils.CreateBoxCollider(
                "wallRight", terrainFolder,
                new MapRect(mapData.width, -MapConstants.kWallHalfHeight, MapConstants.kWallThickness, MapConstants.kWallHalfHeight << 1),
                false /* is_trigger */);
        }
        private void InitDeadArea() {
            dead_area = MapUtils.CreateBoxCollider(
                "deadArea", terrainFolder,
                new MapRect(0, mapData.deadHeight - MapConstants.kWallThickness, mapData.width, MapConstants.kWallThickness),
                false /* is_trigger */);
        }
        private void InitialBuild() {
            InitBoundingColliders();
            InitDeadArea();

            RefreshBlocks();
        }

        public void Reset(MapData map_data) {
            mapData = map_data;

            // Reset varialbes
            progress = 0;
            next_block_index_ = 0;

            // Clear all blocks
            for (int i = 0; i < blocks_.Count; i++) {
                blocks_[i].RecycleSelf();
            }
            blocks_.Clear();

            // Clear all folders
            ClearFolder(terrainFolder);

            // Initial build
            InitialBuild();
        }

        public void UpdateProgress(float new_progress) {
            progress = Mathf.Max(progress, new_progress);
        }

        private void RefreshBlocks() {
            MapValue left_bound = MapValue.LowerBound(progress);
            MapValue right_bound = MapValue.UpperBound(progress + (float)mapData.sightRange);

            // Step1: Try to recycle the out-of-sight block in left side.
            while (blocks_.Count > 0) {
                var block = blocks_[0];
                if (block.bound.x < left_bound) {
                    // If this leftmost block is out of sight, recycle it.
                    block.RecycleSelf();
                    blocks_.RemoveAt(0);
                } else {
                    // Otherwise, all remains are in sight.
                    break;
                }
            }

            // Step2: Cache up to 'minBlocksToCacheup'
            while (blocks_.Count < minBlocksToCacheup) {
                if (!CacheupNextBlock()) {
                    // If there is no next block, just return
                    return;
                }
            }

            // Step3: Try to add the right block that is going to in sight.
            while (blocks_.Count > 0) {
                var block = blocks_[blocks_.Count - 1];
                if (block.bound.max < right_bound) {
                    // If the rightmost block is not enough to cover all sight range, add a new one.
                    if (!CacheupNextBlock()) {
                        // If there is no next block, just return.
                        return;
                    }
                } else {
                    // Otherwise, the cached blocks covered all sight.
                    break;
                }
            }
        }

        private bool CacheupNextBlock() {
            if (next_block_index_ >= mapData.blocks.Count) {
                return false;
            }
            blocks_.Add(new MapBlock(mapData.blocks[next_block_index_++], terrainFolder, map_factory));
            return true;
        }

        private void ClearFolder(Transform folder) {
            for (int i = 0; i < folder.childCount; i++) {
                var child = folder.GetChild(i);
                GameObject.Destroy(child);
            }
        }

        void Awake() {
            map_factory = GetComponent<MapFactory>();

            Reset(CreateMockMapData());
        }

        /// <summary>
        /// For test only
        /// </summary>
        private MapData CreateMockMapData() {
            SimpleMapBuilder builder = new SimpleMapBuilder(new SimpleMapBuilder.Options());

            int num_ground = 10;
            MapVector random_ground_width_range = new MapVector(3, 10);
            MapVector random_ground_offset_range = new MapVector(0, 3);

            builder.NewGround(
                0, Random.Range(Mathf.Max(7, random_ground_width_range.x), random_ground_width_range.y));
            for (int i = 1; i < num_ground; i++) {
                builder.NewGround(
                    Random.Range(random_ground_offset_range.x, random_ground_offset_range.y),
                    Random.Range(random_ground_width_range.x, random_ground_width_range.y));
            }

            return builder.Build();
        }
    }
}