using System.Collections.Generic;
using System.Threading;
using System;

namespace GirlDash.Map {
    /// <summary>
    /// Not implemented yet.
    /// </summary>
    public abstract class MapGeneratorAsync {
        protected IMapBuilder builder_;

        // the lock object to lock the 'cached_blocks' and 'is_finished_'.
        private object sync_obj_ = new object();
        private Thread generate_thread_;
        // the event shows that we need to generate the next batch of blocks.
        private AutoResetEvent need_more_event_ = new AutoResetEvent(false);
        private AutoResetEvent has_event_ = new AutoResetEvent(false);

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

        public abstract MapData InitMapData();

        /// <summary>
        /// Pull out next batch of blocks, when pulling 
        /// </summary>
        /// <returns></returns>
        public List<BlockData> PullNextBatch() {
            List<BlockData> ret_blocks;
            lock (sync_obj_) {
                // Release owership
                ret_blocks = cached_blocks_;
                cached_blocks_ = new List<BlockData>();
            }

            // Informs we need to calculate the next batch of blocks.
            need_more_event_.Set();

            return ret_blocks;
        }

        /// <summary>
        /// Start threading and inits the first batch of blocks.
        /// </summary>
        public void Start() {
            lock (sync_obj_) {
                if (!is_finished_) {
                    cached_blocks_.Clear();
                }
                is_finished_ = false;
            }

            generate_thread_.Start();
            need_more_event_.Set();
        }

        public void Stop(TimeSpan timeout) {
            lock (sync_obj_) {
                is_finished_ = true;
            }
            if (!generate_thread_.Join(timeout)) {
                generate_thread_.Interrupt();
            }
            need_more_event_.Reset();
        }

        private void ThreadWorker() {
            while (true) {
                lock (sync_obj_) {
                    if (is_finished_) {
                        return;
                    }
                }

                // Waits until informed.
                need_more_event_.WaitOne();

                AppendBatch(GenerateNextBatch());
            }
        }
    }
}
