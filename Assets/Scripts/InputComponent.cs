using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputComponent : MonoBehaviour
{
    public InputActionAsset _inputActions;

    void Awake()
    {
        _inputActions.Enable();
        foreach (InputActionMap actionMap in _inputActions.actionMaps)
        {
            foreach (InputAction action in actionMap.actions)
            {
                action.performed += ctx => ActionPerformed(ctx);
            }
        }
    }

    void ActionPerformed(InputAction.CallbackContext ctx)
    {
    }
}
