using UnityEngine;
using System.Collections.Generic;

namespace GirlDash.Map {
    public class MapBlock {
        public List<TerrianComponent> terrians = new List<TerrianComponent>();

        public BlockData blockData {
            get; private set;
        }
        public MapRect bound {
            get { return blockData.bound; }
        }

        public MapBlock(BlockData block_data, MapFactory factory) {
            for (int i = 0; i < blockData.terrians.Count; i++) {
                terrians.Add(factory.CreateTerrian(blockData.terrians[i]));
            }
        }

        public void RecycleSelf() {
            for (int i = 0; i < terrians.Count; i++) {
                terrians[i].RecycleSelf();
            }
            terrians.Clear();
        }
    }

    [RequireComponent(typeof(MapFactory))]
    public class MapManager : MonoBehaviour {
        [Tooltip("Minimum of number of blocks that is cached.")]
        public int minBlocksToCacheup = 5;
        public Transform terrianFolder;

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
                "wallLeft", terrianFolder,
                new MapRect(-MapConstants.kWallThickness, -MapConstants.kWallHalfHeight, MapConstants.kWallThickness, MapConstants.kWallHalfHeight << 1),
                false /* is_trigger */);
            MapUtils.CreateBoxCollider(
                "wallRight", terrianFolder,
                new MapRect(mapData.width, -MapConstants.kWallHalfHeight, MapConstants.kWallThickness, MapConstants.kWallHalfHeight << 1),
                false /* is_trigger */);
        }
        private void InitDeadArea() {
            dead_area = MapUtils.CreateBoxCollider(
                "deadArea", terrianFolder,
                new MapRect(0, mapData.deadHeight - MapConstants.kWallThickness, mapData.width, MapConstants.kWallThickness),
                false /* is_trigger */);
        }
        private void InitialBuild() {
            InitBoundingColliders();
            InitDeadArea();

            RefreshBlocks();
        }

        public void Reset(MapData map_data) {
            // Reset varialbes
            progress = 0;
            next_block_index_ = 0;

            // Clear all blocks
            for (int i = 0; i < blocks_.Count; i++) {
                blocks_[i].RecycleSelf();
            }
            blocks_.Clear();

            // Clear all folders
            ClearFolder(terrianFolder);

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
                if (block.bound.xMax < left_bound) {
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
                if (block.bound.xMax < right_bound) {
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
            blocks_.Add(new MapBlock(mapData.blocks[next_block_index_++], map_factory));
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
            var map_data = new MapData();
            map_data.width = 21;
            map_data.deadHeight = -1;

            var block_data = new BlockData();

            var terrain_data = new GirlDash.Map.TerrainData();
            terrain_data.terrianType = TerrainData.TerrianType.Ground;
            terrain_data.region = 

            map_data.blocks.Add(blocks_data);

            return map_data;
        }
    }
}