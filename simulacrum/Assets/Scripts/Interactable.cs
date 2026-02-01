public enum InteractionMode
{
    CanInteract,
    CannontInteract,
    None
}

interface Interactable
{
    InteractionMode CanInteract(PlayerInputControls player);
    void SetInteractionMode(InteractionMode mode);

    void Interact();
}