using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private InputSystem_Actions playerInput;
    [SerializeField] private float speed;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();   
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
    }
   
}
