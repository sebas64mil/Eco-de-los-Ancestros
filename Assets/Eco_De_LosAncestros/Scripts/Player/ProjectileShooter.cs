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

    [Header("Disparo")]
    [SerializeField] private float fireCooldown = 0.5f;

    private float lastShootTime;
    private ObjectPool pool;

    private void Awake()
    {
        pool = new ObjectPool(projectilePrefab, poolSize, poolContainer);
    }

    public void Shoot()
    {
        if (Time.time < lastShootTime + fireCooldown)
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

    private bool GetProjectileBounce()
    {
        if (projectilePrefab.TryGetComponent<BasicProjectile>(out var projectile))
        {
            return projectile.AllowBounce;
        }

        return false;
    }
}