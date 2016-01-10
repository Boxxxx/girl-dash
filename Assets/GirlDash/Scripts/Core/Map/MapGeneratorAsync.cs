using System.Collections.Generic;
using System.Threading;
using System;

namespace GirlDash.Map {
    public abstract class MapGeneratorAsync {
        protected IMapBuilder builder_;

        // the lock object to lock the 'cached_blocks' and 'is_finished_'.
        private object sync_obj_ = new object();
        private Thread generate_thread_;
        // the event shows that we need to generate the next batch of blocks.
        private AutoResetEvent feed_event_;

        private bool is_finished_ = true;
        private List<BlockData> cached_blocks_ = new List<BlockData>();

        public MapGeneratorAsync(IMapBuilder builder) {
            builder_ = builder;
            generate_thread_ = new Thread(ThreadWorker);
        }

        protected abstract List<BlockData> GenerateNextBatch();
        protected void AppendBatch(List<BlockData> new_batch) {
            lock (sync_obj_) {
                cached_blocks_.AddRange(new_batch);
            }
        }

        /// <summary>
        /// Pull out next batch of blocks, when pulling 
        /// </summary>
        /// <returns></returns>
        public BlockData[] PullNextBatch() {
            BlockData[] ret_blocks;
            lock (sync_obj_) {
                ret_blocks = cached_blocks_.ToArray();
                cached_blocks_.Clear();
            }

            // Informs we need to calculate the next batch of blocks.
            feed_event_.Set();

            return ret_blocks;
        }

        /// <summary>
        /// Start threading and inits the first batch of blocks.
        /// </summary>
        public void Init() {
            if (!is_finished_) {
                return;
            }

            is_finished_ = false;

            generate_thread_.Start();
            feed_event_.Set();
        }

        public void Stop(TimeSpan timeout) {
            lock (sync_obj_) {
                is_finished_ = true;
            }
            if (!generate_thread_.Join(timeout)) {
                generate_thread_.Interrupt();
            }
        }

        private void ThreadWorker() {
            while (true) {
                lock (sync_obj_) {
                    if (is_finished_) {
                        return;
                    }
                }

                // Waits until informed.
                feed_event_.WaitOne();

                AppendBatch(GenerateNextBatch());
            }
        }
    }
}
