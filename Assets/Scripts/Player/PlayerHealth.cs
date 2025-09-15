using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 16;
    private int _currentHealth;
    public static event Action<int, int> OnHealthChanged; // Actualiza HUD

    private void Awake() { _currentHealth = maxHealth; }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Max(_currentHealth, 0);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (_currentHealth <= 0) Die();
    }

    private void Die() { GameManager.Instance.RestartLevel(); } // Para prototipo
}