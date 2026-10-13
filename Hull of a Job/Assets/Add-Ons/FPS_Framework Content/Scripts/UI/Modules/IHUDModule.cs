namespace cowsins
{
    /// <summary>
    /// Introduced in FPS Engine 1.6. Replaces UIController with Modular HUD sections.
    /// Each module manages its own UI elements and event subscriptions.
    /// </summary>
    public interface IHUDModule
    {
        void Initialize(PlayerDependencies dependencies);
    }
}
