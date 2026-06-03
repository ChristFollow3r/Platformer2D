using Data;
using Player;
using UnityEngine;

namespace Items
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DropComponent : MonoBehaviour
    {
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private new CircleCollider2D collider;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [SerializeField] private ItemData itemData;

        private readonly float pickUpRadius = 0.4f;
        private readonly float magnetSpeed = 15f;

        [SerializeField] private float rotateSpeed = 100f;
        [SerializeField] private float popForceUp = 5f;
        [SerializeField] private float popForceSide = 2f;

        [Header("Settings")]
        [Tooltip("Delay before item can be picked up so player doesn't instantly catch dropped items.")]
        [SerializeField] private float pickupDelay = 0.5f;
        private float spawnTime;

        private Rigidbody2D rb;
        public Transform player;
        private bool isBeingPickedUp;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spawnTime = Time.time;

            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;

                    // FIX: Ignore collision so tossed items bounce on the ground, not off the player's head/feet
                    Collider2D playerCollider = playerObj.GetComponent<Collider2D>();
                    if (playerCollider != null && collider != null)
                    {
                        Physics2D.IgnoreCollision(collider, playerCollider, true);
                    }
                }
            }

            if (itemData == null)
            {
                SendMessage("GetItemData", SendMessageOptions.DontRequireReceiver);
            }

            // Apply a real physics impulse to make the item "pop" out
            float randomX = Random.Range(-popForceSide, popForceSide);
            rb.AddForce(new Vector2(randomX, popForceUp), ForceMode2D.Impulse);
        }

        private void Update()
        {
            if (Inventory.Singleton == null || player == null) return;

            // Visual spinning effect
            // if (!isBeingPickedUp)
            // {
            //     transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
            // }
            transform.rotation = Quaternion.Euler(transform.rotation * new Vector3(0, 0, 1));

            if (itemData == null) return;

            // DELAY CHECK
            if (Time.time < spawnTime + pickupDelay) return;

            // Check if player is close enough to suck the item in
            if (!isBeingPickedUp && Inventory.Singleton.Fits(itemData))
            {
                float distance = Vector2.Distance(player.position, transform.position);
                if (distance <= 1.5f)
                {
                    isBeingPickedUp = true;

                    // Turn off physics so the item can fly through walls/blocks directly to the player
                    rb.isKinematic = true;
                    rb.linearVelocity = Vector2.zero;
                    collider.enabled = false;
                }
            }

            // Magnet logic: Fly to player
            if (isBeingPickedUp)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, magnetSpeed * Time.deltaTime);

                if (Vector2.Distance(transform.position, player.position) <= pickUpRadius)
                {
                    if (pickupSound != null)
                        AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position);

                    Inventory.Singleton.Add(new ItemStack(itemData) { amount = 1 });
                    Destroy(gameObject);
                }
            }
        }

        public void SetItem(ItemData itemData)
        {
            this.itemData = itemData;
            if (spriteRenderer != null && itemData != null)
            {
                spriteRenderer.sprite = itemData.sprite;
            }
        }
    }
}
