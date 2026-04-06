using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CapsuleCollider2D playerCollider;
    [SerializeField] private float gravityForce;
    private Vector3 lastPosition;
    private float verticalVelocity = 0f;
    void Update()
    {
        lastPosition = playerCollider.transform.position;
        bool grounded = IsGrounded();
        ApplyGravity(grounded);
        PlayerCollisions(grounded);
        Debug.Log(grounded);
    }
    void ApplyGravity(bool grounded)
    {
        if (!grounded) verticalVelocity += gravityForce * Time.deltaTime; // If the raycast doesn't hit it should fall...
        else verticalVelocity = 0f;
        transform.position += new Vector3(0, verticalVelocity, 0);
    }
    void PlayerCollisions(bool grounded) 
    {
        if (grounded) transform.position = lastPosition;
    }
    bool IsGrounded()
    {
        if (Physics2D.Raycast(playerCollider.transform.position, Vector2.down, 0.2f)) return true; // FUCK THIS SHIT. Hardcoded value cause I have no clue where there raycast is starting from
        else return false;
    }
}
