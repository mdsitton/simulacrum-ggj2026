using UnityEngine;

public class Door : MonoBehaviour, Interactable
{
    public SpriteRenderer Access;
    public SpriteRenderer NoAccess;

    public CharacterColor DoorColor = CharacterColor.None;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        gameObject.SetActive(false);
        SetInteractionMode(InteractionMode.None);
    }

    public InteractionMode CanInteract(PlayerInputControls player)
    {
        if (DoorColor == CharacterColor.None || DoorColor == player.CurrentPlayerAnimator.CharacterColor){
            return InteractionMode.CanInteract;            
        } else
        {
            return InteractionMode.CannontInteract;
        }
    } 

    public void SetInteractionMode(InteractionMode mode)
    {
        switch (mode)
        {
            case InteractionMode.CanInteract:
                Access.enabled = true;
                NoAccess.enabled = false;
                break;
            case InteractionMode.CannontInteract:
                Access.enabled = false;
                NoAccess.enabled = true;
                break;
            case InteractionMode.None:
                Access.enabled = false;
                NoAccess.enabled = false;
                break;
        }
    }
}
