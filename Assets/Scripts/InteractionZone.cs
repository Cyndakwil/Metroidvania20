using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// InteractionZone uses a collider as a trigger for its interactions
[RequireComponent(typeof(Collider2D))]
public class InteractionZone : MonoBehaviour
{
    public Collider2D _triggerZone;
    public List<InteractionLock> _interactionLocks;

    private PlayerInteractionHandler _overlappingPlayer;

    // Start is called before the first frame update
    void Start()
    {
        _overlappingPlayer = null;

        foreach (InteractionLock interactionLock in _interactionLocks)
        {
            interactionLock.SetOwner(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Add all owned interactions to available interactions list under PlayerInteractionHandler
        PlayerInteractionHandler playerInteractionHandler = other.GetComponent<PlayerInteractionHandler>();

        if (playerInteractionHandler)
        {
            _overlappingPlayer = playerInteractionHandler;
            foreach(InteractionLock interactionLock in _interactionLocks)
            {
                foreach (InteractionDefinition interaction in interactionLock._interactions)
                {
                    playerInteractionHandler.AddAvailableInteraction(interaction);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Add all owned interactions to available interactions list under PlayerInteractionHandler
        PlayerInteractionHandler playerInteractionHandler = other.GetComponent<PlayerInteractionHandler>();

        if (playerInteractionHandler)
        {
            _overlappingPlayer = null;
            foreach (InteractionLock interactionLock in _interactionLocks)
            {
                foreach (InteractionDefinition interaction in interactionLock._interactions)
                {
                    playerInteractionHandler.RemoveAvailableInteraction(interaction);
                }
            }
        }
    }

    void SetInteractionLocksUsable(bool usable)
    {
        foreach (InteractionLock interactionLock in _interactionLocks)
        {
            interactionLock.SetIsUsable(usable);
        }
    }

    public PlayerInteractionHandler GetOverlappingPlayer()
    {
        return _overlappingPlayer;
    }
}
