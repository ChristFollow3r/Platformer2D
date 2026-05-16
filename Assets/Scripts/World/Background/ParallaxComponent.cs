using UnityEngine;

namespace World.Background
{
    public class ParallaxComponent : MonoBehaviour
    {
        [HideInInspector] public Transform cam;
        [HideInInspector] public Vector2 parallaxEffect;

        private float length;
        private float startPosX;
        private float startPosY;
        private float startCamPosY;

        private void Start()
        {
            startPosX = transform.position.x - (cam.position.x * parallaxEffect.x);
            startPosY = transform.position.y;
            startCamPosY = cam.position.y;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();

            // World space length (used for knowing when to teleport)
            length = sr.bounds.size.x;

            // THE FIX: Auto-generate left and right clones to hide the seams!
            // We use local width here so it respects the Scale multiplier you set in the manager.
            float localWidth = sr.sprite.bounds.size.x;

            CreateSideClone(sr, localWidth);  // Spawn a clone exactly to the right
            CreateSideClone(sr, -localWidth); // Spawn a clone exactly to the left
        }

        private void CreateSideClone(SpriteRenderer original, float xOffset)
        {
            // Create the dummy object
            GameObject clone = new GameObject("Clone");
            clone.transform.SetParent(this.transform); // Attach it to the main moving background

            // Offset it horizontally, keep local scale at 1 so it inherits the parent's scale
            clone.transform.localPosition = new Vector3(xOffset, 0, 0);
            clone.transform.localScale = Vector3.one;

            // Copy the sprite and sorting order so it looks identical
            SpriteRenderer cloneSr = clone.AddComponent<SpriteRenderer>();
            cloneSr.sprite = original.sprite;
            cloneSr.sortingOrder = original.sortingOrder;
        }

        private void LateUpdate()
        {
            float tempX = (cam.position.x * (1 - parallaxEffect.x));
            float distanceX = (cam.position.x * parallaxEffect.x);
            float distanceY = (cam.position.y - startCamPosY) * parallaxEffect.y;

            // Move the parent (which now drags the left and right clones with it)
            transform.position = new Vector3(startPosX + distanceX, startPosY + distanceY, transform.position.z);

            // Infinite Looping Treadmill
            if (tempX > startPosX + length)
            {
                startPosX += length;
            }
            else if (tempX < startPosX - length)
            {
                startPosX -= length;
            }
        }
    }
}
