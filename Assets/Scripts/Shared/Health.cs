using UnityEngine;
using System;
using Enemies.Skeleton;

namespace Shared
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        private int currentHealth;
        
        public event Action<float> OnHealthChanged;
        public event Action OnDeath;

        private void Awake() => currentHealth = maxHealth;

        private void OnEnable() => SkeletonAnimations.OnPlayerHit += TakeDamage;
        private void OnDisable() => SkeletonAnimations.OnPlayerHit -= TakeDamage;

        private void TakeDamage(int damage, int direction, float knockback)
        {
            currentHealth -= damage;
            float healthPercentage = (float)currentHealth / maxHealth;
            OnHealthChanged?.Invoke(healthPercentage); 
            
            if (currentHealth <= 0) Die();
        }

        private void Die() => OnDeath?.Invoke();
    }
}