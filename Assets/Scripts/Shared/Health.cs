using UnityEngine;
using System;
using Random = UnityEngine.Random;

namespace Shared
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private GameObject deathDrop;
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

        private void Die()
        {
            OnDeath?.Invoke();
        }

        public void SpawnDeathDrops()
        {
            int randomAmount = Random.Range(2, 6);
            for (int i = 0 ; i < randomAmount; i++)
            {
                var bone = Instantiate(deathDrop, transform.position, transform.rotation);
                if (bone.TryGetComponent(out Rigidbody2D rb))
                {
                    rb.AddForce(new Vector2(Random.Range(-5f, 5f), Random.Range(2f, 7f)), ForceMode2D.Impulse);
                }

                Destroy(bone, 500f);
            }
        }

        public int GetCurrentHealth() => currentHealth;
    }
}
