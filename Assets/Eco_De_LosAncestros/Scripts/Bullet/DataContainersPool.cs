using UnityEngine;

public class DataContainersPool : MonoBehaviour
{
    public static DataContainersPool Instance { get; private set; }

    [Header("Containers")]
    [SerializeField] private Transform miniProjectilesContainer;

    public static Transform MiniProjectilesContainer => Instance.miniProjectilesContainer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}