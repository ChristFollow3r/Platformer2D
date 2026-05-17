using UnityEngine;

namespace Shaders
{
    public class PlayerLightUpdate : MonoBehaviour
    {
        private static readonly int PlayerPosition = Shader.PropertyToID("_PlayerPosition");
        public Material blockMaterial;

        void Update()
        {
            blockMaterial.SetVector(PlayerPosition, transform.position);
        }
    }
}
