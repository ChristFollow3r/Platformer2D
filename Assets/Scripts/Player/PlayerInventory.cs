using UnityEngine;

namespace Player
{
    public class PlayerInventory : MonoBehaviour
    {
        private static PlayerInventory Instance { get; set; }
        public ScriptableObject[] items;
    

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    
    
    
    }
}
