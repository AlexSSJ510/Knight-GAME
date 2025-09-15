using UnityEngine;

/// <summary>
/// Modular: gestiona ataques (espada, disparo y especial).
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Espada")]
    public float swordRange = 1.1f;
    public int swordDamage = 2;
    public LayerMask enemyMask;

    [Header("Disparo")]
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public int projectileDamage = 1;
    public float projectileCooldown = 0.23f;

    [Header("Especial/Desfragmentación")]
    public bool canUseSpecial = false;
    public float specialDuration = 2f;

    private bool _isUsingSpecial = false;
    private float _lastShootTime = -10f;

    private void Update()
    {
        if (GameManager.Instance.isPaused) return;

        if (InputManager.Instance.IsAttackPressed()) SwordAttack();

        if (InputManager.Instance.IsShootPressed() && Time.time > _lastShootTime + projectileCooldown)
        {
            Shoot();
            _lastShootTime = Time.time;
        }
    }

    private void SwordAttack()
    {
        // Detectar enemigos en rango con Physics2D.OverlapCircle/Box
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position + transform.right * (transform.localScale.x > 0 ? 1 : -1) * swordRange * 0.5f, swordRange * 0.45f, enemyMask);
        foreach (var h in hits)
        {
            if (h.TryGetComponent(out EnemyBase enemy))
                enemy.TakeDamage(swordDamage);
        }
    }

    private void Shoot()
    {
        Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity).GetComponent<Projectile>().Init(projectileDamage, transform.localScale.x > 0 ? 1 : -1);
    }

    public void EnableSpecial()
    {
        if (!_isUsingSpecial) StartCoroutine(SpecialRoutine());
    }

    private System.Collections.IEnumerator SpecialRoutine()
    {
        _isUsingSpecial = true;
        canUseSpecial = true;
        // TODO: Cambia sprite/efecto visual
        yield return new WaitForSeconds(specialDuration);
        canUseSpecial = false;
        _isUsingSpecial = false;
    }
}