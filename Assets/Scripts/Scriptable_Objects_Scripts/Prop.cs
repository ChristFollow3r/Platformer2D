using UnityEngine;
using System.Collections.Generic;
using Data;

namespace Scriptable_Objects_Scripts
{
    [System.Serializable]
    public class Drop
    {
        public ItemData item;
        public int minAmount;
        public int maxAmount;
        [Range(0, 101)] public int dropChance = 100;
    }

    [CreateAssetMenu(fileName = "Prop", menuName = "Scriptable Objects/Prop")]
    public class Prop : ScriptableObject
    {
        public List<Drop> drops = new List<Drop>();
        public Sprite sprite;

        public new string name;
        public PropType type;

        public int hardness;
        [Range(0f, 100f)] public float spawnChance;
        public bool hasPriority;
        public bool isFromSurface;

        [Tooltip("How many blocks of empty air this prop needs to the left and right.")]
        public int requiredSpace;

        public BlockType[] allowedGroundBlocks;
    }
}
