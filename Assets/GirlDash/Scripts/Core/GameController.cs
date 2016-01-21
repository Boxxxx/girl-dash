using GirlDash.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GirlDash {
    public class GameController : SingletonObject<GameController> {
        public enum StateEnum {
            kNew,
            kPlaying,
            kDead
        }

        public struct RuntimeInfo {
            public float progress;
            public float deadProgress;
        }

        public StartingLine startingLine;
        // Only for debug, a line to show dead progress.
        // TODO(hyf042): replace it with more fancy things.
        public Transform deadLine;

        public PlayerController playerController;
        public Transform playerSpawnPoint;

        public CameraController cameraController;
        public MapManager mapManager;

        public bool debugMode = false;
        public bool showDeadline = true;
        public float maximumDeadDistance = 5.68f;

        public float progress;
        public StateEnum state = StateEnum.kNew;

        private List<IGameComponent> components_ = new List<IGameComponent>();

        public float deadProgress {
            get; private set;
        }
        public bool isPlaying {
            get { return state == StateEnum.kPlaying; }
        }
        public EnemyQueue enemyQueue {
            get { return enemy_queue_; }
        }

        private EnemyQueue enemy_queue_ = new EnemyQueue();

        private void Restart() {
            playerController.transform.position = playerSpawnPoint.transform.position;

            progress = startingLine.GetOffset(playerController.transform).x;
            deadProgress = progress - maximumDeadDistance;

            for (int i = 0; i < components_.Count; i++) {
                components_[i].GameStart();
            }

            state = StateEnum.kPlaying;
        }

        #region Game Events
        public void OnEnemyBorn(object[] args) {
            enemy_queue_.NewEnemy(args[0] as Enemy);
        }
        public void OnEnemyInView(object[] args) {
        }
        public void OnEnemyAction(object[] args) {
        }
        public void OnEnemyDestroy(object[] args) {
            enemy_queue_.RemoveEnemy(args[0] as Enemy);
        }
        #endregion

        #region Public Interfaces
        public void Register(IGameComponent component) {
            components_.Add(component);
        }

        public float GetDistToPlayer(Transform transform) {
            return Mathf.Abs(transform.position.x - playerController.transform.position.x);
        }

        public bool InFrontOfPlayer(Transform transform) {
            return transform.position.x >= playerController.transform.position.x;
        }

        public RuntimeInfo GetRuntimeInfo() {
            return new RuntimeInfo() {
                progress = progress,
                deadProgress = deadProgress
            };
        }

        public void OnPlayerDie() {
            if (isPlaying) {
                state = StateEnum.kDead;

                for (int i = 0; i < components_.Count; i++) {
                    components_[i].GameOver();
                }
            }
        }
        #endregion

        #region Private & Proected Methods
        private void InitRuntimeConsts() {
            RuntimeConsts.groundLayer = LayerMask.NameToLayer(Consts.kGroundLayer);
            RuntimeConsts.groundLayerMask = 1 << LayerMask.NameToLayer(Consts.kGroundLayer);
        }

        private void RegisterEvents() {
            EventPool.Instance.On(Events.OnEnemyBorn, OnEnemyBorn);
            EventPool.Instance.On(Events.OnEnemyAction, OnEnemyAction);
            EventPool.Instance.On(Events.OnEnemyInView, OnEnemyInView);
            EventPool.Instance.On(Events.OnEnemyDestroy, OnEnemyDestroy);
        }
        #endregion

        #region Unity Callbacks
        void Awake() {
            Application.targetFrameRate = Consts.kFps;

            InitRuntimeConsts();
            RegisterEvents();

            Register(PoolManager.Instance);
            Register(mapManager);
            Register(cameraController);
            Register(playerController);
        }

        IEnumerator Start() {
            // Preload pooling objects.
            yield return StartCoroutine(PoolManager.Instance.Load());

            // Inits the map data.
            yield return StartCoroutine(mapManager.Load(new SimpleMapGenerator()));

            yield return StartCoroutine(playerController.Load(new CharacterData()));

            Restart();
        }

        void FixedUpdate() {
            if (isPlaying) {
                progress = startingLine.GetOffset(playerController.transform).x;

                if (!debugMode) {
                    // Updates dead progress
                    // TODO(hyf042): find a better dead speed, now it's set to the character's speed.
                    deadProgress += Time.deltaTime * playerController.moveSpeed * 0.8f;
                    deadProgress = Mathf.Max(deadProgress, progress - maximumDeadDistance);

                    mapManager.UpdateProgress(progress);

                    playerController.Move(1);
                }

                if (deadProgress >= progress) {
                    playerController.Die();
                }
            }

            // Only for debugging, draw a dead line, run close to deadProgress smoothly.
            var deadline_position = deadLine.transform.localPosition;
            if (deadLine.transform.localPosition.x - deadProgress < Consts.kSoftEps) {
                deadline_position.x = deadProgress;
            } else {
                deadline_position.x = (deadLine.transform.localPosition.x - deadProgress) / 3 + deadProgress;
            }
            deadLine.transform.localPosition = deadline_position;

            enemy_queue_.Update(progress, deadProgress);
        }

        /// <summary>
        /// All contents here are for debugging.
        /// </summary>
        void OnGUI() {
            GUI.color = Color.red;
            GUI.Label(new Rect(0, 0, 100, 50), state.ToString());
            GUI.Label(
                new Rect(0, 50, 200, 50),
                string.Format("HP: {0}\nProgress: {1}\nJump CD: {2}, Fire CD: {3}",
                playerController.hp, progress, playerController.jumpCooldown, playerController.fireCooldown));
        }
        #endregion
    }
}