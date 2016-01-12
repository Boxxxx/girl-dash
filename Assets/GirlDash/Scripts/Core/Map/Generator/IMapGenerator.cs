using System.Collections;

namespace GirlDash.Map {
    public interface IMapGenerator {
        IEnumerator Generate();
        MapData GetMap();
    }
}
