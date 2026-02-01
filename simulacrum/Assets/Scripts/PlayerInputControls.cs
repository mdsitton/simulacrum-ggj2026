using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.VFX;
using static InputSystem_Actions;

public class PlayerInputControls : MonoBehaviour
{
    public PlayerAnimator CurrentPlayerAnimator;
    public VisualEffect Transfer;

    public GameObject TransferTarget;

    public Vector2 Look { get; private set; }

    public Vector2 Move { get; private set; }
    private Vector2 previousMove;

    private InputSystem_Actions actions;

    public void Awake()
    {
        actions = new InputSystem_Actions();
        actions.Player.Enable();
    }

    /// <summary>
    /// Calculates the CharacterDirection from a direction vector using 8-way directional angles.
    /// Uses standard mathematical angles where:
    /// - 0°/360° = East (right, +X)
    /// - 90° = North (up, +Y)
    /// - 180° = West (left, -X)
    /// - 270° = South (down, -Y)
    /// 
    /// Sprite angles are offset by 90° clockwise from standard math angles:
    /// - Sprite 45° = Northeast = Math 45°
    /// - Sprite 90° = East = Math 0°
    /// - Sprite 135° = Southeast = Math -45° (315°)
    /// - Sprite 180° = South = Math -90° (270°)
    /// - Sprite 225° = Southwest = Math -135° (225°)
    /// - Sprite 270° = West = Math 180°
    /// - Sprite 315° = Northwest = Math 135°
    /// - Sprite 360° = North = Math 90°
    /// </summary>
    private CharacterDirection GetDirectionFromVector(Vector2 directionVector)
    {
        // Calculate angle in degrees using standard math convention
        // Atan2(y, x) gives: 0° = right, 90° = up, 180°/-180° = left, -90° = down
        // Negate X to mirror horizontally (fixes left/right being swapped)
        float mathAngle = Mathf.Atan2(directionVector.y, -directionVector.x) * Mathf.Rad2Deg;

        // Convert math angle to sprite angle:
        // After X-flip: 0° = left, 90° = up, 180° = right, -90° = down
        // Sprite: 90° = East (right), 360° = North (up), 270° = West (left), 180° = South (down)
        // Adding 90° aligns the angles correctly
        float spriteAngle = mathAngle + 90f;

        // Normalize to 0-360 range
        if (spriteAngle <= 0f)
            spriteAngle += 360f;
        if (spriteAngle > 360f)
            spriteAngle -= 360f;

        // Map to 8 discrete directions with 45° sectors
        // Each sector is centered on its angle (±22.5°)
        if (spriteAngle > 337.5f || spriteAngle <= 22.5f)
            return CharacterDirection.Angle360; // North (up)
        else if (spriteAngle > 22.5f && spriteAngle <= 67.5f)
            return CharacterDirection.Angle45;  // Northeast
        else if (spriteAngle > 67.5f && spriteAngle <= 112.5f)
            return CharacterDirection.Angle90;  // East (right)
        else if (spriteAngle > 112.5f && spriteAngle <= 157.5f)
            return CharacterDirection.Angle135; // Southeast
        else if (spriteAngle > 157.5f && spriteAngle <= 202.5f)
            return CharacterDirection.Angle180; // South (down)
        else if (spriteAngle > 202.5f && spriteAngle <= 247.5f)
            return CharacterDirection.Angle225; // Southwest
        else if (spriteAngle > 247.5f && spriteAngle <= 292.5f)
            return CharacterDirection.Angle270; // West (left)
        else // spriteAngle > 292.5f && spriteAngle <= 337.5f
            return CharacterDirection.Angle315; // Northwest
    }

    private void Update()
    {
        if (CurrentPlayerAnimator == null) return;

        previousMove = Move;
        var move = actions.Player.Move.ReadValue<Vector2>();

        // Only process when movement changes
        if (move != previousMove)
        {
            Move = move;

            if (Move != Vector2.zero)
            {
                // Only update direction when actually moving - keeps last direction when stopping
                CharacterDirection newDirection = GetDirectionFromVector(Move);
                CurrentPlayerAnimator.SetDirection(newDirection);
                CurrentPlayerAnimator.SetState(CharacterState.Walking);
            }
            else
            {
                // Stop moving but keep facing the last direction
                CurrentPlayerAnimator.SetState(CharacterState.Standing);
            }
        }

        // Apply velocity every frame, separate from facing direction
        CurrentPlayerAnimator.SetVelocity(Move);

        if (actions.Player.Interact.WasPressedThisFrame())
        {
           Interact();
        }
    }

    void Interact()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(CurrentPlayerAnimator.GetLocation().position, (float)0.5);
        foreach (Collider2D other in hitColliders)
        {                
            Door door = other.GetComponent<Door>();
            if (door != null)
            {
                door.Open();
                return;
            }                
        }

        hitColliders = Physics2D.OverlapCircleAll(CurrentPlayerAnimator.GetLocation().position, (float)3);        
        foreach (Collider2D other in hitColliders)
        {
            PlayerAnimator target = other.GetComponent<PlayerAnimator>();
            if (target != null && target != CurrentPlayerAnimator)
            {
                UnityEngine.Debug.Log("Switch");
                CurrentPlayerAnimator.SetState(CharacterState.Standing);  
                
                Transfer.transform.position = CurrentPlayerAnimator.GetTransferTarget().position;
                TransferTarget.transform.position = target.GetTransferTarget().position;

                Transfer.Play();

                CurrentPlayerAnimator = target;
                Move = Vector2.zero;
                return;
            }              
        }
    }
}
