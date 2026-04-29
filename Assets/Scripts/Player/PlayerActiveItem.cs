using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerActiveItem : MonoBehaviour
    {
        private Animator animator;
        private Rigidbody2D playerRigidBody;
        private static readonly int IsActive  = Animator.StringToHash("isActive");
        private static readonly int IsRunning  = Animator.StringToHash("isRunning");
        private InputSystem_Actions input;

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
            animator.SetBool(IsActive, Mouse.current.leftButton.isPressed);
            animator.SetBool(IsRunning, Mathf.Abs(playerRigidBody.linearVelocityX) > 0.1f);
        }
    }
}
