using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerActiveItem : MonoBehaviour
    {
        private static AnimationClip _swingClip;
        
        private Rigidbody2D playerRigidbody;
        private Animator animator;
        private SpriteRenderer toolSprite; // To do: Add listener method that gets the item in the hotbar to change the sprite
        private Color activeColor;
        
        private static readonly int Active = Animator.StringToHash("isActive");
        private float swingClipDuration;
        
        private int playerDirection;
        private int facingDirection;
        
        private InputSystem_Actions input;
        private bool isAttacking;
    
        private void Awake()
        {
            input             = new InputSystem_Actions();
            input.Enable();
            
            playerRigidbody   = transform.parent.GetComponent<Rigidbody2D>();
            animator          = GetComponent<Animator>();
            _swingClip        = animator.runtimeAnimatorController.animationClips[0];
            swingClipDuration = _swingClip.length;

            toolSprite        = transform.GetChild(0).GetComponent<SpriteRenderer>();
            activeColor       = toolSprite.color;
        }

        private void Start()
        {
            isAttacking        = false;
            activeColor.a      = 0;
            toolSprite.color   = activeColor;
        }

        private void Update()
        {
            UpdateFacingDirection();
            PlayAnimation();
        }

        private void PlayAnimation()
        {
            if (!Mouse.current.leftButton.isPressed || isAttacking) return;
            isAttacking           = true;
            StartCoroutine(Attack());

        }
        
        private IEnumerator Attack()
        {
            activeColor.a       = 1;
            toolSprite.color    = activeColor;
            playerDirection = facingDirection;
            Vector2 attackPoint = (Vector2)transform.position + new Vector2(playerDirection * 0.2f, 0f);
            
            animator.SetBool(Active, true);
            Collider2D hit = Physics2D.OverlapCircle(attackPoint, 1f, 5);
            if (hit is not null)
            {
                if (hit.TryGetComponent(out Shared.Health enemyHealth))
                {
                    enemyHealth.TakeDamage(10, playerDirection, 10);
                }
            }
            yield return new WaitForSeconds(swingClipDuration);
            
            activeColor.a       = 0;
            toolSprite.color    = activeColor;
            
            animator.SetBool(Active, false);
            isAttacking         = false;
            yield return null;
        }

        private void UpdateFacingDirection()
        {
            Vector2 moveInput = input.Player.Move.ReadValue<Vector2>();
            
            if (moveInput.x > 0.1f) facingDirection = 1;
            else if (moveInput.x < -0.1f) facingDirection = -1;
        }
    }
}
