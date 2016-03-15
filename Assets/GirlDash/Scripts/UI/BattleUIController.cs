using UnityEngine;
using System.Collections.Generic;

namespace GirlDash.UI {
    public class BattleUIController : SingletonObject<BattleUIController>, IGameComponent {
        public new Camera camera;
        public PlayerController controller;
        public UIHpBar hpBar;
        public Transform recycleFolder;

        private Dictionary<Enemy, UIHpBar> enemy_hpbar_map_ = new Dictionary<Enemy, UIHpBar>();
        private Bounds cached_bounds_;
        private bool is_firing_ = false;

        #region Public Interfaces
        public void GameReset() {
            for (int i = 0; i < recycleFolder.childCount; i++) {
                PoolManager.Deallocate(recycleFolder.GetChild(i));
            }
            enemy_hpbar_map_.Clear();
        }
        public void GameStart() { }
        public void GameOver() {}

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
    }
}