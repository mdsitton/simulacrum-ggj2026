using UnityEngine;

public class Door : MonoBehaviour, Interactable
{
    public SpriteRenderer Access;
    public SpriteRenderer NoAccess;
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
        return InteractionMode.CanInteract;
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
