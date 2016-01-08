using UnityEngine;

namespace GirlDash.Map {
    public static class MapResolution {
        public const float kMapToUnityResolutionFactor = 1f;
    }
    /// <summary>
    /// Represents a vector in map resolution.
    /// </summary>
    public struct MapVector {
        public int x { get; private set; }
        public int y { get; private set; }
        public int min { get { return x; } }
        public int max { get { return y; } }
        public int length { get { return y - x; } }

        public MapVector(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public static explicit operator Vector2(MapVector vec) {
            return new Vector2(
                vec.x * MapResolution.kMapToUnityResolutionFactor,
                vec.y * MapResolution.kMapToUnityResolutionFactor);
        }
    }

    /// <summary>
    /// Represents a value, or a length in map resolution.
    /// </summary>
    public struct MapValue {
        public int value { get; private set; }

        public MapValue(int value) {
            this.value = value;
        }

        public static explicit operator float (MapValue value) {
            return value.value * MapResolution.kMapToUnityResolutionFactor;
        }

        public static implicit operator int (MapValue value) {
            return value.value;
        }

        public static implicit operator MapValue (int value) {
            return new MapValue(value);
        }

        public static MapValue operator +(MapValue first, MapValue second) {
            return new MapValue(first.value + second.value);
        }

        public static MapValue operator -(MapValue first, MapValue second) {
            return new MapValue(first.value - second.value);
        }

        /// <summary>
        /// Convert unity value to lower bound map value.
        /// 
        /// For example, if the factor is 0.5, and input 'value' is 2.2,
        /// then the lower bound of map value is ceil(2.2 / 0.5) = 4.
        /// </summary>
        public static MapValue LowerBound(float value) {
            return Mathf.CeilToInt(value / MapResolution.kMapToUnityResolutionFactor);
        }

        /// <summary>
        /// Convert unity value to upper bound map value.
        /// 
        /// For example, if the factor is 0.5, and input 'value' is 2.2,
        /// then the upper bound of map value is floor(2.2 / 0.5) = 5.
        /// </summary>
        public static MapValue UpperBound(float value) {
            return Mathf.FloorToInt(value / MapResolution.kMapToUnityResolutionFactor);
        }

        public static MapValue RoundValue(float value) {
            return Mathf.RoundToInt(value / MapResolution.kMapToUnityResolutionFactor);
        }

        /// <summary>
        /// Convert unity value to real map value (float)
        /// </summary>
        public static float RealValue(float value) {
            return value / MapResolution.kMapToUnityResolutionFactor;
        }
    }

    /// <summary>
    /// Represents a rect in map resolution.
    /// </summary>
    public struct MapRect {
        public int x { get; private set; }
        public int y { get; private set; }
        public int width { get; private set; }
        public int height { get; private set; }

        public int xMin {
            get { return x; }
        }
        public int yMin {
            get { return y; }
        }
        public int xMax {
            get { return x + width; }
        }
        public int yMax {
            get { return y + height; }
        }

        public MapVector topLeft {
            get { return new MapVector(x, y + height); }
        }
        public MapVector topRight {
            get { return new MapVector(x + width, y + height); }
        }
        public MapVector bottomLeft {
            get { return new MapVector(x, y); }
        }
        public MapVector bottomRight {
            get { return new MapVector(x + width, y); }
        }

        public MapRect(int x, int y, int width, int height) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public static MapRect MinMaxRect(int x_min, int y_min, int x_max, int y_max) {
            return new MapRect(x_min, y_min, x_max - x_min, y_max - y_min);
        }

        public static MapRect FromPoint(MapVector point) {
            return new MapRect(point.x, point.y, 0, 0);
        }

        public static explicit operator Rect(MapRect rect) {
            return new Rect(
                rect.x * MapResolution.kMapToUnityResolutionFactor,
                rect.y * MapResolution.kMapToUnityResolutionFactor,
                rect.width * MapResolution.kMapToUnityResolutionFactor,
                rect.height * MapResolution.kMapToUnityResolutionFactor);
        }
    }
}
