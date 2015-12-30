using UnityEngine;
using System.Collections.Generic;

namespace GirlDash {
    public class ReuseableObject : MonoBehaviour {
        public bool isUsing {
            get; private set;
        }
        public ObjectPool rootPool {
            get; private set;
        }

        // It's used to deallocate from rootPool, to avoid traversing all nodes.
        private LinkedListNode<ReuseableObject> deallocate_token_;

        public virtual void Init(ObjectPool root_pool, LinkedListNode<ReuseableObject> deallocate_token) {
            rootPool = root_pool;
            deallocate_token_ = deallocate_token;
        }

        public virtual void Active() {
            if (!isUsing) {
                isUsing = true;
                gameObject.SetActive(true);
            }
        }
        public virtual void Deactive() {
            if (isUsing) {
                isUsing = false;
                gameObject.SetActive(false);
                rootPool.Deallocate(deallocate_token_);
            }
        }
    }
}
