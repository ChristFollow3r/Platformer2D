using UnityEngine;
using System;

namespace Shared
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        private int currentHealth;
        
        public event Action<float> OnHealthChanged;
        public event Action OnDeath;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void OnEnable()
        {
            Enemies.SkeletonAI.OnPlayerHit += TakeDamage; // Fucking cool
        }

        private void OnDisable()
        {
            Enemies.SkeletonAI.OnPlayerHit -= TakeDamage;
        }

        private void TakeDamage(int damage, int direction)
        {
            currentHealth -= damage;
            float healthPercentage = (float)currentHealth / this.maxHealth;
            OnHealthChanged?.Invoke(healthPercentage); // Call the take hit animation for the player and update UI
            if (currentHealth <= 0) Die();
        }
        private void Die()
        {
            OnDeath?.Invoke(); // Call the death animation
        }
    }
}
