using System;
using data;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class DropBehavior : MonoBehaviour
{
    private PolygonCollider2D collisionCollider;
    private readonly float pickUpRadius = 0.2f;
    private readonly float speed = 10f;

    private void Start()
    {
        collisionCollider = GetComponent<PolygonCollider2D>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        collisionCollider.enabled = false;
        transform.position = Vector2.MoveTowards(
            transform.position, other.transform.position, speed * Time.deltaTime);
        // var playerInventory = other.GetComponent<Inventory>();
        
        if (Vector2.Distance(transform.position, other.transform.position) <= pickUpRadius)
            Destroy(gameObject);
        
    }
    
    
    
}
