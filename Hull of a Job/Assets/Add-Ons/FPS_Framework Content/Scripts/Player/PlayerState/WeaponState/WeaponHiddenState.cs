using UnityEngine;

namespace cowsins
{
    public class WeaponHiddenState : WeaponBaseState
    {
        public WeaponHiddenState(WeaponStates currentContext, WeaponStateFactory playerStateFactory)
            : base(currentContext, playerStateFactory)
        {
        }

        private static readonly int HitHash = Animator.StringToHash("hit");

        public sealed override void EnterState()
        {
            CowsinsUtilities.PlayAnim(HitHash, _ctx.WeaponAnimator.HolsterMotionObject);
        }

        public sealed override void UpdateState()
        {
        }

        public sealed override void FixedUpdateState()
        {
        }

        public sealed override void ExitState() 
        {
            _ctx.WeaponAnimator.HolsterMotionObject.Play("MeleeShowWeapon", 0, 0);
        }

        public sealed override void CheckSwitchState() {}
    }

}
