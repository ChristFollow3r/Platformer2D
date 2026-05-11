using Player;
using UnityEngine;

namespace Items // Maybe I should add this to items?
{
  public class DropBehavior : MonoBehaviour
  {
    [SerializeField] private AudioClip dropSound;

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
      // TODO: Check if inventory is full
      collisionCollider.enabled = false;
      transform.position = Vector2.MoveTowards(
          transform.position, other.transform.position, speed * Time.deltaTime);

      if (Vector2.Distance(transform.position, other.transform.position) <= pickUpRadius)
      {
        AudioSource.PlayClipAtPoint(dropSound, transform.position);
        // TODO: Add item to inv
        Destroy(gameObject);
      }
    }
  }
}
