using Data;
using Player;
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


        /// <summary>Method</summary>
        public void SetItem(ItemData itemData)
        {
            #region SetItem
            this.itemData = itemData;
            spriteRenderer.sprite = itemData.sprite;
            #endregion
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            // TODO: Check if inventory is full
            transform.position = Vector2.MoveTowards(
                transform.position, other.transform.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, other.transform.position) <= pickUpRadius)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                Destroy(gameObject);
                collider.enabled = false;
                Inventory.Singleton.Add(new ItemStack { data = itemData, amount = 1 });
            }
        }
    }
}
