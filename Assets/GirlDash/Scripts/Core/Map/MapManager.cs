using UnityEngine;
using System.Collections;
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
    public class MapManager : MonoBehaviour, IGameComponent {
        [Tooltip("Minimum of number of blocks that is cached.")]
        public int minBlocksToCacheup = 3;
        public bool is_infinity = true;
        public Transform terrainFolder;

        /// <summary>
        /// What's really needed here is a dequeue, but we have only a couple of blocks (less than 10) in this group,
        /// so we just use a list to simulate it.
        /// </summary>
        private List<MapBlock> map_blocks = new List<MapBlock>();

        public MapData mapData { get; private set; }

        public float progress { get; private set; }
        public float sightRange { get; private set; }

        private BoxCollider2D dead_area_;
        private BoxCollider2D left_wall_;
        private BoxCollider2D right_wall_;

        private MapFactory map_factory_;
        private MapGeneratorAsync map_generator_;
        private int next_block_index_;

        private void InitBoundingColliders() {
            left_wall_ = MapUtils.CreateBoxCollider(
                "WallLeft", terrainFolder,
                new MapRect(-MapConstants.kWallThickness + mapData.leftBorder, -MapConstants.kWallHalfHeight, MapConstants.kWallThickness, MapConstants.kWallHalfHeight << 1),
                false /* is_trigger */);
            right_wall_ = MapUtils.CreateBoxCollider(
                "WallRight", terrainFolder,
                new MapRect(mapData.rightBorder, -MapConstants.kWallHalfHeight, MapConstants.kWallThickness, MapConstants.kWallHalfHeight << 1),
                false /* is_trigger */);
        }
        private void InitDeadArea() {
            dead_area_ = MapUtils.CreateBoxCollider(
                "DeadArea", terrainFolder,
                new MapRect(0, mapData.deadHeight - MapConstants.kWallThickness, MapConstants.kInfinity, MapConstants.kWallThickness),
                false /* is_trigger */);
            dead_area_.gameObject.layer = LayerMask.NameToLayer(Consts.kGroundLayer);
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
            for (int i = 0; i < map_blocks.Count; i++) {
                map_blocks[i].RecycleSelf();
            }
            map_blocks.Clear();

            // Clear all folders
            ClearFolder(terrainFolder);

            // Initial build
            InitialBuild();
        }

        public void UpdateProgress(float new_progress) {
            if (new_progress > progress) {
                progress = new_progress;
                RefreshBlocks();
            }
        }

        public IEnumerator Load(MapGeneratorAsync map_generator) {
            map_factory_ = GetComponent<MapFactory>();
            map_generator_ = map_generator;
            map_generator_.Start();
            Reset(map_generator.InitMapData());
            yield return null;
        }

        public void GameStart() { }

        public void GameOver() { }

        private void RefreshBlocks() {
            // Calcualtes the left and right bound value in map resolution,
            // use lower as left and upper as right to make sure it includes the whole sight view.
            MapValue left_bound = MapValue.LowerBound(progress) + mapData.sightRange.min;
            MapValue right_bound = MapValue.UpperBound(progress) + mapData.sightRange.max;

            // Step1: Try to recycle the out-of-sight block in left side.
            while (map_blocks.Count > 0) {
                var block = map_blocks[0];
                if (block.bound.max < left_bound) {
                    Debug.Log(
                        string.Format("[MapManger] Going to recycle the blocks at [{0}, {1}], the progress now is {2}",
                        block.bound.min, block.bound.max, progress));
                    // If this leftmost block is out of sight, recycle it.
                    block.RecycleSelf();
                    map_blocks.RemoveAt(0);
                } else {
                    // Otherwise, all remains are in sight.
                    break;
                }
            }

            // Step2: Cache up to 'minBlocksToCacheup'
            while (map_blocks.Count < minBlocksToCacheup) {
                if (!CacheupNextBlock()) {
                    // If there is no next block, just return
                    return;
                }
            }

            // Step3: Try to add the right block that is going to in sight.
            while (map_blocks.Count > 0) {
                var block = map_blocks[map_blocks.Count - 1];
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
                if (!is_infinity || !ExtendMap()) {
                    return false;
                }
            }
            map_blocks.Add(new MapBlock(mapData.blocks[next_block_index_++], terrainFolder, map_factory_));
            return true;
        }

        private void ClearFolder(Transform folder) {
            for (int i = 0; i < folder.childCount; i++) {
                var child = folder.GetChild(i);
                GameObject.Destroy(child);
            }
        }

        private bool ExtendMap() {
            var new_blocks = map_generator_.PullNextBatch();

            if (new_blocks.Count == 0) {
                Debug.LogError("[MapManager] can not extend map, unexpected stop.");
                return false;
            }

            mapData.blocks.AddRange(new_blocks);
            UpdateWalls();
            return true;
        }

        private void UpdateWalls() {
            MapUtils.SetBoxCollider(
                left_wall_,
                new MapRect(-MapConstants.kWallThickness + mapData.leftBorder, -MapConstants.kWallHalfHeight, MapConstants.kWallThickness, MapConstants.kWallHalfHeight << 1));
            MapUtils.SetBoxCollider(
                right_wall_,
                new MapRect(mapData.rightBorder, -MapConstants.kWallHalfHeight, MapConstants.kWallThickness, MapConstants.kWallHalfHeight << 1));
        }

        void OnDestroy() {
            // Waits for 0.5s to stop
            map_generator_.Stop(System.TimeSpan.FromSeconds(0.5));
        }
    }
}