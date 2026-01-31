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
    Front,
    Back,
    Left,
    Right
}

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    private PlayerSpriteSet spriteSet;
    private CharacterState state;
    private CharacterDirection direction;

    private bool stateResetNeeded = false;

    private SpriteRenderer spriteRenderer;

    private float framerate = 16f; // frames per second
    private float frameTime;

    private float animationTimer = 0f;

    private Rigidbody2D playerBody;

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
        stateResetNeeded = true;
        UpdateSprite();
    }

    public void SetVelocity(Vector2 moveVector)
    {
        if (state == CharacterState.Walking)
        {
            playerBody.linearVelocity = moveVector * 2f; // Example speed
        }
    }

    int frameIndex = 0;
    int frameCount = 0;

    private void UpdateSprite()
    {
        (Sprite sprite, var newFrameCount) = spriteSet.GetSprite(state, direction, frameIndex);
        frameCount = newFrameCount;
        if (stateResetNeeded)
        {
            frameIndex = 0;
            stateResetNeeded = false;
            Debug.Log("State reset");
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
