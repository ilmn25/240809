using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public class ChunkData
{
    public int[,,] Map { get; private set; }
    public List<EntityData> StaticEntity { get; private set; }
    public List<EntityData> DynamicEntity { get; private set; }

    public static ChunkData Zero { get; private set; }

    static ChunkData()
    {
        Zero = new ChunkData();
        for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
        {
            for (int y = 0; y < WorldStatic.CHUNKSIZE; y++)
            {
                for (int z = 0; z < WorldStatic.CHUNKSIZE; z++)
                {
                    Zero.Map[x, y, z] = 0;
                }
            }
        }
    }

    public ChunkData()
    {
        Map = new int[WorldStatic.CHUNKSIZE, WorldStatic.CHUNKSIZE, WorldStatic.CHUNKSIZE];
        StaticEntity = new List<EntityData>();
        DynamicEntity = new List<EntityData>();
    }
}


public class ChunkMap
{
    private static NativeMap3D<int> _map;
    private static int _size = WorldStatic.CHUNKSIZE + 2; 
    public static NativeMap3D<int> Create(Vector3Int coordinate)
    {
        _map = new NativeMap3D<int>(_size, Allocator.TempJob);

        // Call methods to set faces
        SetPX(coordinate);
        SetNX(coordinate);
        SetPY(coordinate);
        SetNY(coordinate);
        SetPZ(coordinate);
        SetNZ(coordinate);

        // Call methods to set edges
        SetEdgePXNY(coordinate);
        SetEdgePXPY(coordinate);
        SetEdgePXPZ(coordinate);
        SetEdgePXNZ(coordinate);
        SetEdgeNXNY(coordinate);
        SetEdgeNXPY(coordinate);
        SetEdgeNXPZ(coordinate);
        SetEdgeNXNZ(coordinate);
        SetEdgePYNZ(coordinate);
        SetEdgePYPZ(coordinate);
        SetEdgeNYNZ(coordinate);
        SetEdgeNYPZ(coordinate);

        // Call methods to set corners
        SetCornerNNN(coordinate);
        SetCornerNNP(coordinate);
        SetCornerNPN(coordinate);
        SetCornerNPP(coordinate);
        SetCornerPNN(coordinate);
        SetCornerPNP(coordinate);
        SetCornerPPN(coordinate);
        SetCornerPPP(coordinate);
        
        SetMiddle(coordinate); 
        return _map;
    }
 
 
    
    public static void SetMiddle(Vector3Int coordinate)
    { 
        ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y, coordinate.z];
        for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
        {
            for (int y = 0; y < WorldStatic.CHUNKSIZE; y++)
            {
                for (int z = 0; z < WorldStatic.CHUNKSIZE; z++)
                {
                    _map[x, y, z] = chunkData.Map[x, y, z];
                }
            }
        }
    }

    public static void SetPX(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNKSIZE, coordinate.y, coordinate.z];
        for (int y = 0; y < WorldStatic.CHUNKSIZE; y++)
        {
            for (int z = 0; z < WorldStatic.CHUNKSIZE; z++)
            {
                _map[WorldStatic.CHUNKSIZE, y, z] = chunkData.Map[0, y, z];
            }
        }
    }

    public static void SetNX(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNKSIZE, coordinate.y, coordinate.z];
        for (int y = 0; y < WorldStatic.CHUNKSIZE; y++)
        {
            for (int z = 0; z < WorldStatic.CHUNKSIZE; z++)
            {
                _map[-1, y, z] = chunkData.Map[WorldStatic.CHUNKSIZE - 1, y, z];
            }
        }
    }

    public static void SetPY(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y + WorldStatic.CHUNKSIZE, coordinate.z];
        for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
        {
            for (int z = 0; z < WorldStatic.CHUNKSIZE; z++)
            {
                _map[x, WorldStatic.CHUNKSIZE, z] = chunkData.Map[x, 0, z];
            }
        }
    }

    public static void SetNY(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y - WorldStatic.CHUNKSIZE, coordinate.z];
        for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
        {
            for (int z = 0; z < WorldStatic.CHUNKSIZE; z++)
            {
                _map[x, -1, z] = chunkData.Map[x, WorldStatic.CHUNKSIZE - 1, z];
            }
        }
    }

    public static void SetPZ(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y, coordinate.z + WorldStatic.CHUNKSIZE];
        for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
        {
            for (int y = 0; y < WorldStatic.CHUNKSIZE; y++)
            {
                _map[x, y, WorldStatic.CHUNKSIZE] = chunkData.Map[x, y, 0];
            }
        }
    }

    public static void SetNZ(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y, coordinate.z - WorldStatic.CHUNKSIZE];
        for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
        {
            for (int y = 0; y < WorldStatic.CHUNKSIZE; y++)
            {
                _map[x, y, -1] = chunkData.Map[x, y, WorldStatic.CHUNKSIZE - 1];
            }
        }
    }
    // Methods to set the edges
    public static void SetEdgePXNY(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNKSIZE, coordinate.y - WorldStatic.CHUNKSIZE, coordinate.z];
        for (int z = 0; z < WorldStatic.CHUNKSIZE; z++)
        {
            _map[WorldStatic.CHUNKSIZE, -1, z] = chunkData.Map[0, WorldStatic.CHUNKSIZE - 1, z];
        }
    }

    public static void SetEdgePXPY(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNKSIZE, coordinate.y + WorldStatic.CHUNKSIZE, coordinate.z];
        for (int z = 0; z < WorldStatic.CHUNKSIZE; z++)
        {
            _map[WorldStatic.CHUNKSIZE, WorldStatic.CHUNKSIZE, z] = chunkData.Map[0, 0, z];
        }
    }

    public static void SetEdgePXPZ(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNKSIZE, coordinate.y, coordinate.z + WorldStatic.CHUNKSIZE];
        for (int y = 0; y < WorldStatic.CHUNKSIZE; y++)
        {
            _map[WorldStatic.CHUNKSIZE, y, WorldStatic.CHUNKSIZE] = chunkData.Map[0, y, 0];
        }
    }

    public static void SetEdgePXNZ(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNKSIZE, coordinate.y, coordinate.z - WorldStatic.CHUNKSIZE];
        for (int y = 0; y < WorldStatic.CHUNKSIZE; y++)
        {
            _map[WorldStatic.CHUNKSIZE, y, -1] = chunkData.Map[0, y, WorldStatic.CHUNKSIZE - 1];
        }
    }

    public static void SetEdgeNXNY(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNKSIZE, coordinate.y - WorldStatic.CHUNKSIZE, coordinate.z];
        for (int z = 0; z < WorldStatic.CHUNKSIZE; z++)
        {
            _map[-1, -1, -1] = chunkData.Map[WorldStatic.CHUNKSIZE - 1, WorldStatic.CHUNKSIZE - 1, z];
        }
    }

    public static void SetEdgeNXPY(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNKSIZE, coordinate.y + WorldStatic.CHUNKSIZE, coordinate.z];
        for (int z = 0; z < WorldStatic.CHUNKSIZE; z++)
        {
            _map[-1, WorldStatic.CHUNKSIZE, z] = chunkData.Map[WorldStatic.CHUNKSIZE - 1, 0, z];
        }
    }

    public static void SetEdgeNXPZ(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNKSIZE, coordinate.y, coordinate.z + WorldStatic.CHUNKSIZE];
        for (int y = 0; y < WorldStatic.CHUNKSIZE; y++)
        {
            _map[-1, y, WorldStatic.CHUNKSIZE] = chunkData.Map[WorldStatic.CHUNKSIZE - 1, y, 0];
        }
    }

    public static void SetEdgeNXNZ(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNKSIZE, coordinate.y, coordinate.z - WorldStatic.CHUNKSIZE];
        for (int y = 0; y < WorldStatic.CHUNKSIZE; y++)
        {
            _map[-1, y, -1] = chunkData.Map[WorldStatic.CHUNKSIZE - 1, y, WorldStatic.CHUNKSIZE - 1];
        }
    }

    public static void SetEdgePYNZ(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y + WorldStatic.CHUNKSIZE, coordinate.z - WorldStatic.CHUNKSIZE];
        for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
        {
            _map[x, WorldStatic.CHUNKSIZE, -1] = chunkData.Map[x, 0, WorldStatic.CHUNKSIZE - 1];
        }
    }

    public static void SetEdgePYPZ(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y + WorldStatic.CHUNKSIZE, coordinate.z + WorldStatic.CHUNKSIZE];
        for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
        {
            _map[x, WorldStatic.CHUNKSIZE, WorldStatic.CHUNKSIZE] = chunkData.Map[x, 0, 0];
        }
    }

    public static void SetEdgeNYNZ(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y - WorldStatic.CHUNKSIZE, coordinate.z - WorldStatic.CHUNKSIZE];
        for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
        {
            _map[x, -1, -1] = chunkData.Map[x, WorldStatic.CHUNKSIZE - 1, WorldStatic.CHUNKSIZE - 1];
        }
    }

    public static void SetEdgeNYPZ(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y - WorldStatic.CHUNKSIZE, coordinate.z + WorldStatic.CHUNKSIZE];
        for (int x = 0; x < WorldStatic.CHUNKSIZE; x++)
        {
            _map[x, -1, WorldStatic.CHUNKSIZE] = chunkData.Map[x, WorldStatic.CHUNKSIZE - 1, 0];
        }
    }

    // Methods to set the corners
    public static void SetCornerNNN(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNKSIZE, coordinate.y - WorldStatic.CHUNKSIZE, coordinate.z - WorldStatic.CHUNKSIZE];
        _map[-1, -1, -1] = chunkData.Map[WorldStatic.CHUNKSIZE - 1, WorldStatic.CHUNKSIZE - 1, WorldStatic.CHUNKSIZE - 1];
    }

    public static void SetCornerNNP(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNKSIZE, coordinate.y - WorldStatic.CHUNKSIZE, coordinate.z + WorldStatic.CHUNKSIZE];
        _map[0, -1, WorldStatic.CHUNKSIZE] = chunkData.Map[WorldStatic.CHUNKSIZE - 1, WorldStatic.CHUNKSIZE - 1, 0];
    }

    public static void SetCornerNPN(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNKSIZE, coordinate.y + WorldStatic.CHUNKSIZE, coordinate.z - WorldStatic.CHUNKSIZE];
        _map[-1, WorldStatic.CHUNKSIZE, -1] = chunkData.Map[WorldStatic.CHUNKSIZE - 1, 0, WorldStatic.CHUNKSIZE - 1];
    }

    public static void SetCornerNPP(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNKSIZE, coordinate.y + WorldStatic.CHUNKSIZE, coordinate.z + WorldStatic.CHUNKSIZE];
        _map[-1, WorldStatic.CHUNKSIZE, WorldStatic.CHUNKSIZE] = chunkData.Map[WorldStatic.CHUNKSIZE - 1, 0, 0];
    }

    public static void SetCornerPNN(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNKSIZE, coordinate.y - WorldStatic.CHUNKSIZE, coordinate.z - WorldStatic.CHUNKSIZE];
        _map[WorldStatic.CHUNKSIZE, -1, -1] = chunkData.Map[0, WorldStatic.CHUNKSIZE - 1, WorldStatic.CHUNKSIZE - 1];
    }

    public static void SetCornerPNP(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNKSIZE, coordinate.y - WorldStatic.CHUNKSIZE, coordinate.z + WorldStatic.CHUNKSIZE];
        _map[WorldStatic.CHUNKSIZE, -1, WorldStatic.CHUNKSIZE] = chunkData.Map[0, WorldStatic.CHUNKSIZE - 1, 0];
    }

    public static void SetCornerPPN(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNKSIZE, coordinate.y + WorldStatic.CHUNKSIZE, coordinate.z - WorldStatic.CHUNKSIZE];
        _map[WorldStatic.CHUNKSIZE, WorldStatic.CHUNKSIZE, -1] = chunkData.Map[0, 0, WorldStatic.CHUNKSIZE - 1];
    }

    public static void SetCornerPPP(Vector3Int coordinate)
    {
        ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNKSIZE, coordinate.y + WorldStatic.CHUNKSIZE, coordinate.z + WorldStatic.CHUNKSIZE];
        _map[WorldStatic.CHUNKSIZE, WorldStatic.CHUNKSIZE, WorldStatic.CHUNKSIZE] = chunkData.Map[0, 0, 0];
    }
    
    
}
