using UnityEngine;
using UnityEngine.Events;

namespace Platformer
{
    public class DeathZone : MonoBehaviour
    {
        public UnityEvent onHit;
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                onHit.Invoke();
            }
        }
    }
}
