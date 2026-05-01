using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public event Action<float> OnRotate;
    public event Action<float> OnStrength;
    public event Action OnFire;
    public static event Action OnSpecial;
    public void OnRotateInput(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        OnRotate?.Invoke(value);
    }

    public void OnStrengthInput(InputAction.CallbackContext context)
    {
        float value = context.ReadValue<float>();
        OnStrength?.Invoke(value);
    }

    public void OnFireInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnFire?.Invoke();
        }
    }

    public void OnSpecialInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnSpecial?.Invoke();
        }
    }


}