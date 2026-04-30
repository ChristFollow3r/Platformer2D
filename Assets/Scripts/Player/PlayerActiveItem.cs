using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerActiveItem : MonoBehaviour
    {
        private static AnimationClip _swingClip;
        
        private Animator             animator;
        private SpriteRenderer       toolSprite;
        private Color                activeColor;
        
        private static readonly int  Active = Animator.StringToHash("isActive");
        private float                swingClipDuration;
        
        private InputSystem_Actions  input;
        private bool                 isAttacking;
    
        private void Awake()
        {
            input             = new InputSystem_Actions();
            input.Enable();
            
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
            isAttacking       = true;
            StartCoroutine(Attack());

        }

        private IEnumerator Attack()
        {
            activeColor.a    = 1;
            toolSprite.color = activeColor;
            
            animator.SetBool(Active, true);
            yield return new WaitForSeconds(swingClipDuration);
            
            activeColor.a    = 0;
            toolSprite.color = activeColor;
            
            animator.SetBool(Active, false);
            isAttacking      = false;
            yield return null;
        }
    }
}
