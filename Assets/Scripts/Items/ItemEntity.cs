using System.Collections;
using Data;
using Player;
using UnityEngine;

namespace Items
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ItemEntity : MonoBehaviour
    {
        private PolygonCollider2D collisionCollider;
        private Transform player;

        private readonly float pickUpRadius = 0.4f;
        private readonly float suckInRadius = 1.5f; // Distance before magnet starts
        private readonly float speed = 10f;

        [Header("Settings")]
        [Tooltip("Time in seconds before the item can be picked up")]
        [SerializeField] private float pickupDelay = 0.5f;
        private float spawnTime;
        private bool isBeingPickedUp = false;
        private ItemStack stack = null;


        [Header("Data")]
        public ItemData itemData;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spawnTime = Time.time;
        }

        private void Start()
        {
            collisionCollider = GetComponent<PolygonCollider2D>();

            // Find the player and explicitly ignore physical collision
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;

                // FIX: This completely stops the player from standing on the item.
                Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
                if (playerCollider != null && collisionCollider != null)
                {
                    Physics2D.IgnoreCollision(collisionCollider, playerCollider, true);
                }
            }
        }

        public void Initialize(ItemData data)
        {
            itemData = data;
            if (itemData != null && spriteRenderer != null)
            {
                spriteRenderer.sprite = itemData.sprite;
            }
            spawnTime = Time.time;
        }

        private void Update()

        {
            if (player == null || Inventory.Singleton == null) return;

            // DELAY CHECK: Prevents instant pickup when breaking blocks or dropping items
            if (Time.time < spawnTime + pickupDelay) return;

            // Magnet start logic (replaces OnTriggerStay2D)
            if (!isBeingPickedUp)
            {
                if (Vector2.Distance(transform.position, player.position) <= suckInRadius)
                {
                    isBeingPickedUp = true;
                    collisionCollider.enabled = false;
                }
            }

            // Fly to player
            if (isBeingPickedUp)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position, player.position, speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, player.position) <= pickUpRadius)
                {
                    ItemStack itemStack1 = stack == null ? new(itemData)
                    {
                        amount = 1,
                    } : stack;
                    Inventory.Singleton.Add(itemStack1);
                    Destroy(gameObject);
                }

                ItemStack itemStack = stack == null ? new(itemData)
                {
                    amount = 1,
                } : stack;
                Inventory.Singleton.Add(itemStack);
                Destroy(gameObject);
            }
        }
    }
}
