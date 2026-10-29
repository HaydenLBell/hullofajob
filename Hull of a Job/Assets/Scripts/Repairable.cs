using UnityEngine;
namespace hb.framework
{
    public class Repairable : RepairableHealth
    {

        public override void Start()
        {
            base.Start();
        }

        public override void Damage(float damage, bool isHeadshot)
        {
            base.Damage(damage, isHeadshot);
        }
        

#if SAVE_LOAD_ADD_ON
        public override void LoadedState()
        {
            if (health <= 0)
                Revive();
                
            healthBar.fillAmount = health / maxHealth;
            shieldBar.fillAmount = shield / maxShield;
        }
#endif
    }
}
