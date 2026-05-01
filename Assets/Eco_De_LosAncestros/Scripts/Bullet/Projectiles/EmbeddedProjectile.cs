using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EmbeddedProjectile : MonoBehaviour, IPoolable, IProjectile
{
    private Rigidbody2D rb;
    private ObjectPool pool;

    [Header("Configuración")]
    [SerializeField] private ProjectileDataSO projectileData;

    [Header("Boost")]
    [SerializeField] private float boostForce = 10f;


    private float timer;
    private bool hasUsedSpecial;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public bool AllowBounce => false;

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

        InputHandler.OnSpecial += ActivateBoost;
    }

    public float FireCooldown => projectileData != null ? projectileData.FireCooldown : 0.5f;

    private void ActivateBoost()
    {
        if (hasUsedSpecial) return;

        hasUsedSpecial = true;

        float directionX = Mathf.Sign(rb.linearVelocity.x);

        if (Mathf.Abs(directionX) < 0.01f)
            directionX = 1f;

        Vector2 direction = new Vector2(directionX, 0f);

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * boostForce, ForceMode2D.Impulse);
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
        InputHandler.OnSpecial -= ActivateBoost;
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