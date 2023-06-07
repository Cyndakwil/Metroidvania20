using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Interactor))]
public class InteractionDefinition : MonoBehaviour
{
    public InputActionReference _inputType;

    [Header("Options")]
    [Tooltip("Set to true if you want the player to be able to move while the interaction is in progress.")]
    public bool _canMoveDuringInteraction;

    [Tooltip("The interactions in this list will exclusively permitted to override this interaction.")]
    public List<InteractionDefinition> _overrideableInteractions;

    [Space(10)]

    [Header("Enter Stage")]
    [InspectorName("Length")]
    public float _enterLength = 0.0f;

    [Header("Update Stage")]
    [InspectorName("Length")]
    public float _updateLength = 0.0f;

    [Header("Exit Stage")]
    [InspectorName("Length")]
    public float _exitLength = 0.0f;

    private Interactor _owner;

    // Start is called before the first frame update
    void Start()
    {
        _inputType.action.performed += OnInteractionInputPerformed;
    }

    bool IsAllowedToOverride(InteractionDefinition currentInteraction)
    {
        bool result = false;

        if (currentInteraction._overrideableInteractions.Exists(interaction => interaction.GetType() == this.GetType()))
        {
            result = true;
        }

        return result;
    }

    // Extend this to add a custom validity check for your interaction
    virtual public bool IsInteractionPossible(PlayerInteractionHandler playerInteractionHandler)
    {
        bool isPossible = true;

        if (playerInteractionHandler.GetCurrentInteraction())
        {
            isPossible = IsAllowedToOverride(playerInteractionHandler.GetCurrentInteraction());
        }

        return isPossible;
    }

    public Interactor GetOwner()
    {
        return _owner;
    }

    public void SetOwner(Interactor newOwner)
    {
        _owner = newOwner;
    }

    // Start Interaction
    private void OnInteractionInputPerformed(InputAction.CallbackContext ctx)
    {
        PlayerInteractionHandler player = GetOwner().GetOwner().GetOverlappingPlayer();
        if (player && player.GetPossibleInteractions().Contains(this))
        {
            // Perform the interaction
            Debug.Log("Player performed " + ctx.action + "input on " + gameObject.name);

            player.SetCurrentInteraction(this);

            OnInteractionEnterStart(player);
        }
    }

    // Montage callback functions (implement in derived classes)
    public virtual void OnInteractionEnterStart(PlayerInteractionHandler player) { }
    public virtual void OnInteractionEnterTick(PlayerInteractionHandler player) { }
    public virtual void OnInteractionEnterEnd(PlayerInteractionHandler player) { }

    public virtual void OnInteractionUpdateStart(PlayerInteractionHandler player) { }
    public virtual void OnInteractionUpdateTick(PlayerInteractionHandler player) { }
    public virtual void OnInteractionUpdateEnd(PlayerInteractionHandler player) { }

    public virtual void OnInteractionExitStart(PlayerInteractionHandler player) { }
    public virtual void OnInteractionExitTick(PlayerInteractionHandler player) { }
    public virtual void OnInteractionExitEnd(PlayerInteractionHandler player) { }
}
