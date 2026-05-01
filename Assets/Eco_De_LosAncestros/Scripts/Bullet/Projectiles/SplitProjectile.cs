using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SplitProjectile : MonoBehaviour, IPoolable, IProjectile
{
    private Rigidbody2D rb;
    private ObjectPool pool;

    [Header("Configuración")]
    [SerializeField] private ProjectileDataSO projectileData;

    [Header("Split")]
    [SerializeField] private GameObject miniProjectilePrefab;
    [SerializeField] private int poolSize = 10;

    [SerializeField] private int splitCount = 3;
    [SerializeField] private float spreadAngle = 30f;
    [SerializeField] private float splitForce = 6f;

    private static ObjectPool smallProjectilePool;

    private float timer;
    private bool hasUsedSpecial;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (smallProjectilePool == null)
        {
            Transform container = DataContainersPool.MiniProjectilesContainer;

            if (container == null)
            {
                Debug.LogError("MiniProjectilesContainer no asignado en DataContainersPool");
                return;
            }

            smallProjectilePool = new ObjectPool(
                miniProjectilePrefab,
                poolSize,
                container
            );
        }
    }

    public bool AllowBounce => false;

    public float FireCooldown => projectileData != null ? projectileData.FireCooldown : 0.5f;

    public bool IsSpecial => true;

    public void SetPool(ObjectPool pool)
    {
        this.pool = pool;
    }

    public void Launch(Vector2 position, Vector2 direction, float force, bool allowBounce)
    {
        transform.position = position;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.AddForce(direction * force, ForceMode2D.Impulse);

        timer = projectileData != null ? projectileData.LifeTime : 3f;
        hasUsedSpecial = false;

        InputHandler.OnSpecial += ActivateSplit;
    }

    private void ActivateSplit()
    {
        if (hasUsedSpecial) return;

        hasUsedSpecial = true;

        Split();
        ReturnToPool();
    }

    private void Split()
    {
        float directionX = Mathf.Sign(rb.linearVelocity.x);

        if (Mathf.Abs(directionX) < 0.01f)
            directionX = 1f;

        float startAngle = -spreadAngle * 0.5f;

        for (int i = 0; i < splitCount; i++)
        {
            float angle = startAngle + (spreadAngle / (splitCount - 1)) * i;

            Vector2 baseDir = new Vector2(directionX, 0f);
            Vector2 newDir = Quaternion.Euler(0, 0, angle) * baseDir;

            GameObject obj = smallProjectilePool.Get();

            if (obj.TryGetComponent<IProjectile>(out var proj))
            {
                proj.Launch(transform.position, newDir, splitForce, false);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Piso"))
        {
            hasUsedSpecial = true;
        }
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ReturnToPool();
        }
    }

    private void OnDisable()
    {
        InputHandler.OnSpecial -= ActivateSplit;
    }

    private void ReturnToPool()
    {
        if (pool != null)
        {
            pool.Return(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}