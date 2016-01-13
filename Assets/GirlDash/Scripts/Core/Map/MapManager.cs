using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GirlDash.Map {
    public class MapBlock {
        public List<TerrainComponent> terrains = new List<TerrainComponent>();
        public List<Enemy> enemies = new List<Enemy>();

        public BlockData blockData {
            get; private set;
        }
        public MapVector bound {
            get { return blockData.bound; }
        }

        public Transform folder {
            get; private set;
        }

        public MapBlock(BlockData block_data, Transform parent_transform, MapFactory factory) {
            blockData = block_data;
            folder = parent_transform;

            for (int i = 0; i < blockData.terrains.Length; i++) {
                var terrain = factory.CreateTerrain(blockData.terrains[i], parent_transform);
                terrains.Add(terrain);
            }

            for (int i = 0; i< blockData.enemies.Length; i++) {
                var enemy = factory.CreateEnemy(blockData.enemies[i], parent_transform);
                enemies.Add(enemy);
            }

        }

        public void RecycleSelf(MapManager manager) {
            for (int i = 0; i < terrains.Count; i++) {
                terrains[i].RecycleSelf();
            }
            terrains.Clear();
            for (int i = 0; i < enemies.Count; i++) {
                enemies[i].RecycleSelf();
            }
            enemies.Clear();
            if (folder != null) {
                manager.RecycleFolder(folder);
            }
        }
    }

    [RequireComponent(typeof(MapFactory))]
    public class MapManager : MonoBehaviour, IGameComponent {
        [Tooltip("Minimum of number of blocks that is cached.")]
        public int minBlocksToCacheup = 3;
        public Transform blockFolder;
        public Transform fixObjFolder;

        /// <summary>
        /// What's really needed here is a dequeue, but we have only a couple of blocks (less than 10) in this group,
        /// so we just use a list to simulate it.
        /// </summary>
        private List<MapBlock> map_blocks = new List<MapBlock>();
        private Queue<Transform> unused_folder_queue_ = new Queue<Transform>();

        public MapData mapData { get; private set; }

        public float progress { get; private set; }
        public float sightRange { get; private set; }

        private BoxCollider2D dead_area_;

        private MapFactory map_factory_;
        private IMapGenerator map_generator_;
        private int next_block_index_;

        private void InitBoundingColliders() {
            MapUtils.CreateBoxCollider(
                "WallLeft", fixObjFolder,
                new MapRect(-MapConstants.kWallThickness + mapData.leftmost, -MapConstants.kWallHalfHeight, MapConstants.kWallThickness, MapConstants.kWallHalfHeight << 1),
                false /* is_trigger */);
            MapUtils.CreateBoxCollider(
                "WallRight", fixObjFolder,
                new MapRect(mapData.rightmost, -MapConstants.kWallHalfHeight, MapConstants.kWallThickness, MapConstants.kWallHalfHeight << 1),
                false /* is_trigger */);
        }
        private void InitDeadArea() {
            dead_area_ = MapUtils.CreateBoxCollider(
                "DeadArea", fixObjFolder,
                new MapRect(0, mapData.deadHeight - MapConstants.kWallThickness, mapData.rightmost, MapConstants.kWallThickness),
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
                RecycleBlock(map_blocks[i]);
            }
            map_blocks.Clear();

            // Initial build
            InitialBuild();
        }

        public void UpdateProgress(float new_progress) {
            if (new_progress > progress) {
                progress = new_progress;
                RefreshBlocks();
            }
        }

        public IEnumerator Load(IMapGenerator map_generator) {
            map_generator_ = map_generator;
            map_factory_ = GetComponent<MapFactory>();

            yield return StartCoroutine(map_generator_.Generate());

            Reset(map_generator_.GetMap());
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
                    block.RecycleSelf(this);
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
                return false;
            }
            map_blocks.Add(
                new MapBlock(mapData.blocks[next_block_index_], NextCleanFolder("Block_" + next_block_index_), map_factory_));
            next_block_index_++;
            return true;
        }

        private void RecycleBlock(MapBlock map_block) {
            map_block.RecycleSelf(this);
        }

        private void ClearFolder(Transform folder) {
            for (int i = 0; i < folder.childCount; i++) {
                var child = folder.GetChild(i);
                GameObject.Destroy(child);
            }
        }

        private Transform NextCleanFolder(string name) {
            Transform folder;
            if (unused_folder_queue_.Count > 0) {
                folder = unused_folder_queue_.Dequeue();
            } else {
                folder = NewFolder();
            }
            folder.name = name;
            folder.gameObject.SetActive(true);
            return folder;
        }

        public void RecycleFolder(Transform folder) {
            folder.gameObject.SetActive(false);
            unused_folder_queue_.Enqueue(folder);
        }

        /// <summary>
        /// Should not be called at runtime.
        /// </summary>
        private Transform NewFolder() {
            var folder = new GameObject("Cached Folder");
            folder.transform.parent = blockFolder;
            folder.transform.localPosition = Vector3.zero;
            folder.gameObject.SetActive(false);
            return folder.transform;
        }

        void Awake() {
            // Caches block folders, we cache twice the folder to avoid dynamically instantiate.
            for (int i = 0; i < minBlocksToCacheup * 2; i++) {
                unused_folder_queue_.Enqueue(NewFolder());
            }
        }
    }
}