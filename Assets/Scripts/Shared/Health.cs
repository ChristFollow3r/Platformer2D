using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace Shared
{
    // Struct to hold all data related to a specific drop
    [Serializable]
    public struct DropItem
    {
        public GameObject prefab;
        public int minAmount;
        public int maxAmount;
        [Range(0f, 100f)] public float dropChance; // Percentage chance to drop
    }

    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth;

        // Replaced the single GameObject with an array of our new struct
        [SerializeField] private DropItem[] deathDrops;

        private int currentHealth;

        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
        public event Action<int, float> OnKnockbackRecieved;

        private bool isDead = false;

        private void Awake() => currentHealth = maxHealth;

        public void TakeDamage(int damage, int direction, float knockback)
        {
            if (isDead) return;

            currentHealth -= damage;
            OnKnockbackRecieved?.Invoke(direction, knockback);

            float healthPercentage = (float)currentHealth / maxHealth;
            OnHealthChanged?.Invoke(healthPercentage);

            if (currentHealth <= 0)
            {
                isDead = true;
                Die();
            }
        }

        private void Die()
        {
            OnDeath?.Invoke();
        }

        public void SpawnDeathDrops()
        {
            // Iterate through every possible item in your drop table
            foreach (var dropItem in deathDrops)
            {
                // Roll the dice to see if this specific item should drop
                if (Random.Range(0f, 100f) <= dropItem.dropChance)
                {
                    // Calculate how many to spawn. (+1 because int Random.Range is exclusive at the max bound)
                    int randomAmount = Random.Range(dropItem.minAmount, dropItem.maxAmount + 1);

                    for (int i = 0; i < randomAmount; i++)
                    {
                        var drop = Instantiate(dropItem.prefab, transform.position, transform.rotation);

                        if (drop.TryGetComponent(out Rigidbody2D rb))
                        {
                            // Retained your physics-based movement so the drops scatter nicely
                            rb.AddForce(new Vector2(Random.Range(-5f, 5f), Random.Range(2f, 7f)), ForceMode2D.Impulse);
                        }

                        Destroy(drop, 180f);
                    }
                }
            }
        }

        public int GetCurrentHealth() => currentHealth;
    }
}
