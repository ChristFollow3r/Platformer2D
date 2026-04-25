using System;
using UnityEngine;
using UnityEngine.Events;

namespace Platformer
{
    public class CheckPoint : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        [SerializeField] private Rigidbody2D rigidbody;
        
        private Vector2 playerSavedPosition;
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                playerSavedPosition = transform.position;
            }
        }
        
        public void Respawn()
        {
            if (rigidbody is not null)
            {
                rigidbody.linearVelocity = Vector2.zero;
                rigidbody.angularVelocity = 0;
            }
            player.transform.position = playerSavedPosition;
        }
    }
}
