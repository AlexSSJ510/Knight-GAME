using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 5;
    private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} recibió {amount} daño. HP: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Puedes agregar efectos/anim aquí
        Debug.Log($"{gameObject.name} ha muerto.");
        Destroy(gameObject);
    }
}