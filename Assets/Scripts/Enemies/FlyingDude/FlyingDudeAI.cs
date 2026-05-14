using System;
using System.Collections;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Enemies.FlyingDude
{
    public class FlyingDudeAI : MonoBehaviour
    {
        [SerializeField] private Enemy flyingDudeData;

        private Rigidbody2D          dudeRigidBody;
        private Shared.Health        dudeHealth;
        private Transform            target;
        private FlyingDudeAnimations animations;

        private int                  direction;
        private bool                 isStunned;

        public event Action<int>     OnRange;

        private void Awake()
        {
            dudeRigidBody = GetComponent<Rigidbody2D>();
            dudeHealth    = GetComponent<Shared.Health>();
            animations    = GetComponent<FlyingDudeAnimations>();
            target        = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        private void Start() => isStunned = false;

        private void OnEnable() => dudeHealth.OnKnockbackRecieved += HandleHit;
        private void OnDisable() => dudeHealth.OnKnockbackRecieved -= HandleHit;

        private void Update()
        {
            if (target is null || isStunned) return;

            if (animations.isAttacking)
            {
                dudeRigidBody.linearVelocity = Vector2.zero;
                return;
            }

            float distanceToPlayer = Vector2.Distance(transform.position, target.position);

            if (distanceToPlayer <= flyingDudeData.attackRange)
            {
                OnRange?.Invoke(direction);
            }
            else
            {
                HandleMovement();
            }
        }

        private void HandleMovement()
        {
            direction = target.position.x > transform.position.x ? 1 : -1;

            Vector2 moveDirection = (target.position - transform.position).normalized;
            dudeRigidBody.linearVelocity = moveDirection * flyingDudeData.speed;

            transform.localScale = new Vector3(direction, 1, 1);
        }

        private void HandleHit(int playerDirection, float knockback)
        {
            StartCoroutine(DudeHit(playerDirection, knockback));
        }

        private IEnumerator DudeHit(int playerDirection, float knockback)
        {
            isStunned = true;
            dudeRigidBody.linearVelocity = Vector2.zero;
            dudeRigidBody.AddForce(new Vector2(playerDirection * knockback, 4f), ForceMode2D.Impulse);

            yield return new WaitForSeconds(0.3f);
            isStunned = false;
        }
    }
}
