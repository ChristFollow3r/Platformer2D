using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace Shared
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private GameObject deathDrop;

        [Header("Audio")]
        [SerializeField] private AudioSource hitSound;
        [SerializeField] private AudioSource deathSound;


        private int currentHealth;

        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
        public event Action<int, float> OnKnockbackRecieved;
        private void Awake() => currentHealth = maxHealth;
        private bool isDead = false;

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
            int randomAmount = Random.Range(2, 6);
            for (int i = 0 ; i < randomAmount; i++)
            {
                var drop = Instantiate(deathDrop, transform.position, transform.rotation);
                if (drop.TryGetComponent(out Rigidbody2D rb))
                {
                    rb.AddForce(new Vector2(Random.Range(-5f, 5f), Random.Range(2f, 7f)), ForceMode2D.Impulse);
                }

                Destroy(drop, 180f);
            }
        }

        public int GetCurrentHealth() => currentHealth;
    }
}
