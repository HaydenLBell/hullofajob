using UnityEngine;
using System.Collections;

namespace cowsins
{
    public class QuickActionBehaviour
    {
        private WeaponContext context;
        private InputManager inputManager;
        private IWeaponBehaviourProvider weaponBehaviour;
        private IWeaponReferenceProvider weaponReference;
        private IPlayerMovementStateProvider playerMovement;
        private IPlayerControlProvider playerControl;
        private IWeaponEventsProvider weaponEvents;
        private IPlayerMultipliers playerMultipliers;

        private Weapon_SO weapon => weaponReference.Weapon;
        private WeaponIdentification id => weaponReference.Id;
        private Camera mainCamera => weaponReference.MainCamera;
        private Transform weaponHolder => context.WeaponHolder;

        private WeaponControllerSettings settings;

        private Coroutine meleeCoroutine;
        private Coroutine reEnableMeleeCoroutine;
        private readonly Collider[] meleeHitColliders = new Collider[10];

        private MonoBehaviour CoroutineRunner => context.CoroutineRunner;
        
        private Transform playerRoot;

        public QuickActionBehaviour(WeaponContext context)
        {
            this.context = context;
            this.inputManager = context.InputManager;
            this.weaponBehaviour = context.Dependencies.WeaponBehaviour;
            this.weaponReference = context.Dependencies.WeaponReference;
            this.playerMovement = context.Dependencies.PlayerMovementState;
            this.playerControl = context.Dependencies.PlayerControl;
            this.weaponEvents = context.Dependencies.WeaponEvents;
            this.playerMultipliers = context.Dependencies.PlayerMultipliers;

            this.settings = context.Settings;
            weaponBehaviour.IsMeleeAvailable = true;
            this.playerRoot = context.Transform.root;
        }

        public bool CanExecute()
        {
            return playerControl.IsControllable && playerControl.ActionsControllable && settings.canMelee && weaponBehaviour.IsMeleeAvailable && !playerMovement.IsClimbing;
        }

        public void SecondaryMeleeAttack()
        {
            weaponBehaviour.IsMeleeAvailable = false;
            settings.meleeObject.SetActive(true);

            if (meleeCoroutine != null) CoroutineRunner.StopCoroutine(meleeCoroutine);
            meleeCoroutine = CoroutineRunner.StartCoroutine(MeleeRoutine());

            weaponEvents.Events.OnSecondaryAttack?.Invoke(settings.meleeHeadBone);

            // Play melee audio
            if (settings.meleeAudioClip != null && context.AudioSource != null)
            {
                context.AudioSource.PlayOneShot(settings.meleeAudioClip);
            }
        }

        private IEnumerator MeleeRoutine()
        {
            yield return new WaitForSeconds(settings.meleeDelay);
            Melee();
        }

        private void Melee()
        {
            settings.userEvents.OnSecondaryMelee?.Invoke();
            MeleeAttack(settings.meleeRange, settings.meleeAttackDamage);
            weaponEvents.Events.OnShootShake?.Invoke(settings.meleeCamShakeAmount * weaponBehaviour.AimingCamShakeMultiplier * weaponBehaviour.CrouchingCamShakeMultiplier);
        }

        public void FinishMelee()
        {
            settings.meleeObject.SetActive(false);

            if (reEnableMeleeCoroutine != null) CoroutineRunner.StopCoroutine(reEnableMeleeCoroutine);
            reEnableMeleeCoroutine = CoroutineRunner.StartCoroutine(ReEnableMeleeRoutine());
        }

        private IEnumerator ReEnableMeleeRoutine()
        {
            yield return new WaitForSeconds(settings.reEnableMeleeAfterAction);
            weaponBehaviour.IsMeleeAvailable = true;
        }

        /// <summary>
        /// Moreover, cowsins� FPS ENGINE also supports melee attacking
        /// Use this for Swords, knives etc
        /// </summary>
        private void MeleeAttack(float attackRange, float damage)
        {
            Vector3 basePosition = id != null ? id.transform.position : context.Transform.position;
            int hits = Physics.OverlapSphereNonAlloc(basePosition + mainCamera.transform.parent.forward * attackRange / 2, attackRange, meleeHitColliders, settings.hitLayer);

            float dmg = damage * playerMultipliers.DamageMultiplier.Value;

            for (int i = 0; i < hits; i++)
            {
                var c = meleeHitColliders[i];
                if (CowsinsUtilities.IsDamageableHitbox(c))
                {
                    var parent = CowsinsUtilities.GatherDamageableParent(c.transform);
                    if (parent != null)
                        DamageService.RequestDamage(parent, dmg, false);
                    break;
                }

                if (c.TryGetComponent<IDamageable>(out var damageable))
                {
                    DamageService.RequestDamage(damageable, dmg, false);
                    break;
                }
            }

            //VISUALS
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, attackRange, settings.hitLayer))
            {
                weaponEvents.Events.OnHit?.Invoke(hit.collider.gameObject.layer, 0f, hit, false);
            }
        }
    }

}
