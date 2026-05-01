using UnityEngine;

public class ProjectileShooter : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private DataPlayer dataPlayer;

    [Header("Pool")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private Transform poolContainer;

    private float lastShootTime;
    private ObjectPool pool;

    private void Awake()
    {
        pool = new ObjectPool(projectilePrefab, poolSize, poolContainer);
    }

    public void Shoot()
    {
        float prefabCooldown = GetProjectilePrefabCooldown();

        if (Time.time < lastShootTime + prefabCooldown)
            return;

        lastShootTime = Time.time;

        GameObject obj = pool.Get();

        if (obj.TryGetComponent<IProjectile>(out var projectile))
        {
            Vector2 direction = firePoint.up;
            float force = dataPlayer.currentStrength;

            bool allowBounce = GetProjectileBounce();

            projectile.Launch(
                firePoint.position,
                direction,
                force,
                allowBounce
            );
        }
    }

    private float GetProjectilePrefabCooldown()
    {
        if (projectilePrefab == null) return 0.5f;

        if (projectilePrefab.TryGetComponent<BasicProjectile>(out var basic))
            return basic.FireCooldown;

        if (projectilePrefab.TryGetComponent<EmbeddedProjectile>(out var embedded))
            return embedded.FireCooldown;

        return 0.5f;
    }

    private bool GetProjectileBounce()
    {
        if (projectilePrefab.TryGetComponent<IProjectile>(out var projectile))
        {
            return projectile.AllowBounce;
        }

        return false;
    }
}