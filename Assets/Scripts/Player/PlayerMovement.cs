using UnityEngine;
using UnityEngine.InputSystem;

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
        if (rb.linearVelocityY < 0) rb.gravityScale = 5f;
        else rb.gravityScale = 3f;
        Movemenet();
    }

    private void Movemenet()
    {
        Vector2 movement = playerInput.Player.Move.ReadValue<Vector2>();
        rb.linearVelocityX = movement.x * speed;

        Vector2 origin = new Vector3(transform.position.x, playerCollider.bounds.min.y); // Dude still gets stuck on the wall
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, 0.2f);
        if (playerInput.Player.Jump.WasPerformedThisFrame() && hit.collider != null) rb.linearVelocityY = jumpForce;
    }
   
}
