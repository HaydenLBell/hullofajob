namespace cowsins 
{ 
    public interface IDamageable 
    { 
        void Damage(float damage, bool isHeadshot); 
        float Health { get; } 
        float Shield { get; } 
        bool IsDead { get; } 
    } 
}
