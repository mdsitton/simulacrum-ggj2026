using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerSpriteSet", menuName = "Assets/Player Sprite Set", order = 1)]
public class PlayerSpriteSet : ScriptableObject
{
    [SerializeField]
    Sprite standingFront;

    [SerializeField]
    Sprite standingBack;

    [SerializeField]
    Sprite standingLeft;

    [SerializeField]
    Sprite standingRight;

    [SerializeField]
    Sprite sittingFront;

    [SerializeField]
    Sprite sittingBack;

    [SerializeField]
    Sprite sittingLeft;

    [SerializeField]
    Sprite sittingRight;

    [SerializeField]
    Sprite dieFront;

    [SerializeField]
    Sprite dieLeft;

    [SerializeField]
    Sprite[] walkingFront;
    [SerializeField]
    Sprite[] walkingBack;
    [SerializeField]
    Sprite[] walkingLeft;
    [SerializeField]
    Sprite[] walkingRight;

    public (Sprite sprite, int frameCount) GetSprite(CharacterState state, CharacterDirection direction, int frameIndex = 0)
    {
        switch (state)
        {
            case CharacterState.Standing:
                var spriteStand = direction switch
                {
                    CharacterDirection.Front => standingFront,
                    CharacterDirection.Back => standingBack,
                    CharacterDirection.Left => standingLeft,
                    CharacterDirection.Right => standingRight,
                    _ => throw new System.NotImplementedException()
                };
                return (spriteStand, 1);

            case CharacterState.Sitting:
                var spriteSit = direction switch
                {
                    CharacterDirection.Front => sittingFront,
                    CharacterDirection.Back => sittingBack,
                    CharacterDirection.Left => sittingLeft,
                    CharacterDirection.Right => sittingRight,
                    _ => throw new System.NotImplementedException(),
                };
                return (spriteSit, 1);
            case CharacterState.Die:
                var spriteDie = direction switch
                {
                    CharacterDirection.Front => dieFront,
                    CharacterDirection.Back => dieFront,
                    CharacterDirection.Left => dieLeft,
                    CharacterDirection.Right => dieLeft,
                    _ => throw new System.NotImplementedException(),
                };
                return (spriteDie, 1);

            case CharacterState.Walking:
                var spriteAnimation = direction switch
                {
                    CharacterDirection.Front => walkingFront,
                    CharacterDirection.Back => walkingBack,
                    CharacterDirection.Left => walkingLeft,
                    CharacterDirection.Right => walkingRight,
                    _ => throw new System.NotImplementedException(),
                };

                return (spriteAnimation[frameIndex % spriteAnimation.Length], spriteAnimation.Length);
        }

        return (null, 0); // Default case if no sprite is found
    }
}