using System.Collections.Generic;

namespace hb.framework
{
    public interface IPlayerMultipliers
    {
        IReadOnlyList<PlayerMultipliers.TemporaryModifierData> ActiveTemporaryModifiers { get; }
        
        ModifiableStat DamageMultiplier { get; }
        ModifiableStat HealMultiplier { get; }
        ModifiableStat WeightMultiplier { get; }
        ModifiableStat DamageTakenMultiplier { get; }

        void AddTemporaryModifier(ModifiableStat stat, StatModifier modifier, float duration);
        void RemoveModifierFromSource(object source);
    }
}
