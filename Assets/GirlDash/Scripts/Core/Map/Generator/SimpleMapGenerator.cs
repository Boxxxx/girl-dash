using UnityEngine;
using System.Collections.Generic;

using Random = System.Random;

namespace GirlDash.Map {
    public class SimpleMapGenerator : MapGeneratorAsync {
        private int offset_ = 0;
        private Random random_ = new Random();

        public SimpleMapGenerator(SimpleMapBuilder builder): base(builder) {}

        public override MapData InitMapData() {
            var map_data = builder_.InitMapData();
            map_data.blocks.AddRange(PullNextBatch());
            return map_data;
        }

        protected override List<BlockData> GenerateNextBatch() {
            var builder = builder_ as SimpleMapBuilder;
            builder.Reset(offset_);

            int num_ground = 100;
            MapVector random_ground_width_range = new MapVector(3, 10);
            MapVector random_ground_offset_range = new MapVector(0, 4);

            builder.NewGround(
                0, random_.Next(Mathf.Max(7, random_ground_width_range.x), random_ground_width_range.y));
            for (int i = 1; i < num_ground; i++) {
                var ground_data = builder.NewGround(
                    random_.Next(random_ground_offset_range.x, random_ground_offset_range.y),
                    random_.Next(random_ground_width_range.x, random_ground_width_range.y));
                if (random_.NextDouble() < 0.25) {
                    // 25% possibility to add a obstacle
                    builder.AddObstacle(ground_data.region.width, random_.Next(1, 2), 0, 0);
                }
            }

            var blocks = builder.BuildBlocks();
            if (blocks.Count > 0) {
                offset_ = blocks[blocks.Count - 1].bound.max;
            }
            return blocks;
        }
    }
}