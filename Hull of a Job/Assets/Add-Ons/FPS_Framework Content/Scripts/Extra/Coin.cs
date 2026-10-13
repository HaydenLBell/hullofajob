using UnityEngine;

namespace cowsins
{
    public class Coin : Trigger
    {
        [SerializeField] private int minCoins, maxCoins;

        [SerializeField] private AudioClip collectCoinSFX;
        public override void TriggerEnter(Collider other)
        {
            int amountOfCoins = Random.Range(minCoins, maxCoins);
            other.GetComponentInParent<PlayerDependencies>().ProgressionManager.coinService.AddCoins(amountOfCoins, true);
            
            SoundManager.Instance.PlaySound(collectCoinSFX, 0, 1, false);
            SpawnService.Despawn(gameObject);
        }


#if SAVE_LOAD_ADD_ON
        public override void LoadedState()
        {
            Destroy(this.gameObject);
        }
#endif
    }

}
