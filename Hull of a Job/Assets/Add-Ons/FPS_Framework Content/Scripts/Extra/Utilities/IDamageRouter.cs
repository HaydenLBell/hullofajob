namespace cowsins
{
    /// <summary>
    /// Routes damage requests through the appropriate channel.
    /// </summary>
    public interface IDamageRouter
    {
        void RequestDamage(IDamageable target, float damage, bool isHeadshot);
    }
}
