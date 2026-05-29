using Data;
using Player;
using UnityEngine;

namespace Items
{
    // Make sure the prefab actually has a SpriteRenderer!
    [RequireComponent(typeof(SpriteRenderer))]
    public class ItemEntity : MonoBehaviour
    {
        private PolygonCollider2D collisionCollider;
        private readonly float pickUpRadius = 0.4f;
        private readonly float speed = 10f;

        [Header("Data")]
        public ItemData itemData;

        // Cache the SpriteRenderer
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            collisionCollider = GetComponent<PolygonCollider2D>();
        }

        // ADD THIS: BreakAndPlace will call this right after instantiating it
        public void Initialize(ItemData data)
        {
            itemData = data;
            if (itemData != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = itemData.sprite;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            collisionCollider.enabled = false;
            transform.position = Vector2.MoveTowards(
                transform.position, other.transform.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, other.transform.position) <= pickUpRadius)
            {
                ItemStack itemStack = new(itemData)
                {
                    amount = 1,
                };
                Inventory.Singleton.Add(itemStack);
                Destroy(gameObject);
            }
        }
    }
}
