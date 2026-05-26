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
        public float CurrentSurfaceAlpha { get; private set; }

        [Header("Dependencies")]
        public Camera mainCamera;

        [Header("Biome Blending Setup")]
        [Tooltip("The Y height where the surface starts to fade out (e.g. 15)")]
        public float transitionStartY = 15f;
        [Tooltip("The Y height where it becomes 100% cave (e.g. 5)")]
        public float transitionEndY = 5f;

        [Header("Layer Setup")]
        public List<BackgroundLayer> surfaceLayers = new List<BackgroundLayer>();
        public List<BackgroundLayer> caveLayers = new List<BackgroundLayer>();

        // Lists to keep track of the spawned objects so we can fade them
        private List<ParallaxComponent> spawnedSurface = new List<ParallaxComponent>();
        private List<ParallaxComponent> spawnedCaves = new List<ParallaxComponent>();

        private void Start()
        {
            if (mainCamera == null) mainCamera = Camera.main;

            float startX = mainCamera.transform.position.x;
            float startY = mainCamera.transform.position.y;

            foreach (var layer in surfaceLayers)
            {
                spawnedSurface.Add(GenerateLayerObject(layer, startX, startY + layer.yOffset, "Surface_"));
            }

            foreach (var layer in caveLayers)
            {
                spawnedCaves.Add(GenerateLayerObject(layer, startX, startY + layer.yOffset, "Cave_"));
            }
        }

        private ParallaxComponent GenerateLayerObject(BackgroundLayer data, float startX, float startY, string prefix)
        {
            GameObject layerObj = new GameObject(prefix + data.layerName);
            layerObj.transform.SetParent(this.transform);
            layerObj.transform.localScale = new Vector3(data.scale, data.scale, 1f);

            SpriteRenderer sr = layerObj.AddComponent<SpriteRenderer>();
            sr.sprite = data.backgroundSprite;
            sr.sortingOrder = data.sortingOrder;

            ParallaxComponent parallaxScript = layerObj.AddComponent<ParallaxComponent>();
            parallaxScript.cam = mainCamera.transform;
            parallaxScript.parallaxEffect = data.parallaxEffect;

            layerObj.transform.position = new Vector3(startX, startY, 0);

            return parallaxScript;
        }

        private void Update()
        {
            if (mainCamera == null) return;

            float currentY = mainCamera.transform.position.y;

            CurrentSurfaceAlpha = Mathf.InverseLerp(transitionEndY, transitionStartY, currentY);

            float caveAlpha = 1f - CurrentSurfaceAlpha;

            foreach (var p in spawnedSurface) p.SetAlpha(CurrentSurfaceAlpha);
            foreach (var p in spawnedCaves) p.SetAlpha(caveAlpha);
        }
    }
}
