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
        Debug.Log($"{gameObject.name} recibi� {amount} da�o. HP: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Puedes agregar efectos/anim aqu�
        Debug.Log($"{gameObject.name} ha muerto.");
        Destroy(gameObject);
    }
}