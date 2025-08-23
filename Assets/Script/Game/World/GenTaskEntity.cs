using UnityEngine;

public class GenTaskEntity : WorldGen
{
    private static int _id;
    private static int Dirt => _id == 0 ? Block.ConvertID(ID.DirtBlock) : _id; 
    
    public static void Run(Vector3Int CurrentCoordinate, Chunk CurrentChunk)
    {
        for (int x = 0; x < World.ChunkSize; x++)
        {
            for (int y = 0; y < World.ChunkSize; y++)
            {
                for (int z = 0; z <  World.ChunkSize; z++)
                {
                    if (
                        y + 1 < World.ChunkSize &&
                        CurrentChunk[x, y, z] != 0 && 
                        CurrentChunk[x, y + 1, z] == 0) 
                    {
       
                        double rng = Random.NextDouble();
                        if (CurrentChunk[x, y, z] == Dirt)
                        {
                            { 
                                if (rng <= 0.02)
                                {
                                    CurrentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Tree, CurrentCoordinate + new Vector3(x, y + 1, z)));
                                }
                                else if (rng <= 0.04)
                                {
                                    CurrentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Bush, CurrentCoordinate + new Vector3(x, y + 1, z)));
                                }
                                else if (rng <= 0.2)
                                {
                                    CurrentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Grass, CurrentCoordinate + new Vector3(x, y + 1, z)));
                                }
                            }
                        }
                        else
                        {
                            if (rng <= 0.0004)
                            {
                                CurrentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Chest, CurrentCoordinate + new Vector3(x, y + 1, z)));
                            } 
                            else if (rng <= 0.02)
                            {
                                CurrentChunk.StaticEntity.Add(Entity.CreateInfo(ID.Slab, CurrentCoordinate + new Vector3(x, y + 1, z)));
                            }
                        }
                    }
                }
            }
        }
    }
}