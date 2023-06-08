using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionLock : MonoBehaviour
{

    private bool _isUsable;

    private InteractionZone _owner;

    public List<InteractionDefinition> _interactions;

    // Start is called before the first frame update
    void Start()
    {
        SetIsUsable(true);

        foreach (InteractionDefinition interaction in _interactions)
        {
            interaction.SetOwner(this);
        }
    }

    public bool GetIsUsable()
    {
        return _isUsable;
    }
    public InteractionZone GetOwner()
    {
        return _owner;
    }

    public void SetIsUsable(bool usable)
    {
        _isUsable = usable;
    }
    public void SetOwner(InteractionZone newOwner)
    {
        _owner = newOwner;
    }
}
