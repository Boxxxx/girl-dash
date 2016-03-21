using GirlDash.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GirlDash {
    public class GameController : SingletonObject<GameController> {
        public enum StateEnum {
            kLoading,
            kReady,
            kPlaying,
            kIdle
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
        public StateEnum state = StateEnum.kIdle;

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
        public bool isPaused {
            get { return is_paused_; }
            set {
                SetPause(value);
            }
        }

        private bool is_paused_ = false;
        private EnemyQueue enemy_queue_ = new EnemyQueue();

        private IEnumerator ResetInternal() {
            // Inits the map data.
            yield return StartCoroutine(mapManager.Load(new SimpleMapGenerator(mapManager)));

            yield return StartCoroutine(playerController.Load(new CharacterData()));

            playerController.transform.position = playerSpawnPoint.transform.position;

            progress = startingLine.GetOffset(playerController.transform).x;
            RuntimeData.currentProgress = progress;

            for (int i = 0; i < components_.Count; i++) {
                components_[i].GameReady();
            }

            state = StateEnum.kReady;
            SetPause(true);
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

        public void Reset() {
            if (state == StateEnum.kLoading || state == StateEnum.kReady) {
                return;
            }
            state = StateEnum.kLoading;
            RuntimeData.maxProgress = Mathf.Max(RuntimeData.maxProgress, progress);
            for (int i = 0; i < components_.Count; i++) {
                components_[i].GameReset();
            }
            StartCoroutine(ResetInternal());
        }

        public void GetOff() {
            if (state != StateEnum.kReady) {
                return;
            }
            state = StateEnum.kPlaying;
            for (int i = 0; i < components_.Count; i++) {
                components_[i].GameStart();
            }
            SetPause(false);
        }

        public void OnGameOver() {
            if (isPlaying) {
                state = StateEnum.kIdle;
                RuntimeData.maxProgress = Mathf.Max(RuntimeData.maxProgress, progress);
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

        private void SetPause(bool is_pause) {
            if (is_paused_ == is_pause) {
                return;
            }

            is_paused_ = is_pause;
            if (is_pause) {
                Time.timeScale = 0;
            } else {
                Time.timeScale = 1;
            }
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

            Reset();
        }

        void FixedUpdate() {
            if (is_paused_) {
                return;
            }
            if (isPlaying) {
                progress = startingLine.GetOffset(playerController.transform).x;

                if (!debugMode) {
                    mapManager.UpdateProgress(progress);
                    playerController.Move(1);
                }
            }

            enemy_queue_.Update(progress);
        }

        void Update() {
            if (is_paused_) {
                return;
            }
            if (isPlaying) {
                RuntimeData.currentProgress = progress;
            }
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

            if (state == StateEnum.kIdle) {
                if (GUI.Button(new Rect(0, 0, 100, 50), "Restart")) {
                    Reset();
                }
            }
        }
        #endregion
    }
}