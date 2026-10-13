using UnityEngine;

namespace cowsins
{
    public static class SpawnService
    {
        private static ISpawnService provider;

        public static ISpawnService Provider
        {
            get => provider ?? DefaultSpawnService.Instance;
            set => provider = value;
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
            => Provider.Spawn(prefab, position, rotation);

        public static void Despawn(GameObject obj)
            => Provider.Despawn(obj);

        /// <summary>
        /// Resets to the default singleplayer spawner. Called on scene transitions / cleanup.
        /// </summary>
        public static void Reset() => provider = null;

        private sealed class DefaultSpawnService : ISpawnService
        {
            public static readonly DefaultSpawnService Instance = new DefaultSpawnService();
            public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
                => Object.Instantiate(prefab, position, rotation);
            public void Despawn(GameObject obj) => Object.Destroy(obj);
        }
    }
}
