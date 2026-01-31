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

public class PlayerAnimator
{
    private PlayerSpriteSet spriteSet;

    private CharacterState state;
    private CharacterDirection direction;

    public PlayerAnimator(PlayerSpriteSet spriteSet)
    {
        this.spriteSet = spriteSet;
    }
}
