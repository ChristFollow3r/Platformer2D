

using System;
using Data;

namespace Items
{
    [Serializable]
    public record ItemStackBuilder
    {
        public ItemData data;
        public short amount;
        public float duration;
    }
    [Serializable]
    public record ItemStack
    {
        public ItemData data { get; private set; }
        public short amount;
        public float duration;

        public ItemStack(ItemData data)
        {
            this.data = data;
            if (data.modData != null) duration = data.modData.duration;
        }

        public ItemStack(ItemStack baseStack)
        {
            data = baseStack.data;
            amount = baseStack.amount;
            duration = baseStack.duration;
        }
    }
}
