using UnityEngine;

public enum CharacterState
{
    Standing,
    Walking,
    Sitting,
    Die,
}

public enum CharacterDirection
{
    Angle45,   // Northeast
    Angle90,   // East
    Angle135,  // Southeast
    Angle180,  // South
    Angle225,  // Southwest
    Angle270,  // West
    Angle315,  // Northwest
    Angle360   // North (same as 0 degrees)
}

public class PlayerAnimator : MonoBehaviour, Interactable
{
    [SerializeField]
    private PlayerSpriteSet spriteSet;
    private CharacterState state;
    private CharacterDirection direction;

    private bool stateResetNeeded = false;
    private bool directionResetNeeded = false;

    private SpriteRenderer spriteRenderer;

    private float framerate = 16f; // frames per second
    private float frameTime;

    private float animationTimer = 0f;

    private Rigidbody2D playerBody;

    public Transform GetLocation()
    {
        return playerBody.transform;
    }

    public Transform GetTransferTarget()
    {
        return transform.Find("TransferTarget");
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerBody = GetComponent<Rigidbody2D>();
        frameTime = 1f / framerate;

        // Start in standing state with position frozen
        state = CharacterState.Standing;
        playerBody.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void SetState(CharacterState newState)
    {
        if (state == newState) return;

        // Reset death flag when leaving Die state
        if (state == CharacterState.Die && newState != CharacterState.Die)
        {
            isDead = false;
        }

        state = newState;
        stateResetNeeded = true;
        UpdateSprite();

        if (state == CharacterState.Standing)
        {
            playerBody.linearVelocity = Vector2.zero;
            // Freeze position to prevent being pushed by collisions
            playerBody.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else if (state == CharacterState.Walking)
        {
            // Unfreeze position for movement, keep rotation frozen
            playerBody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else if (state == CharacterState.Die)
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    public void SetDirection(CharacterDirection newDirection)
    {
        if (direction == newDirection) return;

        direction = newDirection;
        directionResetNeeded = true;
        UpdateSprite();
    }

    public void SetVelocity(Vector2 moveVector)
    {
        if (state == CharacterState.Walking)
        {
            // Check if moving diagonally (both X and Y have input)
            bool isDiagonal = Mathf.Abs(moveVector.x) > 0.1f && Mathf.Abs(moveVector.y) > 0.1f;

            Vector2 velocity;
            if (isDiagonal)
            {
                // Diagonal movement follows isometric grid (2:1 ratio = 26.57° angle)
                // Horizontal component is 2x the vertical component
                float signX = Mathf.Sign(moveVector.x);
                float signY = Mathf.Sign(moveVector.y);
                velocity = new Vector2(signX * 2f, signY * 1f).normalized;
            }
            else
            {
                // Cardinal directions move straight up/down/left/right
                velocity = moveVector.normalized;
            }

            playerBody.linearVelocity = velocity * 1.5f;
        }
    }

    int frameIndex = 0;
    int frameCount = 0;

    private void UpdateSprite()
    {
        (Sprite sprite, var newFrameCount, var newFrameTime) = spriteSet.GetSprite(state, direction, frameIndex);

        if (sprite == null) return;
        frameCount = newFrameCount;
        frameTime = newFrameTime;
        if (stateResetNeeded)
        {
            frameIndex = 0;
            stateResetNeeded = false;
        }
        // Direction changes don't reset frame index - allows smooth animation transitions
        if (directionResetNeeded)
        {
            directionResetNeeded = false;
        }
        spriteRenderer.sprite = sprite;
    }

    private bool isDead = false;

    private void Update()
    {
        animationTimer += Time.deltaTime;
        if (animationTimer >= frameTime)
        {
            animationTimer = 0f;
            // Death animation plays once and stays on last frame
            if (state == CharacterState.Die)
            {
                if (!isDead && frameIndex < frameCount - 1)
                {
                    frameIndex++;
                }
                else
                {
                    isDead = true;
                    frameIndex = frameCount - 1; // Stay on last frame
                }
            }
            else
            {
                frameIndex = (frameIndex + 1) % frameCount;
            }
        }
        UpdateSprite();
    }

    public void Interact()
    {
        
        // TODO: Move part of transfer here?
    }

    public InteractionMode CanInteract(PlayerInputControls player)
    {
        if (player.CurrentPlayerAnimator != this)
        {            
          return InteractionMode.CanInteract;            
        } else
        {
            return InteractionMode.None;
        }
    } 

    public void SetInteractionMode(InteractionMode mode)
    {
        // switch (mode)
        // {
        //     case InteractionMode.CanInteract:
        //         Access.enabled = true;
        //         NoAccess.enabled = false;
        //         break;
        //     case InteractionMode.CannontInteract:
        //         Access.enabled = false;
        //         NoAccess.enabled = true;
        //         break;
        //     case InteractionMode.None:
        //         Access.enabled = false;
        //         NoAccess.enabled = false;
        //         break;
        // }
    }
}
