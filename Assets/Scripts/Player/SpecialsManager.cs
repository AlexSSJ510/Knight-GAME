using System.Collections;
using UnityEngine;

/// <summary>
/// Implementa los specials de Zero: Ryuenjin, Raijingeki, Kuuenzan, Hyouretsuzan (spawn), Rakuhouha (onda).
/// Llamar desde UI o PlayerAttack/PlayerController cuando corresponda.
/// </summary>
public class SpecialsManager : MonoBehaviour
{
    [Header("References")]
    public Transform saberPoint;
    public LayerMask enemyMask;
    public GameObject rakuhouhaProjectilePrefab; // onda energ�tica
    public GameObject hyouretsuzanPrefab; // pilar de hielo
    public GameObject kuuenzanEffectPrefab; // vfx de Kuuenzan
    public float specialCooldown = 1f;

    private bool _isOnCooldown = false;

    // RYUENJIN: multi-hit spinning slash that propels forward a little
    public IEnumerator Ryuenjin(float duration = 0.6f, int hitsPerSecond = 8, int damage = 6, float forwardSpeed = 6f)
    {
        if (_isOnCooldown) yield break;
        _isOnCooldown = true;
        float end = Time.time + duration;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        int dir = transform.localScale.x > 0 ? 1 : -1;

        while (Time.time < end)
        {
            // damage around player
            Collider2D[] hits = Physics2D.OverlapCircleAll(saberPoint.position, 0.9f, enemyMask);
            foreach (var h in hits)
            {
                if (h.TryGetComponent(out EnemyBase e))
                {
                    e.TakeDamage(damage);
                }
            }
            // propel player a bit
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(dir * forwardSpeed, rb.linearVelocity.y);
            }
            yield return new WaitForSeconds(1f / hitsPerSecond);
        }

        yield return new WaitForSeconds(specialCooldown);
        _isOnCooldown = false;
    }

    // RAIJINGEKI: powerful lightning slash in front with knockback
    public void Raijingeki(int damage = 25, float knockback = 6f)
    {
        if (_isOnCooldown) return;
        StartCoroutine(RaijingekiRoutine(damage, knockback));
    }

    private IEnumerator RaijingekiRoutine(int damage, float knockback)
    {
        _isOnCooldown = true;
        // VFX: could spawn an effect here
        Collider2D[] hits = Physics2D.OverlapCircleAll(saberPoint.position, 1.2f, enemyMask);
        foreach (var h in hits)
        {
            if (h.TryGetComponent(out EnemyBase e))
            {
                e.TakeDamage(damage);
                if (h.attachedRigidbody != null)
                {
                    Vector2 kb = (h.transform.position - transform.position).normalized;
                    h.attachedRigidbody.AddForce(kb * knockback, ForceMode2D.Impulse);
                }
            }
        }

        yield return new WaitForSeconds(specialCooldown);
        _isOnCooldown = false;
    }

    // KUUENZAN: slash that spawns a vertical disc/whirl that hits enemies (visual)
    public void Kuuenzan(int damage = 18)
    {
        if (_isOnCooldown) return;
        StartCoroutine(KuuenzanRoutine(damage));
    }

    private IEnumerator KuuenzanRoutine(int damage)
    {
        _isOnCooldown = true;
        if (kuuenzanEffectPrefab != null)
        {
            Instantiate(kuuenzanEffectPrefab, saberPoint.position, Quaternion.identity);
        }
        Collider2D[] hits = Physics2D.OverlapCircleAll(saberPoint.position, 1.0f, enemyMask);
        foreach (var h in hits)
        {
            if (h.TryGetComponent(out EnemyBase e))
            {
                e.TakeDamage(damage);
            }
        }
        yield return new WaitForSeconds(specialCooldown);
        _isOnCooldown = false;
    }

    // HYOURETSUZAN: spawn ice pillar at position in front of player
    public void Hyouretsuzan(int damage = 20, float spawnOffset = 1.2f)
    {
        if (_isOnCooldown) return;
        StartCoroutine(HyouretsuzanRoutine(damage, spawnOffset));
    }

    private IEnumerator HyouretsuzanRoutine(int damage, float spawnOffset)
    {
        _isOnCooldown = true;
        int dir = transform.localScale.x > 0 ? 1 : -1;
        Vector3 spawnPos = transform.position + Vector3.right * dir * spawnOffset;
        if (hyouretsuzanPrefab != null)
        {
            GameObject p = Instantiate(hyouretsuzanPrefab, spawnPos, Quaternion.identity);
            var proj = p.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Init(damage, dir, 0f, 2.5f); // pillar could be static but apply damage via trigger in prefab
            }
        }
        else
        {
            // fallback: short-range damage
            Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPos, 0.7f, enemyMask);
            foreach (var h in hits)
            {
                if (h.TryGetComponent(out EnemyBase e)) e.TakeDamage(damage);
            }
        }
        yield return new WaitForSeconds(specialCooldown);
        _isOnCooldown = false;
    }

    // RAKUHOHA: energy wave projectile (large)
    public void Rakuhouha(int damage = 30, float speed = 12f)
    {
        if (_isOnCooldown) return;
        StartCoroutine(RakuhouhaRoutine(damage, speed));
    }

    private IEnumerator RakuhouhaRoutine(int damage, float speed)
    {
        _isOnCooldown = true;
        if (rakuhouhaProjectilePrefab != null)
        {
            int dir = transform.localScale.x > 0 ? 1 : -1;
            GameObject p = Instantiate(rakuhouhaProjectilePrefab, transform.position + Vector3.right * dir * 0.8f, Quaternion.identity);
            var proj = p.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Init(damage, dir, speed, 4f);
                proj.enemyMask = enemyMask;
            }
        }
        yield return new WaitForSeconds(specialCooldown);
        _isOnCooldown = false;
    }
}