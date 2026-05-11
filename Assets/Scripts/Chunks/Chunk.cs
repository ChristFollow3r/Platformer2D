using Data;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Chunks
{
    public class Chunk
    {
        public bool isLoaded;
        public const int ChunkSize = 32;
        
        private readonly Vector2Int chunkPosition;
        private readonly Tilemap blockTileMap;
        private readonly Tilemap propTileMap;
        private bool notCreated;

        public Chunk(bool isLoaded, Vector2Int chunkPosition, Tilemap bTilemap, Tilemap pTilemap)
        {
            this.isLoaded = isLoaded;
            this.chunkPosition = chunkPosition;
            blockTileMap = bTilemap;
            propTileMap = pTilemap;
            notCreated = true;
        }

        private void BuildTiles()
        {
            int x = chunkPosition.x * ChunkSize;
            int y = chunkPosition.y * ChunkSize;
            
            for (int i = x; i < x + ChunkSize; i++)
            {
                for (int j = y; j < y + ChunkSize; j++)
                {
                    if (i >= WorldData.World.width || j >= WorldData.World.height) continue;
                    
                    var blockType = WorldData.World.GetBlockTypes(i, j);
                    var propType = WorldData.World.GetPropType(i, j);
                    Vector3Int pos = new Vector3Int(i, j, 0);
                    
                    float lightValue = WorldData.World.lightValues[i, j];
                    float bloomMultiplier = 1.5f;
                    
                    Color lightColor = new Color(lightValue * bloomMultiplier, lightValue * bloomMultiplier, lightValue * bloomMultiplier, 1f);
                    
                    if (blockType != BlockType.Air)
                    {
                        var tile = ScriptableObject.CreateInstance<Tile>(); 
                        tile.sprite = WorldData.BlockDictionary[blockType].sprite;
                        
                        tile.flags = TileFlags.None;
                        
                        blockTileMap.SetTile(pos, tile);
                        blockTileMap.SetColor(pos, lightColor);
                        
                        blockTileMap.gameObject.layer = LayerMask.NameToLayer("Block");

                    }

                    if (propType != PropType.None)
                    {
                        var propTile = ScriptableObject.CreateInstance<Tile>();
                        propTile.sprite = WorldData.PropDictionary[propType].sprite;
                        propTile.colliderType = Tile.ColliderType.Sprite;
                        
                        propTileMap.SetTile(new Vector3Int(i, j, 0), propTile);
                        propTileMap.SetColor(pos, lightColor);
                    }
                    
                }
            }

            notCreated = false;
        }

        public void LoadChunk()
        {
            if (notCreated) BuildTiles();
    
            blockTileMap.gameObject.SetActive(true);
            propTileMap.gameObject.SetActive(true);
            isLoaded = true;
        }

        public void UnLoadChunk()
        {
            blockTileMap.gameObject.SetActive(false);
            propTileMap.gameObject.SetActive(false);
            isLoaded = false;
        }

        public void UpdateTile(int x, int y) 
        {
            var position = new Vector3Int(x, y, 0);
            BlockType blockType = WorldData.World.GetBlockTypes(x, y);
            PropType propType = WorldData.World.GetPropType(x, y);
            
            if (blockType == BlockType.Air) blockTileMap.SetTile(position, null);

            else
            {
                var tile = ScriptableObject.CreateInstance<Tile>(); 
                tile.sprite = WorldData.BlockDictionary[blockType].sprite;
                blockTileMap.SetTile(position, tile);
            }
            
            if (propType == PropType.None)
                propTileMap.SetTile(position, null);

            else
            {
                Tile propTile = propTileMap.GetTile<Tile>(position);
                if (propTile is not null)
                {
                    propTile.sprite = WorldData.PropDictionary[propType].sprite;
                    propTileMap.RefreshTile(position);
                }
            }
            
        }
    }
}
