using Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Chunks
{
    public class Chunk
    {
        public bool isLoaded;
        private Vector2Int chunkPosition;
        private readonly Tilemap tileMap;

        public Chunk(bool isLoaded, Vector2Int chunkPosition, Tilemap tilemap)
        {
            this.isLoaded = isLoaded;
            this.chunkPosition = chunkPosition;
            this.tileMap = tilemap;
        }

        public void LoadChunk()
        {
            int x = chunkPosition.x * 16;
            int y = chunkPosition.y * 16;
            
            for (int i = x; i < x + 16; i++)
            {
                for (int j = y; j < y + 16; j++)
                {
                    if (i >= WorldData.World.width || j >= WorldData.World.height) continue;
                    var blockType = WorldData.World.GetBlockTypes(i, j);

                    if (blockType == BlockType.Air) continue;

                    var tile = ScriptableObject
                        .CreateInstance<Tile>(); // So you cannot do new Tile() cause it gives error
                    tile.sprite = WorldData.BlockDictionary[blockType].sprite;
                    tileMap.SetTile(new Vector3Int(i, j, 0), tile);
                }
            }

            isLoaded = true;
        }

        public void UnLoadChunk()
        {
            var x = chunkPosition.x * 16;
            var y = chunkPosition.y * 16;

            for (var i = x; i < x + 16; i++)
            {
                for (var j = y; j < y + 16; j++)
                {
                    tileMap.SetTile(new Vector3Int(i, j, 0),
                        null); //  The sky will be a background Image, I won't have a separate tilemap for the sky.
                }
            }

            isLoaded = false;
        }

        public void UpdateTile(int x, int y)
        {
            BlockType blockType = WorldData.World.GetBlockTypes(x, y);
            
            if (blockType == BlockType.Air)
                tileMap.SetTile(new Vector3Int(x, y, 0), null);
            else
            {
                Tile tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = WorldData.BlockDictionary[blockType].sprite;
                tileMap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }
}
