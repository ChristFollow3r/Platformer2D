using Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Chunks
{
    public class Chunk
    {
        public bool isLoaded;
        public const int chunkSize = 16;
        
        private Vector2Int chunkPosition;
        private readonly Tilemap tileMap;
        private bool notCreated;

        public Chunk(bool isLoaded, Vector2Int chunkPosition, Tilemap tilemap)
        {
            this.isLoaded = isLoaded;
            this.chunkPosition = chunkPosition;
            this.tileMap = tilemap;
            notCreated = true;
        }

        public void BuildTiles()
        {
            int x = chunkPosition.x * chunkSize;
            int y = chunkPosition.y * chunkSize;
            
            for (int i = x; i < x + chunkSize; i++)
            {
                for (int j = y; j < y + chunkSize; j++)
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

            notCreated = false;
        }

        public void LoadChunk()
        {
            if (notCreated) 
                BuildTiles();
            tileMap.gameObject.SetActive(true);
            isLoaded = true;
        }

        public void UnLoadChunk()
        {
            tileMap.gameObject.SetActive(false);
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
