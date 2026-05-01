using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public event Action<float> OnRotate;
    public event Action<float> OnStrength;
    public event Action OnFire;

    public static event Action OnSpecial;

    public static event Action OnEmbeddedProjectile;
    public static event Action OnSplitProjectile;
    public static event Action OnAreaProjectile;
    public static event Action OnCancelSpecial; 

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

    public void OnEmbeddedProjectileInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnEmbeddedProjectile?.Invoke();
        }
    }

    public void OnSplitProjectileInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnSplitProjectile?.Invoke();
        }
    }

    public void OnAreaProjectileInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnAreaProjectile?.Invoke();
        }
    }

    public void OnCancelSpecialInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnCancelSpecial?.Invoke();
        }
    }
}