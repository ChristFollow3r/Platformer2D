using UnityEngine;

namespace Enemies
{
    public class SkeletonAI : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed;
        [SerializeField] private float jumpForce;

        private Rigidbody2D skeletonRigidBody;
        private CapsuleCollider2D skeletonCollider;
        private Transform target;
        
        private bool isGrounded;
        private bool theresBlockInFront;

        private void Awake()
        {
            skeletonRigidBody = GetComponent<Rigidbody2D>();
            skeletonCollider =  GetComponent<CapsuleCollider2D>();
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void Update()
        {
            if (target is null) return;
            CheckForObstacles();
            HandleMovement();
        }

        private void HandleMovement()
        {
            int direction = target.position.x > transform.position.x ? 1 : -1;
            skeletonRigidBody.linearVelocityX = direction * speed;
            transform.localScale = new Vector3(direction, 1, 1);
            skeletonRigidBody.gravityScale = skeletonRigidBody.linearVelocityY < 0 ? 5f : 3f;
        }

        private void CheckForObstacles()
        {
            int direction = target.position.x > transform.position.x ? 1 : -1;
            
            isGrounded = Physics2D.Raycast(skeletonCollider.bounds.min, Vector2.down, 0.1f).collider is not null;
            theresBlockInFront = Physics2D.Raycast(transform.position, Vector2.right * direction, 0.6f).collider is not null;
            
            if (isGrounded && theresBlockInFront)
            {
                skeletonRigidBody.linearVelocityY = jumpForce;
            }
        }
    }
}
