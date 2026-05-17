using UnityEngine;

namespace World.Background
{
    public class ParallaxComponent : MonoBehaviour
    {
        public Transform cam;
        public Vector2 parallaxEffect;

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
            length = sr.bounds.size.x;

            float localWidth = sr.sprite.bounds.size.x;
            CreateSideClone(sr, localWidth);
            CreateSideClone(sr, -localWidth);
        }

        private void CreateSideClone(SpriteRenderer original, float xOffset)
        {
            GameObject clone = new GameObject("Clone");
            clone.transform.SetParent(this.transform);
            clone.transform.localPosition = new Vector3(xOffset, 0, 0);
            clone.transform.localScale = Vector3.one;

            SpriteRenderer cloneSr = clone.AddComponent<SpriteRenderer>();
            cloneSr.sprite = original.sprite;
            cloneSr.sortingOrder = original.sortingOrder;
        }

        // THE FIX: Allows the Manager to dynamically fade this layer and its clones
        public void SetAlpha(float alpha)
        {
            // Gets the SpriteRenderer on this object AND the clones we created
            SpriteRenderer[] allRenderers = GetComponentsInChildren<SpriteRenderer>();

            foreach (var sr in allRenderers)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }

        private void LateUpdate()
        {
            float tempX = (cam.position.x * (1 - parallaxEffect.x));
            float distanceX = (cam.position.x * parallaxEffect.x);
            float distanceY = (cam.position.y - startCamPosY) * parallaxEffect.y;

            transform.position = new Vector3(startPosX + distanceX, startPosY + distanceY, transform.position.z);

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
