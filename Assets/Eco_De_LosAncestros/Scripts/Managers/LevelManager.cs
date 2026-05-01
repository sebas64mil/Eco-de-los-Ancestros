using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private InputActionReference pauseAction;

    private bool isPaused = false;

    private void OnEnable()
    {
        // ----------------- Enable pause input action and subscribe to input event ------------------
        pauseAction.action.Enable();
        pauseAction.action.performed += OnPausePressed;
    }

    private void OnDisable()
    {
        // ----------------- Unsubscribe from pause input event and disable input action ------------------
        pauseAction.action.performed -= OnPausePressed;
        pauseAction.action.Disable();
    }

    private void Start()
    {
        // ----------------- Initialize level state and disable pause menu ------------------
        GameManager.CursorVisible(false);
        pauseMenu.SetActive(false);
        GameManager.Instance.GamePause(false);
    }

    private void OnPausePressed(InputAction.CallbackContext context)
    {
        // ----------------- Toggle pause state when pause input is pressed ------------------
        isPaused = !isPaused;
        PauseMenuVisible(isPaused);
    }

    public void PauseMenuVisible(bool state)
    {
        // ----------------- Show or hide pause menu and update game pause state ------------------
        pauseMenu.SetActive(state);
        GameManager.Instance.GamePause(state);
    }
}
