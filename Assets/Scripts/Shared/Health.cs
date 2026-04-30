using UnityEngine;
using System;
using Enemies.Skeleton;

namespace Shared
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        private int                  currentHealth;
        
        public event Action<float> OnHealthChanged;
        public event Action OnDeath;

        public event Action<int, float> OnKnockbackRecieved;

        private void Awake() => currentHealth = maxHealth;

        public void TakeDamage(int damage, int direction, float knockback)
        {
            currentHealth -= damage;
            OnKnockbackRecieved?.Invoke(direction, knockback);
            
            float healthPercentage = (float)currentHealth / maxHealth;  
            OnHealthChanged?.Invoke(healthPercentage); 
            
            if (currentHealth <= 0) Die();
        }

        private void Die() => OnDeath?.Invoke();
    }
}