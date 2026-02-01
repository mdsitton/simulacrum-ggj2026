using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.VFX;
using static InputSystem_Actions;

public class PlayerInputControls : MonoBehaviour
{
    public Vector3 cameraOffset = new Vector3();
    public float cameraFollowTime = 0.2f; 

    public PlayerAnimator CurrentPlayerAnimator;
    public VisualEffect Transfer;

    public GameObject TransferTarget;

    public Vector2 Look { get; private set; }

    public Vector2 Move { get; private set; }
    private Vector2 previousMove;
    private Camera mainCamera;
    private Vector3 cameraVelocity;

    private InputSystem_Actions actions;

    // Direction change throttling - minimum distance (in grid cells) before allowing direction change
    [SerializeField]
    private float gridCellSize = 1.0f;
    [SerializeField]
    private float minMouseThrottleDistance = 0.1f; // Minimum throttle distance when mouse is moving fast
    [SerializeField]
    private float mouseVelocityThreshold = 500.0f; // Screen pixels/sec at which throttle is minimized
    private Vector2 lastDirectionChangePosition;
    private CharacterDirection currentLockedDirection; // Direction locked until distance threshold is met
    private bool hasLockedDirection = false; // Whether we've locked a direction yet

    // Mouse click destination tracking
    [SerializeField]
    private float destinationThreshold = 0.5f; // Stop moving when within this distance of cursor
    private Vector2? mouseClickDestination = null; // World position of the mouse click destination
    private bool isMouseControlled = false; // Track if current movement is from mouse input

    // Mouse velocity tracking for dynamic throttling
    private Vector2 lastMouseScreenPosition;
    private float mouseVelocity = 0f;
    [SerializeField]
    private float mouseVelocitySmoothing = 0.1f; // Smoothing factor for mouse velocity (lower = smoother)

    // Lookup table mapping CharacterDirection to normalized Vector2 directions
    // Vectors are negated to match the sprite direction calculation which inverts the input
    private static readonly System.Collections.Generic.Dictionary<CharacterDirection, Vector2> DirectionVectors = new()
    {
        { CharacterDirection.Angle360, new Vector2(0, -1) },    // North sprite = move South
        { CharacterDirection.Angle45, new Vector2(-1, -1).normalized }, // Northeast sprite = move Southwest
        { CharacterDirection.Angle90, new Vector2(-1, 0) },     // East sprite = move West
        { CharacterDirection.Angle135, new Vector2(-1, 1).normalized }, // Southeast sprite = move Northwest
        { CharacterDirection.Angle180, new Vector2(0, 1) },     // South sprite = move North
        { CharacterDirection.Angle225, new Vector2(1, 1).normalized },  // Southwest sprite = move Northeast
        { CharacterDirection.Angle270, new Vector2(1, 0) },     // West sprite = move East
        { CharacterDirection.Angle315, new Vector2(1, -1).normalized }  // Northwest sprite = move Southeast
    };

    public void Awake()
    {
        mainCamera = Camera.main;
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

    /// <summary>
    /// Calculates the effective throttle distance for mouse input based on mouse velocity.
    /// Faster mouse movement = smaller threshold = more responsive direction changes.
    /// Slow/stationary mouse = larger threshold = smoother, rate-limited movement.
    /// </summary>
    private float GetMouseThrottleDistance()
    {
        // Scale: slow mouse uses full gridCellSize, fast mouse uses minMouseThrottleDistance
        float t = Mathf.Clamp01(mouseVelocity / mouseVelocityThreshold);
        return Mathf.Lerp(gridCellSize, minMouseThrottleDistance, t);
    }

    private void Update()
    {
        if (CurrentPlayerAnimator == null) return;

        previousMove = Move;
        var move = actions.Player.Move.ReadValue<Vector2>();
        float mouseClick = actions.Player.MouseClick.ReadValue<float>();
        bool isMouseInput = false; // Track if this frame's input is from mouse

        if (mouseClick > 0)
        {
            Vector2 playerPos = mainCamera.WorldToScreenPoint(CurrentPlayerAnimator.GetLocation().position);
            var mousePos = actions.Player.MouseMove.ReadValue<Vector2>();

            // Calculate mouse velocity (screen pixels per second)
            float mouseDelta = Vector2.Distance(mousePos, lastMouseScreenPosition);
            float instantVelocity = mouseDelta / Time.deltaTime;
            // Smooth the velocity to avoid jitter
            mouseVelocity = Mathf.Lerp(mouseVelocity, instantVelocity, mouseVelocitySmoothing);
            lastMouseScreenPosition = mousePos;

            var direction = (mousePos - playerPos).normalized;
            move = direction;
            isMouseInput = true;

            // Store the world position of the click destination
            mouseClickDestination = mainCamera.ScreenToWorldPoint(mousePos);
        }
        else if (move != Vector2.zero)
        {
            // Keyboard/controller input - clear mouse destination and switch to keyboard mode
            isMouseControlled = false;
            mouseClickDestination = null;
            // Decay mouse velocity when not using mouse
            mouseVelocity = Mathf.Lerp(mouseVelocity, 0f, mouseVelocitySmoothing);
        }
        else
        {
            // No input - decay mouse velocity
            mouseVelocity = Mathf.Lerp(mouseVelocity, 0f, mouseVelocitySmoothing);
        }


        // Check if we're close enough to the mouse click destination to stop
        if (mouseClickDestination.HasValue)
        {
            Vector2 currentPosition = CurrentPlayerAnimator.GetLocation().position;
            float distanceToDestination = Math.Abs(Vector2.Distance(currentPosition, mouseClickDestination.Value));

            if (distanceToDestination <= destinationThreshold)
            {
                // Reached destination - stop moving
                move = Vector2.zero;
                mouseClickDestination = null;
            }
        }

        // Only process when movement changes
        if (move != previousMove)
        {
            Move = move;

            // Update mouse control state when starting new movement
            if (move != Vector2.zero)
            {
                isMouseControlled = isMouseInput;
            }

            if (Move != Vector2.zero)
            {
                // Get the desired direction from input
                CharacterDirection desiredDirection = GetDirectionFromVector(Move);

                // Check if we can change direction (throttling based on distance traveled)
                // Only apply distance throttling for mouse input, not keyboard/controller
                Vector2 currentPosition = CurrentPlayerAnimator.GetLocation().position;
                float distanceSinceLastChange = Vector2.Distance(currentPosition, lastDirectionChangePosition);

                // Allow direction change if:
                // 1. We haven't locked a direction yet, OR
                // 2. Using keyboard/controller input (immediate response), OR
                // 3. Using mouse AND the desired direction is different AND we've traveled at least 1 grid cell
                if (!hasLockedDirection)
                {
                    // First movement - lock the direction
                    currentLockedDirection = desiredDirection;
                    lastDirectionChangePosition = currentPosition;
                    hasLockedDirection = true;
                    CurrentPlayerAnimator.SetDirection(currentLockedDirection);
                    CurrentPlayerAnimator.SetState(CharacterState.Walking);
                }
                else if (!isMouseControlled && desiredDirection != currentLockedDirection)
                {
                    // Keyboard/controller - allow immediate direction change
                    currentLockedDirection = desiredDirection;
                    lastDirectionChangePosition = currentPosition;
                    CurrentPlayerAnimator.SetDirection(currentLockedDirection);
                    CurrentPlayerAnimator.SetState(CharacterState.Walking);
                }
                else if (isMouseControlled && desiredDirection != currentLockedDirection && distanceSinceLastChange >= GetMouseThrottleDistance())
                {
                    // Mouse input - only change direction after traveling enough distance
                    // Threshold scales based on cursor distance (farther = more responsive)
                    currentLockedDirection = desiredDirection;
                    lastDirectionChangePosition = currentPosition;
                    CurrentPlayerAnimator.SetDirection(currentLockedDirection);
                    CurrentPlayerAnimator.SetState(CharacterState.Walking);
                }
                // Otherwise: keep current locked direction, just ensure we're walking
                else
                {
                    CurrentPlayerAnimator.SetState(CharacterState.Walking);
                }
            }
            else
            {
                // Stop moving but keep facing the last direction
                CurrentPlayerAnimator.SetState(CharacterState.Standing);
                // Reset the lock when stopping so next movement starts fresh
                hasLockedDirection = false;
                // Clear mouse destination when stopping
                mouseClickDestination = null;
            }
        }

        if (mainCamera != null)
        {            
            Vector3 desiredPosition = CurrentPlayerAnimator.GetLocation().position + cameraOffset;
            desiredPosition.z = mainCamera.transform.position.z;
            UnityEngine.Debug.Log("Desired Position " + desiredPosition);
            var lerp = Vector3.SmoothDamp(mainCamera.transform.position, desiredPosition, ref cameraVelocity, cameraFollowTime);
            UnityEngine.Debug.Log("Lerp " + lerp);
            mainCamera.transform.position  = lerp;
            UnityEngine.Debug.Log("Camera Position " + mainCamera.transform.position);

        }

        TransferTarget.transform.position = CurrentPlayerAnimator.GetTransferTarget().position;

        // Apply velocity using the locked 8-way cardinal direction
        // Use the locked direction instead of recalculating every frame
        if (Move != Vector2.zero && hasLockedDirection)
        {
            CurrentPlayerAnimator.SetVelocity(DirectionVectors[currentLockedDirection]);
        }
        else
        {
            CurrentPlayerAnimator.SetVelocity(Vector2.zero);
        }

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
                //TransferTarget posiition updated in update loop

                Transfer.Play();

                CurrentPlayerAnimator = target;
                Move = Vector2.zero;
                return;
            }
        }
    }
}
