using UnityEngine;

[CreateAssetMenu(fileName = "DataPlayer", menuName = "Game/DataPlayer")]
public class DataPlayer : ScriptableObject
{
    [Header("Fuerza")]
    public float currentStrength = 0f;
    public float minStrength = 0f;
    public float maxStrength = 20f;
    public float strengthSpeed = 10f;

    [Header("Rotación")]
    public float minRotation = -90f;
    public float maxRotation = 90f;
}