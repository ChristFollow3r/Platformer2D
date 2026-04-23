using Player;
using Prefabs;
using UnityEngine;

namespace Items // Maybe I should add this to items?
{
    public class DropBehavior : MonoBehaviour
    {
        private PolygonCollider2D collisionCollider;
        private readonly float pickUpRadius = 0.4f;
        private readonly float speed = 10f;

        private void Start()
        {
            collisionCollider = GetComponent<PolygonCollider2D>();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
        
            var playerManager = other.GetComponent<PlayerManager>();
            collisionCollider.enabled = false;
            transform.position = Vector2.MoveTowards(
                transform.position, other.transform.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, other.transform.position) <= pickUpRadius)
            {
                var item = GetComponent<ItemReference>().GetItem();
                playerManager.Inventory.AddItemToHotbar(item, 1);
                Destroy(gameObject);
            }
            
        
        }
    
    
    
    }
}
