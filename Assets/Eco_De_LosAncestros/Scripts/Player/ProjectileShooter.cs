using UnityEngine;
using System;

public class ProjectileShooter : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private DataPlayer dataPlayer;

    public static event Action OnSpecialFired;

    private float lastShootTime;
    private ObjectPool pool;
    private GameObject currentPrefab;
    private bool allowImmediateShot;

    public void Initialize(GameObject prefab, ObjectPool newPool)
    {
        currentPrefab = prefab;
        pool = newPool;

        allowImmediateShot = true;
    }

    public void Shoot()
    {
        if (pool == null || currentPrefab == null) return;

        float cooldown = GetCooldown();

        if (!allowImmediateShot)
        {
            if (Time.time < lastShootTime + cooldown)
                return;
        }
        else
        {
            allowImmediateShot = false;
        }

        lastShootTime = Time.time;

        GameObject obj = pool.Get();

        if (obj.TryGetComponent<IProjectile>(out var projectile))
        {
            projectile.Launch(
                firePoint.position,
                firePoint.up,
                dataPlayer.currentStrength,
                projectile.AllowBounce
            );

            if (projectile.IsSpecial)
            {
                OnSpecialFired?.Invoke();
            }
        }
    }


    private float GetCooldown()
    {
        if (currentPrefab.TryGetComponent<IProjectile>(out var proj))
            return proj.FireCooldown;

        return 0.5f;
    }
}