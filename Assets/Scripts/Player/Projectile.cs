using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 14f;
    public int damage = 1;
    public float lifetime = 2f;
    public LayerMask hitLayers;

    private Vector2 direction = Vector2.right;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) dir = Vector2.right;
        direction = dir.normalized;

        // orientación visual (opcional)
        if (Mathf.Abs(direction.x) > 0.01f)
            transform.localScale = new Vector3(Mathf.Sign(direction.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void Update()
    {
        // Movimiento recto, sin gravedad
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (((1 << col.gameObject.layer) & hitLayers) == 0) return;

        Enemy e = col.GetComponent<Enemy>();
        if (e != null) e.TakeDamage(damage);

        Destroy(gameObject);
    }
}