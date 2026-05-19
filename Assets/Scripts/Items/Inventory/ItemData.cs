using Items;
using Items.Overlays;
using Player;
using UnityEngine;


namespace Data
{
    [CreateAssetMenu(fileName = "Item", menuName = "Item")]
    public class ItemData : ScriptableObject
    {
        public new string name;
        public Sprite sprite;
        public bool isStackable => stack < 0;
        public int stack = 64;
        public bool isPlacable;
        public bool isConsumable;
        public bool isFuel;
        public EquipmentType equipmentType;
        public BlockType blockType;
        public float hardness;
        public float fuelDuration;

        public OverlayType overlayType;
    }

}
