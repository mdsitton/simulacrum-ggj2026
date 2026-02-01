using UnityEngine;
using UnityEngine.InputSystem;

public class WaypointController : MonoBehaviour, ICharacterController
{
    [SerializeField]
    [Tooltip("The PlayerAnimator to control at startup. Leave empty if activating via code.")]
    private PlayerAnimator initialAnimator;

    public Transform[] waypoints;
    private int currentWaypointIndex = 0;
    public float speed = 2f;

    CharacterState state = CharacterState.Walking;

    private PlayerAnimator playerAnimator;
    private bool isActive = false;

    private float decisionTimer = 0f;
    private const float decisionInterval = 1f;

    // ICharacterController implementation
    public bool IsActive => isActive;
    public PlayerAnimator ControlledAnimator => playerAnimator;

    void Awake()
    {
        if (initialAnimator != null)
        {
            Activate(initialAnimator);
        }
        Random.InitState(12345);
    }

    public void Activate(PlayerAnimator animator)
    {
        if (animator == null) return;

        playerAnimator = animator;
        isActive = true;
        playerAnimator.SetActiveController(this);

        // Reset state when activating
        state = CharacterState.Walking;
        decisionTimer = 0f;
    }

    public void Deactivate()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetState(CharacterState.Standing);
            playerAnimator.SetVelocity(Vector2.zero);
        }
        isActive = false;
        playerAnimator = null;
    }

    void Update()
    {
        if (!isActive || playerAnimator == null) return;
        if (waypoints == null || waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector2 currentPosition = playerAnimator.GetLocation().position;
        Vector2 direction = ((Vector2)targetWaypoint.position - currentPosition).normalized;

        if (Vector2.Distance(currentPosition, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        decisionTimer += Time.deltaTime;
        if (decisionTimer >= decisionInterval)
        {
            decisionTimer = 0f;
            var val = Random.Range(0, 100);

            if (val > 50)
            {
                state = CharacterState.Standing;
            }
            else
            {
                state = CharacterState.Walking;
            }
        }

        var dirEnum = PlayerInputControls.GetDirectionFromVector(direction);

        playerAnimator.SetState(state);
        playerAnimator.SetDirection(dirEnum);
        playerAnimator.SetVelocity(direction * speed);
    }
}