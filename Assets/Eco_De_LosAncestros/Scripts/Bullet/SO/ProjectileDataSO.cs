using UnityEngine;

[CreateAssetMenu(menuName = "Game/ProjectileData", fileName = "ProjectileData")]
public class ProjectileDataSO : ScriptableObject
{
    [Header("Vida")]
    [SerializeField] private float lifeTime = 3f;

    [Header("Disparo")]
    [SerializeField] private float fireCooldown = 0.5f;

    public float LifeTime => lifeTime;
    public float FireCooldown => fireCooldown;
}