using System;
using UnityEngine;

namespace Platformer
{
    public class Platform_Parent : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Platform"))
            {
                transform.parent = other.transform;
            }
            
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            transform.parent = null;
        }
    }
}
