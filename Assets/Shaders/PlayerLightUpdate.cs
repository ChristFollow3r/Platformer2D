using UnityEngine;

namespace Shaders
{
    public class PlayerLightUpdate : MonoBehaviour
    {
        private static readonly int PlayerPosition = Shader.PropertyToID("_PlayerPosition");
        public Material blockMaterial;
        private Vector3 lastPosition;

        void Update()
        {
            if (Vector3.SqrMagnitude(transform.position - lastPosition) > 0.001f)
            {
                blockMaterial.SetVector(PlayerPosition, transform.position);
                lastPosition = transform.position;
            }
        }
    }
}
