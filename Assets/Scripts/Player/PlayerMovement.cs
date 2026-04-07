using UnityEngine;

namespace Player // Rider yellow underlying was telling me to use a namespace
{
    public class PlayerMovement : MonoBehaviour
    {
        private Rigidbody2D rb;
        private Collider2D playerCollider;
        private InputSystem_Actions playerInput;

        [SerializeField] private float speed;
        [SerializeField] private float jumpForce;
        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();   
            playerCollider = GetComponent<Collider2D>();
            playerInput = new InputSystem_Actions();
            playerInput.Enable();
        }
        private void Update()
        {
            rb.gravityScale = rb.linearVelocityY < 0 ? 5f : 3f;
            Movement();
        }

        private void Movement()
        {
            var movement = playerInput.Player.Move.ReadValue<Vector2>();
            rb.linearVelocityX = movement.x * speed;

            Vector2 origin = new Vector3(transform.position.x, playerCollider.bounds.min.y); // Dude still gets stuck on the wall
            var hit = Physics2D.Raycast(origin, Vector2.down, 0.2f);
            if (playerInput.Player.Jump.WasPerformedThisFrame() && hit.collider is not null) rb.linearVelocityY = jumpForce; // I uninstalled visual community and installed
            // rider cause community was working so bad, and I'm done with that IDE. Rider warns me about bad performance code, that's why I wrote is not null instead of != null.
        }
   
    }
}

