using UnityEngine;

[CreateAssetMenu(menuName = "Game/TrajectorySettings", fileName = "TrajectorySettings")]
public class TrajectorySettingsSO : ScriptableObject
{
    [Header("Trayectoria")]
    public float timeStep = 0.02f;
    public int maxPoints = 100;
    public int maxBounces = 3;

    [Header("Rebote")]
    public float bounceDamping = 0.8f;
    public float minVelocity = 0.5f;
    public float surfaceOffset = 0.01f;
}