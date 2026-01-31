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

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetState(CharacterState newState)
    {
        state = newState;
        stateResetNeeded = true;
        UpdateSprite();
    }

    public void SetDirection(CharacterDirection newDirection)
    {
        direction = newDirection;
        stateResetNeeded = true;
        UpdateSprite();
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
        }
        spriteRenderer.sprite = sprite;
    }

    private void Update()
    {
        frameIndex = (frameIndex + 1) % frameCount;
        UpdateSprite();
    }
}
