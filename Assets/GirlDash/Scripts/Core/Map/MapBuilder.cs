using UnityEngine;
using System.Collections.Generic;

namespace GirlDash.Map {
    public interface IMapBuilder {
        MapData Build();
    }

    public class SimpleMapBuilder : IMapBuilder {
        public class Options {
            public int expectedBlockWidth = 12;
            public int groundHeight = 1;
            public int deadHeight = -1;
        }

        private Options options_;
        private List<TerrainData> grounds_ = new List<TerrainData>();
        private List<EnemyData> enemies_ = new List<EnemyData>();
        private List<TerrainData> widgets_ = new List<TerrainData>();

        /// <summary>
        /// Ground is the basic component of a map,
        /// all other terrian componetns or enemies or widgets should be placed related to current block.
        /// If there is not a ground yet, just let the basic offset to (0, 0);
        /// </summary>
        private TerrainData current_ground_ = null;

        private MapVector CurrentOffset {
            get {
                if (current_ground_ == null) {
                    return new MapVector(0, 0);
                } else {
                    return current_ground_.region.topRight;
                }
            }
        }

        public SimpleMapBuilder(Options options) {
            options_ = options;
        }

        public SimpleMapBuilder NewGround(int offset, int width) {
            TerrainData terrian_data = new TerrainData();
            terrian_data.terrainType = TerrainData.TerrainType.Ground;
            terrian_data.interactiveType = TerrainData.InteractiveType.Solid;
            if (current_ground_ == null) {
                terrian_data.region = new MapRect(offset, 0, width, options_.groundHeight);
            } else {
                terrian_data.region = new MapRect(current_ground_.region.xMax + offset, 0, width, options_.groundHeight);
            }
            grounds_.Add(terrian_data);

            current_ground_ = terrian_data;
            return this;
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
                var terrain = grounds[i];
                x_min = Mathf.Min(x_min, grounds[i].region.xMin);
                x_max = Mathf.Min(x_max, grounds[i].region.xMax);
            }
            block_data.bound = new MapVector(x_min, x_max);

            List<TerrainData> terrians_in_block = new List<TerrainData>(grounds);
            while (widget_index < sorted_widgets.Count && sorted_widgets[widget_index].center.x < block_data.bound.max) {
                terrians_in_block.AddRange(sorted_widgets);
            }
            List<EnemyData> enemies_in_block = new List<EnemyData>();
            while (enemy_index < sorted_enemies.Count && sorted_enemies[enemy_index].spawnPosition.x < block_data.bound.max) {
                enemies_in_block.Add(sorted_enemies[enemy_index]);
            }

            block_data.terrains = terrians_in_block.ToArray();
            block_data.enemies = enemies_in_block.ToArray();

            grounds.Clear();

            return block_data;
        }

        private void SplitIntoBlocks(int expected_block_width, MapData map_data) {
            map_data.blocks.Clear();
            if (grounds_.Count == 0) {
                // no grounds, no blocks
                return;
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
                        map_data.blocks.Add(NextBlock(grounds_in_current_block, enemies_, ref enemy_index, widgets_, ref widget_index));
                        first_ground_in_current_block = null;
                    } else {
                        first_ground_in_current_block = current_ground;
                    }
                    ground_index++;
                } else {
                    if (current_ground.region.xMax - first_ground_in_current_block.region.xMin > expected_block_width) {
                        map_data.blocks.Add(NextBlock(grounds_in_current_block, enemies_, ref enemy_index, widgets_, ref widget_index));
                        first_ground_in_current_block = null;
                        // Do not forward the 'ground_index', since we have not add current_ground into blocks yet.
                    } else {
                        grounds_in_current_block.Add(current_ground);
                        ground_index++;
                    }
                }
            }

            if (first_ground_in_current_block != null) {
                map_data.blocks.Add(NextBlock(grounds_in_current_block, enemies_, ref enemy_index, widgets_, ref widget_index));
            }

            // There are at least one ground, so at least one block
            BlockData last_block = map_data.blocks[map_data.blocks.Count - 1];

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
        }

        public MapData Build() {
            MapData map_data = new MapData();
            map_data.deadHeight = Mathf.Min(-1, options_.deadHeight);

            SplitIntoBlocks(options_.expectedBlockWidth, map_data);

            // TODO(hyf042): add split here.
            return map_data;
        }
    }
}
