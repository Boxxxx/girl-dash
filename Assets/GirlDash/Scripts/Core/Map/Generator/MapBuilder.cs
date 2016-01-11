using UnityEngine;
using System.Collections.Generic;

namespace GirlDash.Map {
    /// <summary>
    /// Simple map builder, the basic idea is that all terrian components are divided into two layers:
    /// - the first layer is grounds, thay are all the same height and do not overlap with each other.
    /// - the second layer is other widget componets which is placed related to grounds.
    /// - Of course, there is also enemy units.
    /// </summary>
    public class SimpleMapBuilder {
        public class BlockSplitter {
            private List<TerrainData> grounds_;  // not owned
            private List<EnemyData> enemies_;  // not owned
            private List<TerrainData> widgets_;  // not owned

            private void Reset(List<TerrainData> grounds, List<EnemyData> enemies, List<TerrainData> widgets) {
                grounds_ = grounds;
                enemies_ = enemies;
                widgets_ = widgets;
            }

            /// <summary>
            /// Creates a block using the enemies and widgets within the grounds,
            /// the block should include all the 'grounds', and extract the enemies and widgets within this block.
            /// because 'enemies' and 'widgets' are sorted and has index to last block,
            /// so what we need is to forward the index until the x axis is greater than block border.
            /// 
            /// Notice the 'grounds' will be cleared after this function.
            /// </summary>
            private static BlockData NextBlock(List<TerrainData> grounds,
                                               List<EnemyData> sorted_enemies, ref int enemy_index,
                                               List<TerrainData> sorted_widgets, ref int widget_index) {
                if (grounds.Count == 0) {
                    Debug.LogError("BlockData must have at least one terrain.");
                    return null;
                }

                BlockData block_data = new BlockData();
                block_data.enemies = sorted_enemies.ToArray();

                int x_min = grounds[0].region.xMin;
                int x_max = grounds[0].region.xMax;

                for (int i = 1; i < grounds.Count; i++) {
                    x_min = Mathf.Min(x_min, grounds[i].region.xMin);
                    x_max = Mathf.Max(x_max, grounds[i].region.xMax);
                }
                block_data.bound = new MapVector(x_min, x_max);

                List<TerrainData> terrians_in_block = new List<TerrainData>(grounds);
                while (widget_index < sorted_widgets.Count && sorted_widgets[widget_index].center.x < block_data.bound.max) {
                    terrians_in_block.Add(sorted_widgets[widget_index++]);
                }
                List<EnemyData> enemies_in_block = new List<EnemyData>();
                while (enemy_index < sorted_enemies.Count && sorted_enemies[enemy_index].spawnPosition.x < block_data.bound.max) {
                    enemies_in_block.Add(sorted_enemies[enemy_index++]);
                }

                block_data.terrains = terrians_in_block.ToArray();
                block_data.enemies = enemies_in_block.ToArray();

                grounds.Clear();

                return block_data;
            }

            public List<BlockData> Split(int expected_block_width, List<TerrainData> grounds, List<EnemyData> enemies, List<TerrainData> terrains) {
                Reset(grounds, enemies, terrains);

                List<BlockData> blocks = new List<BlockData>();

                if (grounds_.Count == 0) {
                    // no grounds, no blocks
                    return blocks;
                }

                enemies_.Sort((lhs, rhs) => {
                    return lhs.spawnPosition.x - rhs.spawnPosition.y;
                });

                // although the widgets share the structure with grounds,
                // and has an region, but they actually are at a point.
                widgets_.Sort((lhs, rhs) => {
                    return lhs.center.x - rhs.center.x;
                });

                // The grounds do not intersect with each other, and they are in left-to-right order.

                int enemy_index = 0;
                int widget_index = 0;

                TerrainData first_ground_in_current_block = null;
                List<TerrainData> grounds_in_current_block = new List<TerrainData>();
                int ground_index = 0;
                while (ground_index < grounds_.Count) {
                    TerrainData current_ground = grounds_[ground_index];
                    if (first_ground_in_current_block == null) {
                        grounds_in_current_block.Add(current_ground);
                        // If only one ground has already exceeded the expected_block_width, put it as one block.
                        if (current_ground.region.width >= expected_block_width) {
                            blocks.Add(NextBlock(grounds_in_current_block, enemies_, ref enemy_index, widgets_, ref widget_index));
                            first_ground_in_current_block = null;
                        }
                        else {
                            first_ground_in_current_block = current_ground;
                        }
                        ground_index++;
                    }
                    else {
                        if (current_ground.region.xMax - first_ground_in_current_block.region.xMin > expected_block_width) {
                            blocks.Add(NextBlock(grounds_in_current_block, enemies_, ref enemy_index, widgets_, ref widget_index));
                            first_ground_in_current_block = null;
                            // Do not forward the 'ground_index', since we have not add current_ground into blocks yet.
                        }
                        else {
                            grounds_in_current_block.Add(current_ground);
                            ground_index++;
                        }
                    }
                }

                if (first_ground_in_current_block != null) {
                    blocks.Add(NextBlock(grounds_in_current_block, enemies_, ref enemy_index, widgets_, ref widget_index));
                }

                // There are at least one ground, so at least one block
                BlockData last_block = blocks[blocks.Count - 1];

                // Appends the remaining enemies into 'last_block'.
                if (enemy_index < enemies_.Count) {
                    List<EnemyData> last_enemies = new List<EnemyData>(last_block.enemies);
                    while (enemy_index < enemies_.Count) {
                        last_enemies.Add(enemies_[enemy_index++]);
                    }
                    last_block.enemies = last_enemies.ToArray();
                }

                // Appends the remaining widgets into 'last_block'.
                if (widget_index < widgets_.Count) {
                    List<TerrainData> last_terrains = new List<TerrainData>(last_block.terrains);
                    while (widget_index < widgets_.Count) {
                        last_terrains.Add(widgets_[widget_index++]);
                    }
                    last_block.terrains = last_terrains.ToArray();
                }

                return blocks;
            }
        }

        public class Options {
            public int expectedBlockWidth = 12;
            public int groundHeight = 1;
            public int deadHeight = -1;
        }

        private Options options_;
        private List<TerrainData> grounds_ = new List<TerrainData>();
        private List<EnemyData> enemies_ = new List<EnemyData>();
        private List<TerrainData> widgets_ = new List<TerrainData>();
        private BlockSplitter block_splitter_ = new BlockSplitter();

        /// <summary>
        /// Ground is the basic component of a map,
        /// all other terrian componetns or enemies or widgets should be placed related to current block.
        /// If there is not a ground yet, just let the basic offset to (0, 0);
        /// </summary>
        private TerrainData current_ground_ = null;

        /// <summary>
        /// Current ground offset, all widgets components should be placed related to the topleft corner of ground.
        /// If there is not  a ground at all, just return (0, 0).
        /// </summary>
        private MapVector CurrentGroundOffset {
            get {
                if (current_ground_ == null) {
                    return MapVector.zero;
                } else {
                    return current_ground_.region.topLeft;
                }
            }
        }

        public SimpleMapBuilder(Options options) {
            options_ = options;
        }

        public void Reset() {
            grounds_.Clear();
            enemies_.Clear();
            widgets_.Clear();
            current_ground_ = null;
        }

        public TerrainData NewGround(int offset, int width) {
            TerrainData terrian_data = new TerrainData();
            terrian_data.terrainType = TerrainData.TerrainType.Ground;
            terrian_data.interactiveType = TerrainData.InteractiveType.Solid;
            if (current_ground_ == null) {
                terrian_data.region = new MapRect(offset, -options_.groundHeight, width, options_.groundHeight);
            } else {
                terrian_data.region = new MapRect(current_ground_.region.xMax + offset, -options_.groundHeight, width, options_.groundHeight);
            }
            grounds_.Add(terrian_data);

            current_ground_ = terrian_data;
            return current_ground_;
        }

        public TerrainData AddObstacle(int width, int height, int offset_x, int offset_y) {
            TerrainData terrian_data = new TerrainData();
            terrian_data.terrainType = TerrainData.TerrainType.Ground;
            terrian_data.interactiveType = TerrainData.InteractiveType.Solid;

            MapVector current_offset = CurrentGroundOffset;
            terrian_data.region = new MapRect(
                current_offset.x + offset_x, current_offset.y + offset_y,
                width, height);
            widgets_.Add(terrian_data);

            return terrian_data;
        }

        public MapData InitMapData() {
            MapData map_data = new MapData();
            map_data.deadHeight = Mathf.Min(-1, options_.deadHeight);
            map_data.sightRange = new MapVector(MapValue.LowerBound(-11.36f), MapValue.UpperBound(11.36f));
            return map_data;
        }

        public List<BlockData> BuildBlocks() {
            var blocks = block_splitter_.Split(options_.expectedBlockWidth, grounds_, enemies_, widgets_);
            int width = blocks.Count > 0 ? blocks[blocks.Count - 1].bound.max - blocks[0].bound.min : 0;
            Debug.Log(string.Format("[MapBuilder] Blocks data generated, total blocks: {0}, total width: {1}", blocks.Count, width));

            return blocks;
        }
    }
}
