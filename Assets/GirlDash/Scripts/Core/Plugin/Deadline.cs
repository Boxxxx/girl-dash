using UnityEngine;

namespace GirlDash {
    public class Deadline : GamePlugin {
        // Only for debug, a line to show dead progress.
        // TODO(hyf042): replace it with more fancy things.
        public Transform deadLine;

        public bool showDeadline = true;
        public float maximumDeadDistance = 5.68f;

        public float deadProgress {
            get; private set;
        }

        public override void GameReady() {
            deadProgress = progress - maximumDeadDistance;
        }

        void FixedUpdate() {
            if (!isPlaying || debugMode) {
                return;
            }

            // Updates dead progress
            // TODO(hyf042): find a better dead speed, now it's set to the character's speed.
            deadProgress += Time.deltaTime * player.moveSpeed * 0.8f;
            deadProgress = Mathf.Max(deadProgress, progress - maximumDeadDistance);

            if (deadProgress >= progress) {
                player.Die();
            }

            // Only for debugging, draw a dead line, run close to deadProgress smoothly.
            var deadline_position = deadLine.transform.localPosition;
            if (deadLine.transform.localPosition.x - deadProgress < Consts.kSoftEps) {
                deadline_position.x = deadProgress;
            }
            else {
                deadline_position.x = (deadLine.transform.localPosition.x - deadProgress) / 3 + deadProgress;
            }
            deadLine.transform.localPosition = deadline_position;
        }
    }
}
