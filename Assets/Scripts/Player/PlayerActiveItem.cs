using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerActiveItem : MonoBehaviour
    {
        private Animator animator;
        private Rigidbody2D playerRigidBody;
        
        private static readonly int Active  = Animator.StringToHash("isActive");
        private static readonly int Running = Animator.StringToHash("isRunning");
        
        private InputSystem_Actions input;
        private bool isAttacking;

        private void Awake()
        {
            input = new InputSystem_Actions();
            playerRigidBody = transform.parent.GetComponent<Rigidbody2D>();
            input.Enable();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            PlayAnimation();
        }

        private void PlayAnimation()
        {
            animator.SetBool(Active, Mouse.current.leftButton.isPressed);
            animator.SetBool(Running, Mathf.Abs(playerRigidBody.linearVelocityX) > 0.1f);
        }
    }
}
