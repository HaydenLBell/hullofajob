using UnityEngine;
namespace cowsins
{
    public class HurtTrigger : Trigger
    {
        [SerializeField] private float damage;
        [SerializeField] private float cooldown = 1f;

        private float timer;

        private void Awake() => timer = 0;

        private void Update()
        {
            if (timer > 0) timer -= Time.deltaTime;
        }

        public override void TriggerStay(Collider other)
        {
            if (timer <= 0)
            {
                if (other.TryGetComponent(out IDamageable damageable))
                {
                    if (damageable is PlayerStats stats && stats.IsDead) return;
                    if (damageable is EnemyHealth enemy && enemy.IsDead) return;

                    DamageService.EnvironmentalContext = true;
                    DamageService.RequestDamage(damageable, damage, false);
                    timer = cooldown;
                }
            }
        }
    }
}
