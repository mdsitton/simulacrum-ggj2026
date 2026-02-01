using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerSpriteSet", menuName = "Assets/Player Sprite Set", order = 1)]
public class PlayerSpriteSet : ScriptableObject
{
    [Header("Idle Animations - 8 Directions")]
    [SerializeField] Sprite[] idle45;
    [SerializeField] Sprite[] idle90;
    [SerializeField] Sprite[] idle135;
    [SerializeField] Sprite[] idle180;
    [SerializeField] Sprite[] idle225;
    [SerializeField] Sprite[] idle270;
    [SerializeField] Sprite[] idle315;
    [SerializeField] Sprite[] idle360;
    public int idleAnimationTime = 8;

    [Header("Sitting Animations - 8 Directions")]
    [SerializeField] Sprite[] sittingSprites = new Sprite[8];

    [Header("Death Animations")]
    [SerializeField] Sprite dieFront;
    [SerializeField] Sprite dieLeft;

    [Header("Walking Animations - 8 Directions")]
    [SerializeField] Sprite[] walking45;   // Northeast
    [SerializeField] Sprite[] walking90;   // East
    [SerializeField] Sprite[] walking135;  // Southeast
    [SerializeField] Sprite[] walking180;  // South
    [SerializeField] Sprite[] walking225;  // Southwest
    [SerializeField] Sprite[] walking270;  // West
    [SerializeField] Sprite[] walking315;  // Northwest
    [SerializeField] Sprite[] walking360;  // North
    public int walkingAnimationTime = 1;

    // Cached arrays for runtime lookup - indexed by CharacterDirection (0-7)
    private Sprite[][] idleAnimations;
    private Sprite[][] walkingAnimations;

    private void OnEnable()
    {
        CacheAnimationArrays();
    }

    private void CacheAnimationArrays()
    {
        idleAnimations = new Sprite[][]
        {
            idle45, idle90, idle135, idle180,
            idle225, idle270, idle315, idle360
        };

        walkingAnimations = new Sprite[][]
        {
            walking45, walking90, walking135, walking180,
            walking225, walking270, walking315, walking360
        };
    }

    private int GetDirectionIndex(CharacterDirection direction)
    {
        return (int)direction & 7; // Maps Angle45=0, Angle90=1, ..., Angle360=7
    }

    private Sprite[] GetIdleAnimation(CharacterDirection direction)
    {
        int index = GetDirectionIndex(direction);
        Sprite[] anim = idleAnimations?[index];
        return (anim != null && anim.Length > 0) ? anim : idleAnimations?[3]; // Fallback to idle180
    }

    private Sprite[] GetWalkingAnimation(CharacterDirection direction)
    {
        int index = GetDirectionIndex(direction);
        Sprite[] anim = walkingAnimations?[index];
        return (anim != null && anim.Length > 0) ? anim : walkingAnimations?[3]; // Fallback to walking180
    }

    public (Sprite sprite, int frameCount, float frameTime) GetSprite(CharacterState state, CharacterDirection direction, int frameIndex = 0)
    {
        int dirIndex = GetDirectionIndex(direction);

        switch (state)
        {
            case CharacterState.Standing:
                var idleAnimation = GetIdleAnimation(direction);
                var frameTime = (float)idleAnimationTime / idleAnimation.Length;
                if (idleAnimation != null && idleAnimation.Length > 0)
                    return (idleAnimation[frameIndex % idleAnimation.Length], idleAnimation.Length, frameTime);
                return (null, 0, frameTime);

            case CharacterState.Sitting:
                if (sittingSprites[dirIndex] != null)
                    return (sittingSprites[dirIndex], 1, 1.0f);
                return (sittingSprites[3], 1, 1.0f);

            case CharacterState.Die:
                // Death only has front and left variants
                var spriteDie = direction switch
                {
                    CharacterDirection.Angle45 => dieFront,
                    CharacterDirection.Angle90 => dieLeft,
                    CharacterDirection.Angle135 => dieLeft,
                    CharacterDirection.Angle180 => dieFront,
                    CharacterDirection.Angle225 => dieLeft,
                    CharacterDirection.Angle270 => dieLeft,
                    CharacterDirection.Angle315 => dieLeft,
                    CharacterDirection.Angle360 => dieFront,
                    _ => dieFront,
                };
                return (spriteDie, 1, 1.0f);

            case CharacterState.Walking:
                var spriteAnimation = GetWalkingAnimation(direction);
                var walkFrameTime = (float)walkingAnimationTime / spriteAnimation.Length;
                if (spriteAnimation != null && spriteAnimation.Length > 0)
                    return (spriteAnimation[frameIndex % spriteAnimation.Length], spriteAnimation.Length, walkFrameTime);
                return (null, 0, walkFrameTime);
        }

        return (null, 0, 0f); // Default case if no sprite is found
    }
}
