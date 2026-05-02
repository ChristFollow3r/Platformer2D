using Data;
using Player;
using UnityEngine;

namespace Items // Maybe I should add this to items?
{
  public class ItemEntity : MonoBehaviour
  {
    private PolygonCollider2D collisionCollider;
    private readonly float pickUpRadius = 0.4f;
    private readonly float speed = 10f;

    [Header("Data")]
    public ItemData itemData;


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

      if (Vector2.Distance(transform.position, other.transform.position) <= pickUpRadius)
      {
        ItemStack itemStack = new()
        {
          amount = 1,
          data = itemData,
        };
        Inventory.Add(itemStack);
        Destroy(gameObject);
      }
    }
  }
}
