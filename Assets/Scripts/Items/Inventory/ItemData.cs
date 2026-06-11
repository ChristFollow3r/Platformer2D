using System.Collections.Generic;
using Items;
using Items.Overlays;
using Player;
using Scriptable_Objects_Scripts;
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
        public float requiredMiningPower = 0f;
        public float fuelDuration;
        public ModData modData;
        public OverlayType overlayType;

        [Header("Block Drops")]
        public List<Drop> drops = new List<Drop>();

        [Header("Feedback")]
        public AudioClip hitSound;

        public string description;
    }

}
