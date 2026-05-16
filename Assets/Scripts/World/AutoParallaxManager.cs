using System.Collections.Generic;
using UnityEngine;

namespace World
{
    [System.Serializable]
    public class BackgroundLayer
    {
        public string layerName;
        public Sprite backgroundSprite;

        [Tooltip("How much the layer moves. 1 = Static. 0 = Locked to camera. (e.g. Far Sky = 0.9, Close Cave = 0.2)")]
        public Vector2 parallaxEffect;

        [Tooltip("Vertical offset from its base starting position.")]
        public float yOffset;

        [Tooltip("Sorting order (make sure these are negative so they render behind your tiles)")]
        public int sortingOrder = -10;

        [Tooltip("Scale multiplier for the sprite")]
        public float scale = 1f;
    }

    public class AutoParallaxManager : MonoBehaviour
    {
        [Header("Dependencies")]
        public Camera mainCamera;

        [Header("Layer Setup")]
        public List<BackgroundLayer> surfaceLayers = new List<BackgroundLayer>();
        public List<BackgroundLayer> caveLayers = new List<BackgroundLayer>();

        private void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;

            // 1. Calculate the average Y index of your ground level
            // (worldHeight * 0.75f is base, 0.5f is the average of your 0.4 to 0.6 noise)
            float averageGroundIndex = (WorldManager.Instance.worldHeight * 0.75f) * 0.5f;

            // 2. Convert grid index to actual Unity World Space units
            float tileSize = 0.5f;
            float estimatedGroundY = averageGroundIndex * tileSize;

            // Generate Surface
            foreach (var layer in surfaceLayers)
            {
                GenerateLayerObject(layer, estimatedGroundY + layer.yOffset, "Surface_");
            }

            // Generate Caves
            foreach (var layer in caveLayers)
            {
                GenerateLayerObject(layer, estimatedGroundY + layer.yOffset, "Cave_");
            }
        }

        private void GenerateLayerObject(BackgroundLayer data, float startY, string prefix)
        {
            if (data.backgroundSprite == null) return;

            // 1. Create the GameObject
            GameObject layerObj = new GameObject(prefix + data.layerName);
            layerObj.transform.SetParent(this.transform);

            // Scale it up
            layerObj.transform.localScale = new Vector3(data.scale, data.scale, 1f);

            // 2. Add Sprite Renderer
            SpriteRenderer sr = layerObj.AddComponent<SpriteRenderer>();
            sr.sprite = data.backgroundSprite;
            sr.sortingOrder = data.sortingOrder;

            // 3. Setup the custom parallax component
            ParallaxComponent parallaxScript = layerObj.AddComponent<ParallaxComponent>();
            parallaxScript.cam = mainCamera.transform;
            parallaxScript.parallaxEffect = data.parallaxEffect;

            // Position it using our new mathematically accurate Y coordinate!
            layerObj.transform.position = new Vector3(mainCamera.transform.position.x, startY, 0);
        }
    }

    // ---------------------------------------------------------
    // The missing piece! Attach this below the AutoParallaxManager
    // ---------------------------------------------------------
    public class ParallaxComponent : MonoBehaviour
    {
        [HideInInspector] public Transform cam;
        [HideInInspector] public Vector2 parallaxEffect;

        private float length;
        private float startPosX;
        private float startPosY;

        private void Start()
        {
            startPosX = transform.position.x;
            startPosY = transform.position.y;

            // Get the sprite's width * scale to know when to loop it
            length = GetComponent<SpriteRenderer>().bounds.size.x;
        }

        private void LateUpdate()
        {
            // How far the camera has moved relative to the layer
            float tempX = (cam.position.x * (1 - parallaxEffect.x));

            // How far to actually move the layer
            float distanceX = (cam.position.x * parallaxEffect.x);
            float distanceY = (cam.position.y * parallaxEffect.y);

            // Apply movement
            transform.position = new Vector3(startPosX + distanceX, startPosY + distanceY, transform.position.z);

            // Infinite looping logic (Horizontal only, we don't want caves to loop vertically!)
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
