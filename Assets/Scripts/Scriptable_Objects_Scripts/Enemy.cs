using UnityEngine;

namespace Scriptable_Objects_Scripts
{
    [CreateAssetMenu(fileName = "Enemy", menuName = "Scriptable Objects/Enemy")]
    public class Enemy : ScriptableObject
    {
        [Header("Movement")]
        public float speed;
        public float jumpForce;

        [Header("Combat")]
        public int attackDamage;
        public float attackRange;
        public float attackKnockback;
        public Vector2 hitBoxSize;
        public Vector2 attackOffset;
        public float attackCooldown;
        public LayerMask playerLayer;

        [Header("Audio")]
        public AudioClip attackSound;
        public AudioClip moveSound;
        public AudioClip jumpSound;
        public AudioClip hitSound;
        public AudioClip deathSound;
        public AudioClip gruntSound;
        public AudioClip dropSound;
    }
}
