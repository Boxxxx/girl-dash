using System.Collections.Generic;

namespace GirlDash.Map {
    public abstract class MapGeneratorAsync {
        protected IMapBuilder builder_;

        private object mutex_;
        private List<BlockData> cached_blocks_ = new List<BlockData>();

        public MapGeneratorAsync(IMapBuilder builder) {
            builder_ = builder;
        }

        /// <summary>
        /// Inform the information of passed blocks of current game,
        /// it's used to let the generator consider when to do calculation and cache more blocks.
        /// </summary>
        public abstract BlockData[] OnBlockPassed(int num_passed_blocks, int num_remain_blocks, int remain_width);

        /// <summary>
        /// Pull out next batch of blocks
        /// </summary>
        /// <returns></returns>
        public abstract BlockData[] PullNextBatch();
    }
}
