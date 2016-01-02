using UnityEngine;
using System.Collections;
using System;

namespace GirlDash {
    public class DefaultReusableObject : ReuseableObject {
        public bool broadcastMessage = false;

        public override void OnAllocate() {
            if (broadcastMessage) {
                BroadcastMessage("Allocate", SendMessageOptions.DontRequireReceiver);
            }
            else {
                SendMessage("Allocate", SendMessageOptions.DontRequireReceiver);
            }
        }

        public override void OnDeallocate() {
            if (broadcastMessage) {
                BroadcastMessage("Allocate", SendMessageOptions.DontRequireReceiver);
            }
            else {
                SendMessage("Deallocate", SendMessageOptions.DontRequireReceiver);
            }
        }
    }
}