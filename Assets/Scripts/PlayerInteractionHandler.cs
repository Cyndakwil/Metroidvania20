using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InteractionStage
{
    inactive,
    enter,
    update,
    exit
}

public class PlayerInteractionHandler : MonoBehaviour
{
    private List<InteractionDefinition> _availableInteractions;
    private List<InteractionDefinition> _possibleInteractions;

    private InteractionDefinition _currentInteraction;

    private float _tickTime;

    private delegate void InteractionStageMethod(PlayerInteractionHandler player);
    InteractionStageMethod stageTickMethod = null;
    InteractionStageMethod stageEndMethod = null;

    private InteractionStage _interactionStage;

    // Start is called before the first frame update
    void Start()
    {
        _availableInteractions = new List<InteractionDefinition>();
        _possibleInteractions = new List<InteractionDefinition>();
        _currentInteraction = null;
        _tickTime = 0.0f;
        _interactionStage = InteractionStage.inactive;
    }
    private void Update()
    {
        foreach(InteractionDefinition interaction in _availableInteractions)
        {
            if (interaction.IsInteractionPossible(this) && interaction.GetOwner().GetIsUsable())
            {
                if (!_possibleInteractions.Contains(interaction))
                {
                    _possibleInteractions.Add(interaction);
                }
            }
            else
            {
                _possibleInteractions.Remove(interaction);
            }
        }
    }

    private void FixedUpdate()
    {
        // Tick interaction if needed
        if (_currentInteraction)
        {
            // Set tick method and time
            while (_tickTime <= 0)
            {
                if (stageEndMethod != null)
                {
                    stageEndMethod(this);
                }

                // Move to the next interaction stage
                SetInteractionStage(_interactionStage == InteractionStage.exit ? InteractionStage.inactive : _interactionStage + 1);

                // Call start function and set values based on interaction stage
                if (_interactionStage == InteractionStage.enter)
                {
                    // OnInteractionEnterStart was called directly from the interaction in order to be performed instantaneously
                    _tickTime = _currentInteraction._enterLength;
                    stageTickMethod = _currentInteraction.OnInteractionEnterTick;
                    stageEndMethod = _currentInteraction.OnInteractionEnterEnd;
                }
                else if (_interactionStage == InteractionStage.update)
                {
                    _currentInteraction.OnInteractionUpdateStart(this);
                    _tickTime = _currentInteraction._updateLength;
                    stageTickMethod = _currentInteraction.OnInteractionUpdateTick;
                    stageEndMethod = _currentInteraction.OnInteractionUpdateEnd;
                }
                else if (_interactionStage == InteractionStage.exit)
                {
                    _currentInteraction.OnInteractionExitStart(this);
                    _tickTime = _currentInteraction._exitLength;
                    stageTickMethod = _currentInteraction.OnInteractionExitTick;
                    stageEndMethod = _currentInteraction.OnInteractionExitEnd;
                }
                else if (_interactionStage == InteractionStage.inactive)
                {
                    _tickTime = 0.0f;
                    stageTickMethod = null;
                    stageEndMethod = null;
                    _currentInteraction.GetOwner().SetIsUsable(true);
                    SetCurrentInteraction(null);
                    break;
                }
            }

            // Call corresponding tick function and tick down
            if (stageTickMethod != null)
            {
                stageTickMethod(this);
            }
            _tickTime -= Time.fixedDeltaTime;
        }
    }

    public void AddAvailableInteraction(InteractionDefinition interaction)
    {
        _availableInteractions.Add(interaction);
    }

    public void RemoveAvailableInteraction(InteractionDefinition interaction)
    {
        _availableInteractions.Remove(interaction);
        _possibleInteractions.Remove(interaction);
    }

    public List<InteractionDefinition> GetPossibleInteractions()
    {
        return _possibleInteractions;
    }

    public InteractionDefinition GetCurrentInteraction()
    {
        return _currentInteraction;
    }

    public void SetCurrentInteraction(InteractionDefinition interaction)
    {
        _currentInteraction = interaction;
    }

    public void SetInteractionStage(InteractionStage stage)
    {
        _interactionStage = stage;
    }
}
