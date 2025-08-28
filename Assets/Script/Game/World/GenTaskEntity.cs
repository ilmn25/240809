using UnityEngine;

public class GenTaskEntity : Gen
{
    private static int _id;
    private static int Dirt => _id == 0 ? Block.ConvertID(ID.DirtBlock) : _id; 
    public static void Run(Vector3Int currentCoordinate, Chunk currentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z <  World.ChunkSize; z++)
                {
                    if (
                        y + 1 < World.ChunkSize &&
                        currentChunk[x, y, z] != 0 && 
                        currentChunk[x, y + 1, z] == 0) 
                    {
                        Vector3Int position = currentCoordinate + new Vector3Int(x, y + 1, z);
                        double rng = Random.NextDouble();
                        if (currentChunk[x, y, z] == Dirt)
                        {
                            { 
                                if (rng <= 0.02)
                                {
                                    currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Tree,position));
                                }
                                else if (rng <= 0.04)
                                {
                                    currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Bush, position));
                                }
                                else if (rng <= 0.2)
                                {
                                    currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Grass, position));
                                }
                                else if (rng <= 0.2005)
                                {
                                    currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Sticks, position));
                                }
                                else if (rng <= 0.201)
                                {
                                    currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Flint, position));
                                }
                            }
                        }
                        else
                        {
                            if (rng <= 0.0004)
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Chest, position));
                            } 
                            else if (rng <= 0.02)
                            {
                                currentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Slab, position));
                            }
                        }
                    }
                }
            }
        }
    }
}