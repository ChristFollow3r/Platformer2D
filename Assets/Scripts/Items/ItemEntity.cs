using System.Collections;
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

        [Header("Settings")]
        [Tooltip("Time in seconds before the item can be picked up")]
        [SerializeField] private float pickupDelay = 0f;
        private float spawnTime;

        [Header("Data")]
        public ItemData itemData;

        // Cache the SpriteRenderer
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            // Record the time the item was created
            spawnTime = Time.time;
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

            // Reset the timer when initialized (crucial if you are dropping the item dynamically)
            spawnTime = Time.time;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // DELAY CHECK: If the current time is less than the spawn time plus the delay, do nothing.
            if (Time.time < spawnTime + pickupDelay) return;

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
