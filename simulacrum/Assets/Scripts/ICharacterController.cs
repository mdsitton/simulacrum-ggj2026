using UnityEngine;

/// <summary>
/// Interface for controllers that can take control of a PlayerAnimator.
/// Allows switching between autonomous movement (WaypointController) 
/// and player-controlled movement (PlayerInputControls).
/// </summary>
public interface ICharacterController
{
    /// <summary>
    /// Activates this controller for the given PlayerAnimator.
    /// The controller should start controlling the animator after this call.
    /// </summary>
    /// <param name="animator">The PlayerAnimator to control</param>
    void Activate(PlayerAnimator animator);

    /// <summary>
    /// Deactivates this controller.
    /// The controller should stop controlling any animator after this call.
    /// </summary>
    void Deactivate();

    /// <summary>
    /// Returns true if this controller is currently active and controlling an animator.
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Returns the PlayerAnimator currently being controlled, or null if not active.
    /// </summary>
    PlayerAnimator ControlledAnimator { get; }
}
