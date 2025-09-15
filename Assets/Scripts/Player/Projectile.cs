using UnityEngine;

public class Projectile : MonoBehaviour
{
    private int damage;
    private int direction;
    public float speed = 8f;
    public LayerMask enemyMask;

    public void Init(int damage, int direction)
    {
        this.damage = damage;
        this.direction = direction;
    }

    private void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out EnemyBase enemy))
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
