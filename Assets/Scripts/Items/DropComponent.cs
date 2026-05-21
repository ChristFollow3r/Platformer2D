using Data;
using Player;
using Unity.VisualScripting;
using UnityEngine;

namespace Items // Maybe I should add this to items?
{
    public class DropComponent : MonoBehaviour
    {
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private new CircleCollider2D collider;
        [SerializeField] private SpriteRenderer spriteRenderer;
        private readonly float pickUpRadius = 0.4f;
        private readonly float speed = 10f;
        private ItemData itemData;

        [SerializeField] private float bobHeight;
        [SerializeField] private float bobSpeed;
        [SerializeField] private float rotateSpeed;
        [SerializeField] private float initialPopForce;
        [SerializeField] private float gravity = -9.8f;
        private float groundY;
        private float verticalVelocity;
        private bool hasLanded = false;
        public Transform player;
        private bool isBeingPickedUp;
        private float bobTimer = 0f;

        /// <summary>Ran by unity on first enable</summary>
        private void Start()
        {
            #region Start
            transform.Rotate(Vector3.up, Random.Range(0, 20), Space.World);
            transform.position += Vector3.up * Random.Range(0, 0.3f);
            transform.position += Vector3.right * Random.Range(-0.3f, 0.3f);
            verticalVelocity = initialPopForce;
            groundY = transform.position.y;
            #endregion
        }

        /// <summary>Ran by unity each frame</summary>
        private void Update()
        {
            #region Update
            if (Inventory.Singleton == null) return;
            if (player == null) return;
            if (!isBeingPickedUp || !Inventory.Singleton.Fits(itemData))
            {
                Bop();
                float distance = Vector2.Distance(player.position, transform.position);
                if (distance <= 1.5) isBeingPickedUp = true;
                return;
            }

            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, player.position) <= pickUpRadius)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                collider.enabled = false;
                Inventory.Singleton.Add(new ItemStack(itemData) { amount = 1 });
                Destroy(gameObject);
            }
            #endregion
        }

        private void Bop()
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

            if (!hasLanded)
            {
                verticalVelocity += gravity * Time.deltaTime;
                transform.position += Vector3.up * verticalVelocity * Time.deltaTime;

                if (transform.position.y <= groundY)
                {
                    Vector3 pos = transform.position;
                    pos.y = groundY;
                    transform.position = pos;
                    hasLanded = true;
                }
            }
            else
            {
                bobTimer += Time.deltaTime * bobSpeed;
                float newY = groundY + Mathf.Sin(bobTimer) * bobHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        public void SetItem(ItemData itemData)
        {
            #region SetItem
            this.itemData = itemData;
            spriteRenderer.sprite = itemData.sprite;
            #endregion
        }
    }
}
