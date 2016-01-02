using UnityEngine;
using System.Collections.Generic;

namespace GirlDash {
    public class PoolManager : SingletonObject<PoolManager> {
        public List<ObjectPool.Options> poolOptions = new List<ObjectPool.Options>();
        public bool showDebugLog = true;
        public bool dontDestroyOnLoad = false;
        public bool autoAddMissingPrefabPool = true;

        private Dictionary<string, ObjectPool> pools_ = new Dictionary<string, ObjectPool>();
        private Dictionary<GameObject, ObjectPool> instance_to_pool_map = new Dictionary<GameObject, ObjectPool>();

        public ObjectPool NewPool(ObjectPool.Options options) {
            options.Preprocess();
            if (pools_.ContainsKey(options.name)) {
                Debug.LogError(string.Format("There is already a ObjectPool of {0}, you must use unique name.", options.name));
                return null;
            } else if (options.prefab == null) {
                Debug.LogError("Prefab of options must be set.");
                return null;
            }

            var folder = new GameObject(options.name);
            folder.transform.parent = transform;
            folder.transform.position = Vector3.zero;

            var object_pool = new ObjectPool(options, folder.transform);
            pools_[options.name] = object_pool;

            return object_pool;
        }
        public ObjectPool NewPool(GameObject prefab) {
            ObjectPool.Options options = new ObjectPool.Options();
            options.prefab = prefab;
            return NewPool(options);
        }

        private ReuseableObject AllocateInternal(string name, Vector3 position, Quaternion rotation) {
            if (showDebugLog) {
                Debug.Log("Allocate object " + name);
            }

            ObjectPool pool;
            if (!pools_.TryGetValue(name, out pool)) {
                Debug.LogError(string.Format("There is no object named {0} in PoolManager", name));
                return null;
            }

            return pool.Allocate(position, rotation);
        }
        private ReuseableObject AllocateInternal(GameObject prefab, Vector3 position, Quaternion rotation) {
            ObjectPool pool;
            if (!pools_.TryGetValue(prefab.name, out pool)) {
                if (autoAddMissingPrefabPool) {
                    NewPool(prefab);
                } else {
                    Debug.LogError("There is no object pool for " + prefab.name);
                    return null;
                }
            }

            return AllocateInternal(prefab.name, position, rotation);
        }

        private bool DeallocateInternal(ReuseableObject obj) {
            if (showDebugLog) {
                Debug.Log("Deallocate object " + obj.name);
            }

            ObjectPool pool;
            if (instance_to_pool_map.TryGetValue(obj.gameObject, out pool)) {
                return pool.Deallocate(obj);
            } else {
                return false;
            }
        }
        private bool DeallocateInternal(GameObject obj) {
            var reusable = obj.GetComponent<ReuseableObject>();
            if (reusable != null) {
                return DeallocateInternal(reusable);
            } else {
                return false;
            }
        }

        public ObjectPool GetPool(string name) {
            ObjectPool pool;
            if (pools_.TryGetValue(name, out pool)) {
                return pool;
            } else {
                return null;
            }
        }

        public ReuseableObject CreateNew(GameObject prefab, ObjectPool pool) {
            GameObject new_obj = GameObject.Instantiate(prefab) as GameObject;
            instance_to_pool_map[new_obj] = pool;

            ReuseableObject reusable = new_obj.GetComponent<ReuseableObject>();
            if (reusable == null) {
                // If there is on ReusableObject component, create default one.
                reusable = new_obj.AddComponent<DefaultReusableObject>();
            }
            reusable.transform.parent = pool.parentTransform;
            reusable.gameObject.SetActive(false);
            return reusable;
        }

        void Awake() {
            DontDestroyOnLoad(gameObject);

            for (int i = 0; i < poolOptions.Count; i++) {
                NewPool(poolOptions[i]);
            }
        }

        void Start() {
            Init();
            float start_time = Time.realtimeSinceStartup;
            for (int i = 0; i < 1000; i++) {
                var obj = Allocate("RifleBullet");
                Deallocate(obj);
            }
            Debug.Log("Time used " + (Time.realtimeSinceStartup - start_time));
        }

        #region Static interfaces
        public static void Init() {
            var unused_instance = Instance;
            // no-op, just to create instance and call Awake()
        }
        public static ReuseableObject Allocate(string name, Vector3 position, Quaternion rotation) {
            return Instance.AllocateInternal(name, position, rotation);
        }
        public static ReuseableObject Allocate(string name) {
            return Instance.AllocateInternal(name, Vector3.zero, Quaternion.identity);
        }
        public static ReuseableObject Allocate(GameObject prefab, Vector3 position, Quaternion rotation) {
            return Instance.AllocateInternal(prefab, position, rotation);
        }
        public static ReuseableObject Allocate(GameObject prefab) {
            return Instance.AllocateInternal(prefab, Vector3.zero, Quaternion.identity);
        }
        public static bool Deallocate(ReuseableObject obj) {
            return Instance.DeallocateInternal(obj);
        }
        public static bool Deallocate(GameObject obj) {
            return Instance.DeallocateInternal(obj);
        }
        #endregion
    }
}
