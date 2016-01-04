using UnityEngine;

namespace GirlDash.Map {
    public abstract class TerrianComponent : MonoBehaviour {
        public const string kGraphicsTransformName = "graphics";

        public TerrianStyle style;
        public Transform graphics;

        public Rect bound { get; private set; }
        public TerrainData data {
            get; private set;
        }
        public new Collider2D collider {
            get; private set;
        }
        private bool is_dirty = false;

        public void BuildSelf(TerrainData data) {
            if (is_dirty) {
                Cleanup();
            }
            is_dirty = true;

            this.data = data;

            Init();
            collider = BuildCollider(data);
            BuildGraphics(data, style);

            bound = new Rect(
                collider.bounds.center.x - collider.bounds.size.x * 0.5f,
                collider.bounds.center.y - collider.bounds.size.y * 0.5f,
                collider.bounds.size.x, collider.bounds.size.y);
        }

        public void RecycleSelf() {
            Cleanup();
            PoolManager.Deallocate(this);
        }

        protected virtual void Init() {
            Rect rect = (Rect)data.region;
            transform.localPosition = rect.center;
        }
        protected virtual void Cleanup() { is_dirty = false; }

        /// <summary>
        /// Build the collider of this component.
        /// Notice the return value will override the collider field, so that this function can create a new collider to replace old one.
        /// </summary>
        protected abstract Collider2D BuildCollider(TerrainData data);
        protected abstract void BuildGraphics(TerrainData data, TerrianStyle style);

        protected virtual void Awake() { }
    }
}