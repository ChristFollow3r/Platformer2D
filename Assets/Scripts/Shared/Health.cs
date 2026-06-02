using UnityEngine;
using System;
using System.Collections;
using Data;
using Items;
using Random = UnityEngine.Random;
using Player;
using Items.Overlays;

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
        [SerializeField] public int maxHealth;
        [SerializeField] private DropItem[] deathDrops;
        [SerializeField] private GameObject hitVFXPrefab;

        public int currentHealth;
        private int cachedHealth;
        private bool isDead = false;

        // Timer variable to track when the player was last hit
        private float lastDamageTime = -9999f;

        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
        public event Action<int, float> OnKnockbackRecieved;

        private void Awake() => currentHealth = maxHealth;

        private void Start()
        {
            // Start the background healing loop immediately if this is the player
            if (gameObject.CompareTag("Player"))
            {
                StartCoroutine(HealOverTime());
            }
        }

        public void SetHealth(int health)
        {
            currentHealth = health;
            OnHealthChanged?.Invoke(currentHealth);
            if (gameObject.CompareTag("Player")) UIController.Singleton.UpdateHealth(currentHealth, maxHealth);
        }

        public void TakeDamage(int damage, int direction, float knockback)
        {
            if (isDead) return;

            if (gameObject.CompareTag("Player"))
            {
                damage -= Equipment.Singleton.GetDefence();
                cachedHealth = currentHealth;

                lastDamageTime = Time.time;
            }

            currentHealth -= damage;
            OnKnockbackRecieved?.Invoke(direction, knockback);

            float healthPercentage = (float)currentHealth / maxHealth;
            OnHealthChanged?.Invoke(healthPercentage);

            if (gameObject.CompareTag("Player")) UIController.Singleton.UpdateHealth(currentHealth, maxHealth);

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

        public void SpawnHitDrops()
        {
            if (Random.Range(0f, 100f) <= 25f)
            {
                if (hitVFXPrefab != null)
                {
                    var drop = Instantiate(hitVFXPrefab, transform.position, Quaternion.identity);
                    Destroy(drop, 180f);

                    if (drop.TryGetComponent(out Rigidbody2D rb))
                    {
                        float randomX = Random.Range(-0.5f, 0.5f);
                        float randomY = Random.Range(-0.5f, 0.5f);
                        rb.AddForce(new Vector2(randomX, randomY), ForceMode2D.Impulse);
                    }
                }
            }
        }

        private IEnumerator HealOverTime()
        {
            while (true)
            {
                if (gameObject.CompareTag("Player") && !isDead && currentHealth / (float)maxHealth < 0.8f)
                {
                    if (Time.time >= lastDamageTime + 5f)
                    {
                        currentHealth += 10;

                        if (currentHealth > maxHealth)
                        {
                            currentHealth = maxHealth;
                        }

                        float healthPercentage = (float)currentHealth / maxHealth;
                        OnHealthChanged?.Invoke(healthPercentage);
                        UIController.Singleton.UpdateHealth(currentHealth, maxHealth);
                        Debug.Log("Healed");
                    }
                }

                yield return new WaitForSeconds(3f);
            }
        }

        public int GetCurrentHealth() => currentHealth;

        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isDead = false;
        }
    }
}
