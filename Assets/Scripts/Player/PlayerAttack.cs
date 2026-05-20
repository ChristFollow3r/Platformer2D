using System;
using UnityEngine;

namespace Player
{
    public class PlayerAttack : MonoBehaviour
    {
        private PlayerMovement playerMovement;
        private SpriteRenderer playerSpriteRenderer;

        [Header("Attack Settings")]
        [SerializeField] private int attackDamage;
        [SerializeField] private float knockback;
        [SerializeField] private Vector2 attackOffset = new Vector2(1f, 0f);
        [SerializeField] private Vector2 hitBoxSize = new Vector2(1.5f, 1.5f);
        [SerializeField] private LayerMask enemyLayer;

        void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (playerMovement != null) playerMovement.OnAttackPerformed += SlimeAttack;
        }

        private void OnDisable()
        {
            if (playerMovement != null) playerMovement.OnAttackPerformed -= SlimeAttack;
        }

        private void SlimeAttack(Vector2 mousePosition)
        {
            int direction = mousePosition.x < transform.position.x ? -1 : 1;

            Vector2 finalAttackPosition = (Vector2)transform.position + new Vector2(attackOffset.x * direction, attackOffset.y);
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(finalAttackPosition, hitBoxSize, 0f, enemyLayer);

            bool hitAnyEnemy = false;

            foreach (Collider2D hit in hitEnemies)
            {
                if (hit.TryGetComponent(out Shared.Health enemyHealth))
                {
                    enemyHealth.TakeDamage(attackDamage, direction, knockback);
                    hitAnyEnemy = true;
                }
            }

            if (hitAnyEnemy)
            {
                StartCoroutine(HitStop());
            }
        }

        private System.Collections.IEnumerator HitStop()
        {
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(0.05f);
            Time.timeScale = 1f;
        }

        private void OnDrawGizmosSelected()
        {
            if (playerSpriteRenderer == null) playerSpriteRenderer = GetComponent<SpriteRenderer>();
            int direction = (playerSpriteRenderer != null && playerSpriteRenderer.flipX) ? -1 : 1;

            Vector2 finalAttackPosition = (Vector2)transform.position + new Vector2(attackOffset.x * direction, attackOffset.y);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(finalAttackPosition, hitBoxSize);
        }
    }
}
