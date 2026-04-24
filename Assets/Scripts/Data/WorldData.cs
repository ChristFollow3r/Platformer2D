using System.Collections.Generic;
using Scriptable_Objects_Scripts;

namespace Data
{
    public enum BlockType { Air, Dirt, Grass, Stone, Sand }

    public enum PropType { None, Bush, Stone, Tree, Copper, Iron, Coal, Sulphur }

    public enum Hardness { Level1, Level2, Level3, Level4, Level5 }

    public static class WorldData
    {
        public static readonly Dictionary<BlockType, Block> BlockDictionary = new();
        public static readonly Dictionary<PropType, Prop> PropDictionary = new();
        public static World World;
    }
}