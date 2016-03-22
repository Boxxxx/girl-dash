using GirlDash.Map;
using System;
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

        public StateEnum state = StateEnum.kIdle;

        private float progress_;
        // The startingLine is the left border of whole map, so that our play doesn't start from the zero progress.
        // We should record the initial progress to make the progress for player is zero.
        private float init_progress_ = 0;
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
        // Real progress, starts from to startingLine as zero.
        public float realProgress {
            get { return progress_; }
        }
        // Progress to be recorded, starts from 'init_progress_'.
        public float progressToRecord {
            get { return Mathf.Max(0, progress_ - init_progress_); }
        }

        private bool is_paused_ = false;
        private EnemyQueue enemy_queue_ = new EnemyQueue();

        private IEnumerator ResetInternal(Action finishCB) {
            // Inits the map data.
            yield return StartCoroutine(mapManager.Load(new SimpleMapGenerator(mapManager)));

            yield return StartCoroutine(playerController.Load(new CharacterData()));

            playerController.transform.position = playerSpawnPoint.transform.position;

            progress_ = startingLine.GetOffset(playerController.transform).x;
            init_progress_ = progress_;
            RuntimeData.currentProgress = 0;

            for (int i = 0; i < components_.Count; i++) {
                components_[i].GameReady();
            }

            state = StateEnum.kReady;
            SetPause(true);

            if (finishCB != null) {
                finishCB();
            }
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

        public IEnumerator ResetGameAsync(Action finishCB) {
            // If it's now loading in another coroutine, wait until it's done.
            while (state == StateEnum.kLoading) {
                yield return null;
            }
            // If it's already ready, just exit.
            if (state == StateEnum.kReady) {
                if (finishCB != null) {
                    finishCB();
                }
                yield break;
            }
            state = StateEnum.kLoading;
            RuntimeData.maxProgress = Mathf.Max(RuntimeData.maxProgress, progressToRecord);
            for (int i = 0; i < components_.Count; i++) {
                components_[i].GameReset();
            }

            StartCoroutine(ResetInternal(finishCB));
        }

        public void StartGame() {
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
                RuntimeData.maxProgress = Mathf.Max(RuntimeData.maxProgress, progressToRecord);
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

            yield return ResetGameAsync(null);
        }

        void FixedUpdate() {
            if (is_paused_) {
                return;
            }
            if (isPlaying) {
                progress_ = startingLine.GetOffset(playerController.transform).x;

                if (!debugMode) {
                    mapManager.UpdateProgress(progress_);
                    playerController.Move(1);
                }
            }

            enemy_queue_.Update(progress_);
        }

        void Update() {
            if (is_paused_) {
                return;
            }
            if (isPlaying) {
                RuntimeData.currentProgress = progressToRecord;
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
                playerController.hp, progressToRecord, playerController.jumpCooldown, playerController.fireCooldown));
        }
        #endregion
    }
}