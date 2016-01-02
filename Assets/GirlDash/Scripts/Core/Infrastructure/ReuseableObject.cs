using UnityEngine;
using System.Collections.Generic;

namespace GirlDash {
    public class ReuseableObject : MonoBehaviour {
        public virtual void OnAllocate() { }
        public virtual void OnDeallocate() { }
    }
}
