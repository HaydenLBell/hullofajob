using UnityEngine;

namespace hb.framework
{
    public interface ISpawnService
    {
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation);
        void Despawn(GameObject obj);
    }
}
