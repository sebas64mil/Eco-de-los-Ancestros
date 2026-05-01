using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private DataPlayer dataPlayer;

    [Header("Rotación")]
    [SerializeField] private float rotationSpeed = 50f;

    [SerializeField] private ProjectileShooter shooter;

    private float angle;
    private float rotateInput;
    private float strengthInput;

    private void Awake()
    {
        if (inputHandler == null)
            inputHandler = GetComponent<InputHandler>();
    }

    private void Start()
    {
        angle = transform.eulerAngles.z;
    }

    private void Update()
    {
        Rotate();
        HandleStrengthContinuous();
    }

    private void OnEnable()
    {
        if (inputHandler == null)
        {
            Debug.LogWarning("InputHandler no asignado en PlayerController.");
            return;
        }

        inputHandler.OnRotate += HandleRotate;
        inputHandler.OnStrength += HandleStrength;
        inputHandler.OnFire += HandleFire;
    }

    private void OnDisable()
    {
        if (inputHandler == null) return;

        inputHandler.OnRotate -= HandleRotate;
        inputHandler.OnStrength -= HandleStrength;
        inputHandler.OnFire -= HandleFire;
    }

    private void HandleRotate(float value)
    {
        rotateInput = value;
    }

    private void HandleStrength(float value)
    {
        strengthInput = value;
    }

    private void HandleFire()
    {
        shooter.Shoot();
    }

    private void Rotate()
    {
        if (Mathf.Abs(rotateInput) < 0.01f) return;

        angle += rotateInput * rotationSpeed * Time.deltaTime;

        angle = Mathf.Clamp(angle, dataPlayer.minRotation, dataPlayer.maxRotation);

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void HandleStrengthContinuous()
    {
        if (Mathf.Abs(strengthInput) < 0.01f) return;

        float previousStrength = dataPlayer.currentStrength;

        dataPlayer.currentStrength += strengthInput * dataPlayer.strengthSpeed * Time.deltaTime;

        dataPlayer.currentStrength = Mathf.Clamp(
            dataPlayer.currentStrength,
            dataPlayer.minStrength,
            dataPlayer.maxStrength
        );

        if (Mathf.Abs(previousStrength - dataPlayer.currentStrength) > 0.01f)
        {
            Debug.Log("Fuerza actual: " + dataPlayer.currentStrength);
        }
    }
}