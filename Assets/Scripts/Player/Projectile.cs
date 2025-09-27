using UnityEngine;

/// <summary>
/// Proyectil simple configurable: daño, velocidad, vida y pierce/radio.
/// </summary>
public class Projectile : MonoBehaviour
{
    public int damage = 4;
    public float speed = 12f;
    public float lifetime = 3f;
    public float radius = 0.15f;
    public LayerMask enemyMask;
    private int _direction = 1;

    public void Init(int damage, int direction, float speed = 12f, float lifetime = 1f)
    {
        this.damage = damage;
        this.speed = speed;
        this.lifetime = lifetime;
        this._direction = direction;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.Translate(Vector2.right * _direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((enemyMask.value & (1 << other.gameObject.layer)) == 0) return;

        if (other.TryGetComponent(out EnemyBase e))
        {
            e.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}