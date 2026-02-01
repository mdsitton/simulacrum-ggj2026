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

public class PlayerAnimator : MonoBehaviour
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

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerBody = GetComponent<Rigidbody2D>();
        frameTime = 1f / framerate;
    }

    public void SetState(CharacterState newState)
    {
        if (state == newState) return;

        state = newState;
        stateResetNeeded = true;
        UpdateSprite();

        if (state == CharacterState.Standing)
        {
            playerBody.linearVelocity = Vector2.zero;
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

    private void Update()
    {
        animationTimer += Time.deltaTime;
        if (animationTimer >= frameTime)
        {
            animationTimer = 0f;
            frameIndex = (frameIndex + 1) % frameCount;
        }
        UpdateSprite();
    }
}
