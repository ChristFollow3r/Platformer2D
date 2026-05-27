using System.Collections.Generic;
using UnityEngine;

namespace World.Background
{
    [System.Serializable]
    public class BackgroundLayer
    {
        public string layerName;
        public Sprite backgroundSprite;
        public Vector2 parallaxEffect;
        public float yOffset;
        public int sortingOrder = -10;
        public float scale = 1f;
    }

    public class AutoParallaxManager : MonoBehaviour
    {
        public float CurrentSurfaceAlpha { get; private set; }

        public Camera mainCamera;

        public float transitionOffsetStart = 2f;
        public float transitionOffsetEnd = 15f;

        public List<BackgroundLayer> surfaceLayers = new List<BackgroundLayer>();
        public List<BackgroundLayer> caveLayers = new List<BackgroundLayer>();

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
            if (mainCamera == null || WorldManager.Instance == null) return;

            float currentY = mainCamera.transform.position.y;
            float currentX = mainCamera.transform.position.x;

            float dynamicSurfaceY = WorldManager.Instance.GetSurfaceY(currentX);

            float fadeStart = dynamicSurfaceY - transitionOffsetStart;
            float fadeEnd = dynamicSurfaceY - transitionOffsetEnd;

            CurrentSurfaceAlpha = Mathf.InverseLerp(fadeEnd, fadeStart, currentY);
            float caveAlpha = 1f - CurrentSurfaceAlpha;

            foreach (var p in spawnedSurface) p.SetAlpha(CurrentSurfaceAlpha);
            foreach (var p in spawnedCaves) p.SetAlpha(caveAlpha);
        }
    }
}
