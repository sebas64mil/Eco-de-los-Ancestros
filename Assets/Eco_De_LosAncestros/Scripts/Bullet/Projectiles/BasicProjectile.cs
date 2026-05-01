using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BasicProjectile : MonoBehaviour, IPoolable, IProjectile
{
    private Rigidbody2D rb;
    private ObjectPool pool;

    [Header("Vida")]
    [SerializeField] private float lifeTime = 3f;

    [Header("Comportamiento")]
    [SerializeField] private bool allowBounce = true;

    public bool AllowBounce => allowBounce; 

    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetPool(ObjectPool pool)
    {
        this.pool = pool;
    }

    public void Launch(Vector2 position, Vector2 direction, float force, bool allowBounce)
    {
        this.allowBounce = allowBounce;

        transform.position = position;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        rb.AddForce(direction * force, ForceMode2D.Impulse);

        timer = lifeTime;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ReturnToPool();
        }
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