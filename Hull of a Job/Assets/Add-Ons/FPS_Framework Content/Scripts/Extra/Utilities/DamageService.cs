namespace cowsins
{
    /// <summary>
    /// Static service locator for damage routing.
    /// </summary>
    public static class DamageService
    {
        private static IDamageRouter router;

        public static bool EnvironmentalContext { get; set; }

        public static IDamageRouter Router
        {
            get => router ?? DefaultDamageRouter.Instance;
            set => router = value;
        }

        public static void RequestDamage(IDamageable target, float damage, bool isHeadshot)
        {
            try { Router.RequestDamage(target, damage, isHeadshot); }
            finally { EnvironmentalContext = false; }
        }

        public static void Reset() => router = null;

        private sealed class DefaultDamageRouter : IDamageRouter
        {
            public static readonly DefaultDamageRouter Instance = new DefaultDamageRouter();
            public void RequestDamage(IDamageable target, float damage, bool isHeadshot)
                => target?.Damage(damage, isHeadshot);
        }
    }
}
