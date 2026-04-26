using System;
using System.Collections;
using UnityEngine;

namespace Platformer
{
    public class Movable_Platform : MonoBehaviour
    {
        [SerializeField] private float platformSpeed;
        private BoxCollider2D platformCollider;
        private void Start()
        {
            platformCollider = GetComponent<BoxCollider2D>();
            StartCoroutine(MovePlatform());
        }

        private IEnumerator MovePlatform()
        {
            float direction = 1;
            while (true)
            {
                transform.position = new Vector2(transform.position.x + (direction * platformSpeed * Time.deltaTime), transform.position.y);
                if (Physics2D.Raycast(transform.position, Vector2.right * direction, 0.7f).collider is not null)
                {
                    yield return new WaitForSeconds(1f);
                    direction *= -1;
                }
                
                yield return null;
            }
        }
    }
}
