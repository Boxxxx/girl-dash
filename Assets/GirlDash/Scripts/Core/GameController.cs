﻿using GirlDash.Map;
using System;
using UnityEngine;

namespace GirlDash {
    public class GameController : MonoBehaviour {
        public enum StateEnum {
            kNew,
            kPlaying,
            kDead
        }

        public StartingLine startingLine;
        // Only for debug, a line to show dead progress.
        // TODO(hyf042): replace it with more fancy things.
        public Transform deadLine;

        public CharacterController playerController;
        public Transform playerSpawnPoint;

        public CameraController cameraController;
        public MapManager mapManager;

        public bool move = true;
        public bool showDeadline = true;
        public float maximumDeadDistance = 5.68f;
        public GUIStyle deadPromptStyle;

        public float progress;
        public StateEnum state = StateEnum.kNew;

        public float deadProgress {
            get; private set;
        }
        public bool isPlaying {
            get { return state == StateEnum.kPlaying; }
        }

        private void Restart() {
            playerController.transform.position = playerSpawnPoint.transform.position;

            progress = startingLine.GetOffset(playerController.transform).x;
            deadProgress = progress - maximumDeadDistance;

            state = StateEnum.kPlaying;
        }

        private void TouchOfDeath() {
            // Stops the player.
            playerController.Move(0);
            playerController.Die();

            state = StateEnum.kDead;
        }

        void Start() {
            Restart();
        }

        void FixedUpdate() {

            if (isPlaying) {
                progress = startingLine.GetOffset(playerController.transform).x;

                // Updates dead progress
                // TODO(hyf042): find a better dead speed, now it's set to the character's speed.
                deadProgress += Time.deltaTime * playerController.moveSpeed * 0.8f;
                deadProgress = Mathf.Max(deadProgress, progress - maximumDeadDistance);

                mapManager.UpdateProgress(progress);

                if (move) {
                    // Force to move right.
                    playerController.Move(1);
                }
                else {
                    playerController.Move(0);
                }

                if (deadProgress >= progress) {
                    TouchOfDeath();
                }
            }

            // Only for debugging, draw a dead line.
            var tmp_position = deadLine.transform.localPosition;
            tmp_position.x = deadProgress;
            deadLine.transform.localPosition = tmp_position;
        }

        /// <summary>
        /// All contents here are for debugging.
        /// </summary>
        void OnGUI() {
            GUI.color = Color.red;
            GUI.Label(new Rect(0, 0, 100, 50), state.ToString());
        }
    }
}