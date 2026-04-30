using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerActiveItem : MonoBehaviour
    {
        private static AnimationClip _swingClip;
        
        private Rigidbody2D          playerRigidbody;
        private Animator             animator;
        private SpriteRenderer       toolSprite; // To do: Add listener method that gets the item in the hotbar to change the sprite
        private Color                activeColor;
        
        private static readonly int  Active = Animator.StringToHash("isActive");
        private float                swingClipDuration;
        private float                playerDirection;
        
        private InputSystem_Actions  input;
        private bool                 isAttacking;

        public event Action<int>     OnEnemyHit;
    
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
            playerDirection     = playerRigidbody.linearVelocityX >= 0 ? 1f : -1f;
            Vector2 attackPoint = (Vector2)transform.position + new Vector2(playerDirection * 0.2f, 0f);
            
            animator.SetBool(Active, true);
            Collider2D hit = Physics2D.OverlapCircle(attackPoint, 1f, 4);
            if (hit is not null) OnEnemyHit?.Invoke(10); // Use here the get current item method to know the dmg to be applied
            yield return new WaitForSeconds(swingClipDuration);
            
            activeColor.a       = 0;
            toolSprite.color    = activeColor;
            
            animator.SetBool(Active, false);
            isAttacking         = false;
            yield return null;
        }
    }
}
