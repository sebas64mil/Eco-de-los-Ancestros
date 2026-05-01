using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MiniProjectile : MonoBehaviour, IPoolable, IProjectile
{
    private Rigidbody2D rb;
    private ObjectPool pool;

    [Header("Configuración")]
    [SerializeField] private ProjectileDataSO projectileData;

    private float timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public bool AllowBounce => false;

    public float FireCooldown => 0f;

    public bool IsSpecial => false;


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

        timer = projectileData != null ? projectileData.LifeTime : 2f;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            ReturnToPool();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Piso"))
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