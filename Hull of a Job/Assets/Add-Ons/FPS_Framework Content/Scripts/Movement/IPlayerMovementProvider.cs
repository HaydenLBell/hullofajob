using System;
using UnityEngine;
using UnityEngine.Events;

namespace cowsins
{
    // Implemented by PlayerMovement and required by PlayerDependencies
    public interface IPlayerMovementStateProvider
    {
        PlayerOrientation Orientation { get; }
        bool IsIdle { get; }
        float CurrentSpeed { get; set; }
        float CurrentVelocityMagnitude { get; }
        float RunSpeed { get; }
        float WalkSpeed { get; }
        float CrouchSpeed { get; }
        bool Grounded { get; set; }
        bool IsCrouching { get; set; }
        bool IsSliding { get; set; }
        bool IsClimbing { get; set; }
        bool IsWallRunning { get; set; }
        bool IsDashing { get; set; }
        bool CanShootWhileDashing { get; }
        bool DamageProtectionWhileDashing { get; }
        float NormalFOV { get; }
        float WallRunningFOV { get; }
        float RunningFOV { get; }
        float FadeFOVAmount { get; }
        bool UsesStamina { get; }
    }

    public interface IPlayerMovementEventsProvider
    {
        PlayerMovementEvents Events { get; }
    }
}
