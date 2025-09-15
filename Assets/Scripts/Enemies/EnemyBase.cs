using UnityEngine;

/// <summary>
/// Base abstracta para todos los enemigos.
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    public int maxHealth = 3;
    protected int _currentHealth;

    public virtual void Awake()
    {
        _currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}