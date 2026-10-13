namespace cowsins
{
    // Implemented by PlayerControl and required by PlayerDependencies
    public interface IPlayerControlProvider
    {
        bool IsControllable { get; }
        bool IsMovementControllable { get; }
        bool CameraControllable { get; }
        bool ActionsControllable { get; }
        bool ShootingControllable { get; }

        void GrantControl();
        void LoseControl();
        void CheckIfCanGrantControl();
    }
}
