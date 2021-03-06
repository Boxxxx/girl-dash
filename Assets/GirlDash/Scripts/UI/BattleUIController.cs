﻿using UnityEngine;
using System.Collections.Generic;

namespace GirlDash.UI {
    public class BattleUIController : SingletonObject<BattleUIController>, IGameComponent {
        public new Camera camera;
        public PlayerController controller;
        public Transform recycleFolder;

        #region UI
        public UIHpBar hpBar;
        public UIGameProgress gameProgress;
        public UIPanel pausePanel;
        public UIStartPanel startPanel;
        #endregion

        private Dictionary<Enemy, UIHpBar> enemy_hpbar_map_ = new Dictionary<Enemy, UIHpBar>();
        private Bounds cached_bounds_;
        private bool is_firing_ = false;

        #region Public Interfaces
        public void GameReset() {
            // Loop from the end to the front,
            // the childCount will decrease since we remove it from recycleFolder.
            for (int i = recycleFolder.childCount - 1; i >= 0 ; i--) {
                PoolManager.Deallocate(recycleFolder.GetChild(i));
            }
            enemy_hpbar_map_.Clear();
            gameProgress.ResetAll();
        }
        public void GameReady() { }
        public void GameStart() { }
        public void GameOver() {
            gameProgress.ResetAll();
            startPanel.ShowGameOver();
        }

        public Bounds GetCachedCameraBounds(bool force_to_refresh) {
            if (force_to_refresh) {
                cached_bounds_ = ScreenUtil.CalculateOrthographicBounds(camera);
            }
            return cached_bounds_;
        }
        #endregion

        #region UI Handlers
        public void PressFire() {
            is_firing_ = true;
            controller.Fire();
        }
        public void ReleaseFire() {
            is_firing_ = false;
        }

        public void Move(float axis) {
            controller.Move(axis);
        }

        public void Jump() {
            controller.Jump();
        }
        public void Pause() {
            GameController.Instance.isPaused = true;
            pausePanel.gameObject.SetActive(true);
        }
        public void Resume() {
            GameController.Instance.isPaused = false;
            pausePanel.gameObject.SetActive(false);
        }
        public void GetOff() {
            GameController.Instance.StartGame();
        }
        public void BackToTitle() {
            StartCoroutine(GameController.Instance.ResetGameAsync(null));
            pausePanel.gameObject.SetActive(false);
            startPanel.ShowStart();
        }
        #endregion

        #region Event Handlers
        public void OnEnemyBorn(object[] args) {
            var enemy = args[0] as Enemy;
            UIHpBar new_hp_bar = PoolManager.Allocate(hpBar);
            new_hp_bar.transform.parent = recycleFolder;
            new_hp_bar.Reset(enemy);
            enemy_hpbar_map_.Add(enemy, new_hp_bar);
        }

        public void OnEnemyDestroy(object[] args) {
            var enemy = args[0] as Enemy;
            UIHpBar hp_bar;
            if (enemy_hpbar_map_.TryGetValue(enemy, out hp_bar)) {
                enemy_hpbar_map_.Remove(enemy);
                PoolManager.Deallocate(hp_bar);
            }
        }
        #endregion

        void Awake() {
            GameController.Instance.Register(this);
            EventPool.Instance.On(Events.OnEnemyBorn, OnEnemyBorn);
            EventPool.Instance.On(Events.OnEnemyDestroy, OnEnemyDestroy);
        }

        void Update() {
            controller.HoldFire(is_firing_);
        }

        void Start() {
            startPanel.ShowStart();
        }
    }
}