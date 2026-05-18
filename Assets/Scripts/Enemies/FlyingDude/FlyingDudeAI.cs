using System;
using System.Collections;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Enemies.FlyingDude
{
    public class FlyingDudeAI : MonoBehaviour
    {
        [SerializeField] private Enemy flyingDudeData;

        [Header("Flight Dynamics")]
        [SerializeField] private float flightAcceleration = 10f;

        [Header("Obstacle Avoidance")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private Vector2   wallCheckSize = new Vector2(0.8f, 0.8f);
        [SerializeField] private float     wallCheckDistance = 0.5f;
        [SerializeField] private float     upwardSwoopStrength = 2f;

        private Rigidbody2D          dudeRigidBody;
        private Shared.Health        dudeHealth;
        private Transform            target;
        private FlyingDudeAnimations animations;
        private SpriteRenderer       spriteRenderer;

        private int                  direction;
        private bool                 isStunned;

        public event Action<int>     OnRange;

        private void Awake()
        {
            dudeRigidBody  = GetComponent<Rigidbody2D>();
            dudeHealth     = GetComponent<Shared.Health>();
            animations     = GetComponent<FlyingDudeAnimations>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            target         = GameObject.FindGameObjectWithTag("Player")?.transform;
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

            direction = target.position.x > transform.position.x ? 1 : -1;
            spriteRenderer.flipX = direction < 0;

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
            Vector2 desiredDirection = (target.position - transform.position).normalized;

            RaycastHit2D hit = Physics2D.BoxCast(transform.position, wallCheckSize, 0f, Vector2.right * direction, wallCheckDistance, obstacleLayer);
            if (hit.collider is not null) desiredDirection = new Vector2(desiredDirection.x * 0.5f, upwardSwoopStrength).normalized;

            Vector2 currentVelocity = dudeRigidBody.linearVelocity;
            currentVelocity += desiredDirection * (flightAcceleration * Time.deltaTime);

            if (currentVelocity.magnitude > flyingDudeData.speed) currentVelocity = currentVelocity.normalized * flyingDudeData.speed;

            dudeRigidBody.linearVelocity = currentVelocity;
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            int debugDir = Application.isPlaying ? direction : 1;
            Vector3 castStart = transform.position;
            Vector3 castEnd = castStart + (Vector3.right * debugDir * wallCheckDistance);
            Gizmos.DrawWireCube(castEnd, wallCheckSize);
        }
    }
}
