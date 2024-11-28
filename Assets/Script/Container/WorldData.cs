using System;

[System.Serializable]
public class WorldData
{
    private ChunkData[,,] _world;  
    public SerializableVector3Int Length;
    public SerializableVector3Int Bounds;

    // Constructor to initialize the World array
    public WorldData(int x, int y, int z)
    {
        Bounds = new SerializableVector3Int(x * WorldStatic.CHUNKSIZE, y * WorldStatic.CHUNKSIZE, z * WorldStatic.CHUNKSIZE);
        Length = new SerializableVector3Int(x, y, z);
        _world = new ChunkData[x, y, z];
    }

    // Indexer to access the World array
    public ChunkData this[int x, int y, int z]
    {
        get
        {
            int chunkX = x / WorldStatic.CHUNKSIZE;
            int chunkY = y / WorldStatic.CHUNKSIZE;
            int chunkZ = z / WorldStatic.CHUNKSIZE;

            if (chunkX >= 0 && chunkX < _world.GetLength(0) &&
                chunkY >= 0 && chunkY < _world.GetLength(1) &&
                chunkZ >= 0 && chunkZ < _world.GetLength(2))
            {
                return _world[chunkX, chunkY, chunkZ];
            }
            else
            {
                return ChunkData.Zero;
            }
        }
        set
        {
            int chunkX = x / WorldStatic.CHUNKSIZE;
            int chunkY = y / WorldStatic.CHUNKSIZE;
            int chunkZ = z / WorldStatic.CHUNKSIZE; 
            if (chunkX >= 0 && chunkX < _world.GetLength(0) &&
                chunkY >= 0 && chunkY < _world.GetLength(1) &&
                chunkZ >= 0 && chunkZ < _world.GetLength(2))
            {
                _world[chunkX, chunkY, chunkZ] = value;
            }
            else
            {
                throw new IndexOutOfRangeException("Coordinates are out of bounds.");
            }
        }
    }
}