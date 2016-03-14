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

        public PlayerController playerController;
        public Transform playerSpawnPoint;

        public CameraController cameraController;
        public MapManager mapManager;

        public bool debugMode = false;

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

        private IEnumerator RestartInternal() {
            // Inits the map data.
            yield return StartCoroutine(mapManager.Load(new SimpleMapGenerator(mapManager)));

            yield return StartCoroutine(playerController.Load(new CharacterData()));

            playerController.transform.position = playerSpawnPoint.transform.position;

            progress = startingLine.GetOffset(playerController.transform).x;

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
        public void Unregister(IGameComponent component) {
            components_.Remove(component);
        }

        public void Restart() {
            state = StateEnum.kNew;
            StartCoroutine(RestartInternal());
        }

        public void OnGameOver() {
            if (isPlaying) {
                state = StateEnum.kDead;
                Debug.Log("on game over");

                for (int i = 0; i < components_.Count; i++) {
                    components_[i].GameOver();
                }
            }
        }
        #endregion

        #region Private & Proected Methods
        private void InitRuntimeConsts() {
            // Layer
            RuntimeConsts.groundLayer = LayerMask.NameToLayer(Consts.kGroundLayer);
            RuntimeConsts.groundLayerMask = 1 << LayerMask.NameToLayer(Consts.kGroundLayer);

            // Screen size
            Bounds screen_size = CameraController.Instance.GetCachedCameraBounds(true /* force to refresh */);
            RuntimeConsts.initScreenSize = screen_size.size;
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
            
            Register(playerController);
        }

        IEnumerator Start() {
            // Preload pooling objects.
            yield return StartCoroutine(PoolManager.Instance.Load());

            yield return StartCoroutine(RestartInternal());
        }

        void FixedUpdate() {
            if (isPlaying) {
                progress = startingLine.GetOffset(playerController.transform).x;

                if (!debugMode) {
                    mapManager.UpdateProgress(progress);
                    playerController.Move(1);
                }
            }

            enemy_queue_.Update(progress);
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

            if (state == StateEnum.kDead) {
                if (GUI.Button(new Rect(0, 0, 100, 50), "Restart")) {
                    Restart();
                }
            }
        }
        #endregion
    }
}