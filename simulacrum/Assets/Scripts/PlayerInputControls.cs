using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using static InputSystem_Actions;

public class PlayerInputControls : MonoBehaviour
{
    public PlayerAnimator CurrentPlayerAnimator;

    public Vector2 Look { get; private set; }

    public Vector2 Move { get; private set; }
    private Vector2 previousMove;

    private InputSystem_Actions actions;

    public void Awake()
    {
        actions = new InputSystem_Actions();
        actions.Player.Enable();
    }

    private void Update()
    {
        // Look = actions.Player.MouseClick.ReadValue<bool>() ? actions.Player.MouseMove.ReadValue<Vector2>() : Look; // Update Look only on mouse click keep last value otherwise
        previousMove = Move;
        var move = actions.Player.Move.ReadValue<Vector2>();

        var moveDelta = move - previousMove;
        if (moveDelta != Vector2.zero)
        {
            Move = move;

            if (moveDelta.x < 0)
            {
                CurrentPlayerAnimator.SetDirection(CharacterDirection.Right);
            }
            else if (moveDelta.x > 0)
            {
                CurrentPlayerAnimator.SetDirection(CharacterDirection.Left);
            }
            else if (moveDelta.y > 0)
            {
                CurrentPlayerAnimator.SetDirection(CharacterDirection.Back);
            }
            else if (moveDelta.y < 0)
            {
                CurrentPlayerAnimator.SetDirection(CharacterDirection.Front);
            }

            if (Move != Vector2.zero)
            {
                CurrentPlayerAnimator.SetState(CharacterState.Walking);
            }
            else
            {
                CurrentPlayerAnimator.SetState(CharacterState.Standing);
            }
        }


        // Apply velocity every frame, separate from facing direction
        CurrentPlayerAnimator.SetVelocity(Move);
    }
}