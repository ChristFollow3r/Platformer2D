using Scriptable_Objects_Scripts;

namespace Data
{
    public class World 
    {
        public readonly int width;
        public readonly int height;

        private readonly BlockType[] blocks;
        private readonly PropType[] props;

        public World(int width, int height)
        {
            this.width = width;
            this.height = height;
            blocks = new BlockType[width * height];
            props = new PropType[width * height];
        }

        #region Blocks
        public BlockType GetBlockTypes(int x, int y) // Getter
        {
            return blocks[(y * width) + x]; 
        }

        public void SetBlockType(int x, int y, BlockType type) // Setter (Just for the block type).
        {
            blocks[(y * width) + x] = type;
        }

        #endregion
        
        #region Props

        public void SetPropType(int x, int y, PropType type)
        {
            props[(y * width) + x] = type;
        }
        public PropType GetPropType(int x, int y)
        {
            return props[(y * width) + x];
        }
        #endregion
        
        public bool SafeCheck(int x, int y) // Will return true if it's safe to check; if it's out of bounds it won't crash.
        {
            return (x >= 0 && x < width && y >= 0 && y < height);
        }
    }
}
