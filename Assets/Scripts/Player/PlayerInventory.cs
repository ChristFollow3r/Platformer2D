using UnityEngine;

namespace Player
{
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }
        public ScriptableObject[] items;
        public ScriptableObject[] itemSlots;
        public readonly ScriptableObject currentHeldItem;

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
