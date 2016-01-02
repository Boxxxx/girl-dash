using UnityEngine;
using System.Collections.Generic;

namespace GirlDash {
    public class ObjectPool {
        [System.Serializable]
        public struct Options {
            [Tooltip("GameObject this ObjectPool holds. Must set.")]
            public GameObject prefab;
            [Tooltip("Size of objects to preloaded when ObjectPool is created")]
            public int preloadSize;
            [Tooltip("Max capacity of this ObjectPool. if it's zero, then it's equal to preloadSize.")]
            public int maxCapacity;
            [Tooltip("Allow pool to pick out a using object and recycle it when the maxCapacity is reached")]
            public bool allowPoolToRecycle;

            public string name {
                get; private set;
            }

            public void Preprocess() {
                if (maxCapacity <= 0) {
                    maxCapacity = preloadSize;
                }
                name = prefab.name;
            }
        }

        public Options options {
            get; private set;
        }

        private List<ReuseableObject> unused_objs = new List<ReuseableObject>();
        private HashSet<ReuseableObject> using_objs_ = new HashSet<ReuseableObject>();

        public string name {
            get { return options.prefab.name; }
        }
        public GameObject prefab {
            get { return options.prefab; }
        }
        public int availableUnusedCnt {
            get { return unused_objs.Count; }
        }
        public int loadedCnt {
            get { return unused_objs.Count + using_objs_.Count; }
        }
        public Transform parentTransform {
            get; private set;
        }

        public ObjectPool(Options options, Transform parent_transform) {
            this.options = options;
            parentTransform = parent_transform;

            PreloadInternal();
        }

        public void FreeAll() {
            foreach (var obj in using_objs_) {
                GameObject.Destroy(obj);
            }
            using_objs_.Clear();
            for (int i = 0; i < unused_objs.Count; i++) {
                GameObject.Destroy(unused_objs[i]);
            }
            unused_objs.Clear();
        }

        public ReuseableObject Allocate(Vector3 position, Quaternion rotation) {
            ReuseableObject obj = null;

            if (unused_objs.Count > 0) {
                // Case1: there is still available unused objects
                obj = unused_objs[unused_objs.Count - 1];
                unused_objs.RemoveAt(unused_objs.Count - 1);
            } else if (loadedCnt < options.maxCapacity) {
                // Case2: the pool has not reach its capacity, create new one
                obj = PoolManager.Instance.CreateNew(options.prefab, this);
            } else if (options.allowPoolToRecycle) {
                // Case3: recycle a using objects
                obj = RecycleOne();
            }

            if (obj != null) {
                using_objs_.Add(obj);
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.gameObject.SetActive(true);
                obj.OnAllocate();
            }
            return obj;
        }

        public bool Deallocate(ReuseableObject obj) {
            if (using_objs_.Remove(obj)) {
                unused_objs.Add(obj);
                obj.transform.parent = parentTransform;

                obj.OnDeallocate();
                obj.gameObject.SetActive(false);
                return true;
            }
            return false;
        }

        private void PreloadInternal() {
            if (unused_objs.Count > 0) {
                FreeAll();
            }
            for (int i = 0; i < options.preloadSize; i++) {
                var obj = PoolManager.Instance.CreateNew(options.prefab, this);
                unused_objs.Add(obj);
            }
        }

        /// <summary>
        /// Recycles one object from using pool, if there is not any active objects, return null.
        /// </summary>
        /// <returns></returns>
        private ReuseableObject RecycleOne() {
            if (using_objs_.Count > 0) {
                foreach (var obj in using_objs_) {
                    Deallocate(obj);
                    return obj;
                }
            }
            return null;
        }
    }
}
