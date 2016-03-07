using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Random = System.Random;

namespace GirlDash.Map {
    public class SimpleMapGenerator : IMapGenerator {
        private Random random_ = new Random();
        private SimpleMapBuilder builder_;
        private MapData map_data;

        public SimpleMapGenerator() {
            var options = new SimpleMapBuilder.Options();
            options.expectedBlockWidth = 15;
            
            builder_ = new SimpleMapBuilder(options);
        }

        public IEnumerator Generate() {
            map_data = builder_.InitMapData();

            builder_.Reset();

            int num_ground = 10000;
            MapVector random_ground_width_range = new MapVector(4, 20);
            MapVector random_ground_offset_range = new MapVector(2, 6);

            builder_.NewGround(
                0, 0, Mathf.Max(12, random_.Next(random_ground_width_range.x, random_ground_width_range.y)));

            // Try to add a test enemy
            builder_.AddEnemy(EnemyData.EnemyType.Dog, 10, 1, 1, 1);

            for (int i = 1; i < num_ground; i++) {
                var ground_data = builder_.NewGround(
                    random_.Next(random_ground_offset_range.x, random_ground_offset_range.y),
                    random_.NextDouble() < 0.25 ? 1 : 0 /* 25% possibility to be a high ground */,
                    random_.Next(random_ground_width_range.x, random_ground_width_range.y / 2 * 2 /* round down */));
                if (random_.NextDouble() < 0.5) {
                    builder_.AddEnemy(EnemyData.EnemyType.Dog, random_.Next(0, Mathf.Max(1, ground_data.region.width - 1)), 1, 1, 1);
                }
            }

            map_data.blocks = builder_.BuildBlocks();
            yield return map_data;
        }

        public MapData GetMap() {
            return map_data;
        }
    }
}