using System.Collections.Generic;
using UnityEngine;

namespace World.Background
{
    [System.Serializable]
    public class BackgroundLayer
    {
        public string layerName;
        public Sprite backgroundSprite;

        [Tooltip("How much the layer moves. 1 = Static. 0 = Locked to camera.")]
        public Vector2 parallaxEffect;

        [Tooltip("Vertical offset from its base starting position.")]
        public float yOffset;

        [Tooltip("Sorting order (negative so they render behind tiles)")]
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

            // Anchor directly to where the camera starts, completely ignoring world grid coordinates.
            float startX = mainCamera.transform.position.x;
            float startY = mainCamera.transform.position.y;

            foreach (var layer in surfaceLayers)
            {
                GenerateLayerObject(layer, startX, startY + layer.yOffset, "Surface_");
            }

            foreach (var layer in caveLayers)
            {
                GenerateLayerObject(layer, startX, startY + layer.yOffset, "Cave_");
            }
        }

        private void GenerateLayerObject(BackgroundLayer data, float startX, float startY, string prefix)
        {
            if (data.backgroundSprite == null) return;

            GameObject layerObj = new GameObject(prefix + data.layerName);
            layerObj.transform.SetParent(this.transform);

            layerObj.transform.localScale = new Vector3(data.scale, data.scale, 1f);

            SpriteRenderer sr = layerObj.AddComponent<SpriteRenderer>();
            sr.sprite = data.backgroundSprite;
            sr.sortingOrder = data.sortingOrder;

            ParallaxComponent parallaxScript = layerObj.AddComponent<ParallaxComponent>();
            parallaxScript.cam = mainCamera.transform;
            parallaxScript.parallaxEffect = data.parallaxEffect;

            // Spawn exactly at the camera's X and Y
            layerObj.transform.position = new Vector3(startX, startY, 0);
        }
    }
}
