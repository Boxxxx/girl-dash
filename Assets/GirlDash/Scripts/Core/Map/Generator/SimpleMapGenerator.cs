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
            MapVector random_ground_offset_range = new MapVector(2, 5);

            builder_.NewGround(
                0, 0, Mathf.Max(12, random_.Next(random_ground_width_range.x, random_ground_width_range.y)));

            // Try to add a test enemy
            builder_.AddEnemy(EnemyData.EnemyType.Dog, 10, 1, 1, 1);

            bool last_is_high_ground = false;
            for (int i = 1; i < num_ground; i++) {
                bool this_is_high_ground = random_.NextDouble() < 0.25 /* 25% possibility to be a high ground */;
                bool close_to_last_ground = last_is_high_ground != this_is_high_ground ? random_.NextDouble() < 0.5 : false /* 50% possibility to be a closed ground */;

                var ground_data = builder_.NewGround(
                    close_to_last_ground ? 0 : random_.Next(random_ground_offset_range.x, random_ground_offset_range.y),
                    this_is_high_ground ? 1 : 0,
                    random_.Next(random_ground_width_range.x, random_ground_width_range.y / 2 * 2 /* round down */));

                if (random_.NextDouble() < 0.5) {
                    int enemy_type_index = random_.Next(0, 6);
                    EnemyData.EnemyType enemy_type = EnemyData.EnemyType.Scout + enemy_type_index;
                    AddEnemy(enemy_type, random_.Next(0, Mathf.Max(1, ground_data.region.width - 1)));
                }

                last_is_high_ground = this_is_high_ground;
            }

            map_data.blocks = builder_.BuildBlocks();
            yield return map_data;
        }

        public void AddEnemy(EnemyData.EnemyType enemy_type, int offset) {
            switch (enemy_type) {
                case EnemyData.EnemyType.Dog:
                    builder_.AddEnemy(enemy_type, offset, 1, 1, 1);
                    break;
                case EnemyData.EnemyType.Pioneer:
                    builder_.AddEnemy(enemy_type, offset, 1, 1, 1);
                    break;
                case EnemyData.EnemyType.Scout:
                    builder_.AddEnemy(enemy_type, offset, 1, 1, 1);
                    break;
                case EnemyData.EnemyType.Shield:
                    builder_.AddEnemy(enemy_type, offset, 1, 1, 1);
                    break;
                case EnemyData.EnemyType.Bomber:
                    builder_.AddEnemy(enemy_type, offset, 1, 5, 1);
                    break;
            }
        }

        public MapData GetMap() {
            return map_data;
        }
    }
}