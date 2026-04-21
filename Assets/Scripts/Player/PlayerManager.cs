using Data.Inventory;
using UnityEngine;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        public PlayerMovement PlayerMovement { get; private set; }
        public BreakAndPlace BreakPlace { get; private set; }
        public Inventory Inventory { get; private set; }
        
        [SerializeField] private UI.Inventory uiInventory;

        private void Awake()
        {
            Inventory = new Inventory();
            PlayerMovement = GetComponent<PlayerMovement>();
            BreakPlace = GetComponent<BreakAndPlace>();
        }
    }
}
