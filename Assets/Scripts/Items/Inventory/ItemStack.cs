

using System;
using Data;

namespace Items
{
    [Serializable]
    public record ItemStack
    {
        public ItemData data;
        public short amount;
        public int durability;
        public float duration;
    }
}
