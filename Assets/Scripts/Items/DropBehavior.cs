using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class DropBehavior : MonoBehaviour
{
    private float pickUpRadius = 0.2f;

    void OnTriggerEnter2D(Collider2D other) // Fix this shit with events? That might be the move ngl
    {
        if (other.CompareTag("Player"))
                transform.position =
                    Vector2.MoveTowards(transform.position, other.transform.position, 10 * Time.deltaTime);
        
        if (Vector2.Distance(transform.position, other.transform.position) < pickUpRadius)
            Destroy(gameObject);

    }
}
