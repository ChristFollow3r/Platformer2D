using Data.Inventory;
using UnityEngine;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        public PlayerMovement PlayerMovement { get; private set; }
        public Shared.Health PlayerHealth { get; private set; }
        public BreakAndPlace BreakPlace { get; private set; }
        public Inventory Inventory { get; private set; }
        
        [SerializeField] private UI.Inventory uiInventory;

        private void Awake()
        {
            PlayerMovement = GetComponent<PlayerMovement>();
            PlayerHealth = GetComponent<Shared.Health>();
            BreakPlace = GetComponent<BreakAndPlace>();
            Inventory = new Inventory();
        }
    }
}
