using UnityEngine;
using System;
using Data;
using Items;
using Random = UnityEngine.Random;

namespace Shared
{
    [Serializable]
    public struct DropItem
    {
        public GameObject prefab;
        public ItemData itemData;
        public int minAmount;
        public int maxAmount;
        [Range(0f, 100f)] public float dropChance;
    }

    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private DropItem[] deathDrops;

        private int currentHealth;
        private bool isDead = false;

        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
        public event Action<int, float> OnKnockbackRecieved;

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
            foreach (var dropItem in deathDrops)
            {
                if (Random.Range(0f, 100f) <= dropItem.dropChance)
                {
                    int randomAmount = Random.Range(dropItem.minAmount, dropItem.maxAmount + 1);

                    for (int i = 0; i < randomAmount; i++)
                    {
                        var drop = Instantiate(dropItem.prefab, transform.position, transform.rotation);

                        if (drop.TryGetComponent(out DropComponent dropComp) && dropItem.itemData != null)
                        {
                            dropComp.SetItem(dropItem.itemData);
                        }

                        if (drop.TryGetComponent(out Rigidbody2D rb))
                        {
                            rb.AddForce(new Vector2(Random.Range(-3f, 3f), Random.Range(2f, 4f)), ForceMode2D.Impulse);
                        }

                        Destroy(drop, 180f);
                    }
                }
            }
        }

        public int GetCurrentHealth() => currentHealth;
    }
}
