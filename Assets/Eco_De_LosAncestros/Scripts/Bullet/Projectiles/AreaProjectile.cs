using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AreaProjectile : MonoBehaviour, IPoolable, IProjectile
{
    private Rigidbody2D rb;
    private ObjectPool pool;

    [Header("Configuración")]
    [SerializeField] private ProjectileDataSO projectileData;

    [Header("Área")]
    [SerializeField] private float areaRadius = 2f;
    [SerializeField] private float specialDuration = 1f;
    [SerializeField] private LayerMask hitMask;

    private float timer;
    private bool hasUsedSpecial;
    private bool isAreaActive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        isAreaActive = false;

        InputHandler.OnSpecial += ActivateArea;
    }

    private void ActivateArea()
    {
        if (hasUsedSpecial) return;

        hasUsedSpecial = true;
        isAreaActive = true;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        timer = specialDuration;
    }

    private void Update()
    {
        if (isAreaActive)
        {
            DetectArea();
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ReturnToPool();
        }
    }

    private void DetectArea()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaRadius, hitMask);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                // aplicar daño o efecto
            }
        }
    }

    private void OnDisable()
    {
        InputHandler.OnSpecial -= ActivateArea;
    }

    private void ReturnToPool()
    {
        isAreaActive = false;

        if (pool != null)
        {
            pool.Return(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Piso"))
        {
            ReturnToPool();
        }

    }


    private void OnDrawGizmos()
    {
        if (!isAreaActive) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaRadius);
    }
}