using UnityEngine;
using System.Collections.Generic;

namespace GirlDash {
    using Node = LinkedListNode<ReuseableObject>;
    public class ObjectPool : MonoBehaviour {
        public int poolSize = 0;
        public ReuseableObject prefab = null;
        public bool autoLoad = true;
        public bool createNewIfNotEnough = false;

        private LinkedList<ReuseableObject> unused_ = new LinkedList<ReuseableObject>();
        private LinkedList<ReuseableObject> inused_ = new LinkedList<ReuseableObject>();

        public int AvaliableCount {
            get { return unused_.Count; }
        }

        public void Prepare() {
            if (unused_.Count > 0) {
                Free();
            }
            for (int i = 0; i < poolSize; i++) {
                var obj = AllocateInternal();
                Node node = unused_.AddLast(obj);
                obj.Init(this, node);
            }
        }
        public void Prepare(int size) {
            poolSize = size;
            Prepare();
        }
        public void Prepare(int size, ReuseableObject prefab) {
            this.prefab = prefab;
            poolSize = size;
            Prepare();
        }

        public void Free() {
            for (Node iter = inused_.First; iter != null; iter = iter.Next) {
                GameObject.Destroy(iter.Value);
            }
            inused_.Clear();
            for (Node iter = unused_.First; iter != null; iter = iter.Next) {
                GameObject.Destroy(iter.Value);
            }
            unused_.Clear();

            poolSize = 0;
        }

        public ReuseableObject Allocate() {
            if (unused_.Count > 0) {
                Node node = unused_.First;
                unused_.Remove(node);
                inused_.AddFirst(node);

                node.Value.Active();
                return node.Value;
            }
            
            if (createNewIfNotEnough) {
                var obj = AllocateInternal();
                var node = inused_.AddLast(obj);
                obj.Init(this, node);

                obj.Active();
                return obj;
            }
            return null;
        }

        // Only used by ReusablePool
        public void Deallocate(Node node) {
            inused_.Remove(node);
            unused_.AddLast(node);
            node.Value.transform.parent = transform;
        }

        public T Allocate<T>() where T : ReuseableObject {
            return Allocate() as T;
        }
        private ReuseableObject AllocateInternal() {
            var obj = GameObject.Instantiate(prefab) as ReuseableObject;
            obj.transform.parent = transform;
            obj.gameObject.SetActive(false);
            return obj;
        }

        void Start() {
            if (autoLoad) {
                Prepare();
            }
        }
    }
}
