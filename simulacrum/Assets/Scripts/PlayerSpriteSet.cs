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
}