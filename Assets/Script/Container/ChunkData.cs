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
        for (int x = 0; x < WorldStatic.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < WorldStatic.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < WorldStatic.CHUNK_SIZE; z++)
                {
                    Zero.Map[x, y, z] = 0;
                }
            }
        }
    }

    public ChunkData()
    {
        Map = new int[WorldStatic.CHUNK_SIZE, WorldStatic.CHUNK_SIZE, WorldStatic.CHUNK_SIZE];
        StaticEntity = new List<EntityData>();
        DynamicEntity = new List<EntityData>();
    }
}

public class ChunkMap
{
    private static int _size = WorldStatic.CHUNK_SIZE + 2;

    public static NativeMap3D<int> Create(Vector3Int coordinate)
    {
        NativeMap3D<int> _map = new NativeMap3D<int>(_size, Allocator.TempJob);

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



        void SetMiddle(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y, coordinate.z];
            for (int x = 0; x < WorldStatic.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < WorldStatic.CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < WorldStatic.CHUNK_SIZE; z++)
                    {
                        _map[x, y, z] = chunkData.Map[x, y, z];
                    }
                }
            }
        }

        void SetPX(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNK_SIZE, coordinate.y, coordinate.z];
            for (int y = 0; y < WorldStatic.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < WorldStatic.CHUNK_SIZE; z++)
                {
                    _map[WorldStatic.CHUNK_SIZE, y, z] = chunkData.Map[0, y, z];
                }
            }
        }

        void SetNX(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNK_SIZE, coordinate.y, coordinate.z];
            for (int y = 0; y < WorldStatic.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < WorldStatic.CHUNK_SIZE; z++)
                {
                    _map[-1, y, z] = chunkData.Map[WorldStatic.CHUNK_SIZE - 1, y, z];
                }
            }
        }

        void SetPY(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y + WorldStatic.CHUNK_SIZE, coordinate.z];
            for (int x = 0; x < WorldStatic.CHUNK_SIZE; x++)
            {
                for (int z = 0; z < WorldStatic.CHUNK_SIZE; z++)
                {
                    _map[x, WorldStatic.CHUNK_SIZE, z] = chunkData.Map[x, 0, z];
                }
            }
        }

        void SetNY(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y - WorldStatic.CHUNK_SIZE, coordinate.z];
            for (int x = 0; x < WorldStatic.CHUNK_SIZE; x++)
            {
                for (int z = 0; z < WorldStatic.CHUNK_SIZE; z++)
                {
                    _map[x, -1, z] = chunkData.Map[x, WorldStatic.CHUNK_SIZE - 1, z];
                }
            }
        }

        void SetPZ(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y, coordinate.z + WorldStatic.CHUNK_SIZE];
            for (int x = 0; x < WorldStatic.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < WorldStatic.CHUNK_SIZE; y++)
                {
                    _map[x, y, WorldStatic.CHUNK_SIZE] = chunkData.Map[x, y, 0];
                }
            }
        }

        void SetNZ(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y, coordinate.z - WorldStatic.CHUNK_SIZE];
            for (int x = 0; x < WorldStatic.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < WorldStatic.CHUNK_SIZE; y++)
                {
                    _map[x, y, -1] = chunkData.Map[x, y, WorldStatic.CHUNK_SIZE - 1];
                }
            }
        }

        // Methods to set the edges
        void SetEdgePXNY(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNK_SIZE,
                coordinate.y - WorldStatic.CHUNK_SIZE, coordinate.z];
            for (int z = 0; z < WorldStatic.CHUNK_SIZE; z++)
            {
                _map[WorldStatic.CHUNK_SIZE, -1, z] = chunkData.Map[0, WorldStatic.CHUNK_SIZE - 1, z];
            }
        }

        void SetEdgePXPY(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNK_SIZE,
                coordinate.y + WorldStatic.CHUNK_SIZE, coordinate.z];
            for (int z = 0; z < WorldStatic.CHUNK_SIZE; z++)
            {
                _map[WorldStatic.CHUNK_SIZE, WorldStatic.CHUNK_SIZE, z] = chunkData.Map[0, 0, z];
            }
        }

        void SetEdgePXPZ(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNK_SIZE, coordinate.y,
                coordinate.z + WorldStatic.CHUNK_SIZE];
            for (int y = 0; y < WorldStatic.CHUNK_SIZE; y++)
            {
                _map[WorldStatic.CHUNK_SIZE, y, WorldStatic.CHUNK_SIZE] = chunkData.Map[0, y, 0];
            }
        }

        void SetEdgePXNZ(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNK_SIZE, coordinate.y,
                coordinate.z - WorldStatic.CHUNK_SIZE];
            for (int y = 0; y < WorldStatic.CHUNK_SIZE; y++)
            {
                _map[WorldStatic.CHUNK_SIZE, y, -1] = chunkData.Map[0, y, WorldStatic.CHUNK_SIZE - 1];
            }
        }

        void SetEdgeNXNY(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNK_SIZE,
                coordinate.y - WorldStatic.CHUNK_SIZE, coordinate.z];
            for (int z = 0; z < WorldStatic.CHUNK_SIZE; z++)
            {
                _map[-1, -1, -1] = chunkData.Map[WorldStatic.CHUNK_SIZE - 1, WorldStatic.CHUNK_SIZE - 1, z];
            }
        }

        void SetEdgeNXPY(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNK_SIZE,
                coordinate.y + WorldStatic.CHUNK_SIZE, coordinate.z];
            for (int z = 0; z < WorldStatic.CHUNK_SIZE; z++)
            {
                _map[-1, WorldStatic.CHUNK_SIZE, z] = chunkData.Map[WorldStatic.CHUNK_SIZE - 1, 0, z];
            }
        }

        void SetEdgeNXPZ(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNK_SIZE, coordinate.y,
                coordinate.z + WorldStatic.CHUNK_SIZE];
            for (int y = 0; y < WorldStatic.CHUNK_SIZE; y++)
            {
                _map[-1, y, WorldStatic.CHUNK_SIZE] = chunkData.Map[WorldStatic.CHUNK_SIZE - 1, y, 0];
            }
        }

        void SetEdgeNXNZ(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNK_SIZE, coordinate.y,
                coordinate.z - WorldStatic.CHUNK_SIZE];
            for (int y = 0; y < WorldStatic.CHUNK_SIZE; y++)
            {
                _map[-1, y, -1] = chunkData.Map[WorldStatic.CHUNK_SIZE - 1, y, WorldStatic.CHUNK_SIZE - 1];
            }
        }

        void SetEdgePYNZ(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y + WorldStatic.CHUNK_SIZE,
                coordinate.z - WorldStatic.CHUNK_SIZE];
            for (int x = 0; x < WorldStatic.CHUNK_SIZE; x++)
            {
                _map[x, WorldStatic.CHUNK_SIZE, -1] = chunkData.Map[x, 0, WorldStatic.CHUNK_SIZE - 1];
            }
        }

        void SetEdgePYPZ(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y + WorldStatic.CHUNK_SIZE,
                coordinate.z + WorldStatic.CHUNK_SIZE];
            for (int x = 0; x < WorldStatic.CHUNK_SIZE; x++)
            {
                _map[x, WorldStatic.CHUNK_SIZE, WorldStatic.CHUNK_SIZE] = chunkData.Map[x, 0, 0];
            }
        }

        void SetEdgeNYNZ(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y - WorldStatic.CHUNK_SIZE,
                coordinate.z - WorldStatic.CHUNK_SIZE];
            for (int x = 0; x < WorldStatic.CHUNK_SIZE; x++)
            {
                _map[x, -1, -1] = chunkData.Map[x, WorldStatic.CHUNK_SIZE - 1, WorldStatic.CHUNK_SIZE - 1];
            }
        }

        void SetEdgeNYPZ(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x, coordinate.y - WorldStatic.CHUNK_SIZE,
                coordinate.z + WorldStatic.CHUNK_SIZE];
            for (int x = 0; x < WorldStatic.CHUNK_SIZE; x++)
            {
                _map[x, -1, WorldStatic.CHUNK_SIZE] = chunkData.Map[x, WorldStatic.CHUNK_SIZE - 1, 0];
            }
        }

        // Methods to set the corners
        void SetCornerNNN(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNK_SIZE,
                coordinate.y - WorldStatic.CHUNK_SIZE, coordinate.z - WorldStatic.CHUNK_SIZE];
            _map[-1, -1, -1] = chunkData.Map[WorldStatic.CHUNK_SIZE - 1, WorldStatic.CHUNK_SIZE - 1,
                WorldStatic.CHUNK_SIZE - 1];
        }

        void SetCornerNNP(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNK_SIZE,
                coordinate.y - WorldStatic.CHUNK_SIZE, coordinate.z + WorldStatic.CHUNK_SIZE];
            _map[0, -1, WorldStatic.CHUNK_SIZE] =
                chunkData.Map[WorldStatic.CHUNK_SIZE - 1, WorldStatic.CHUNK_SIZE - 1, 0];
        }

        void SetCornerNPN(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNK_SIZE,
                coordinate.y + WorldStatic.CHUNK_SIZE, coordinate.z - WorldStatic.CHUNK_SIZE];
            _map[-1, WorldStatic.CHUNK_SIZE, -1] =
                chunkData.Map[WorldStatic.CHUNK_SIZE - 1, 0, WorldStatic.CHUNK_SIZE - 1];
        }

        void SetCornerNPP(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x - WorldStatic.CHUNK_SIZE,
                coordinate.y + WorldStatic.CHUNK_SIZE, coordinate.z + WorldStatic.CHUNK_SIZE];
            _map[-1, WorldStatic.CHUNK_SIZE, WorldStatic.CHUNK_SIZE] = chunkData.Map[WorldStatic.CHUNK_SIZE - 1, 0, 0];
        }

        void SetCornerPNN(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNK_SIZE,
                coordinate.y - WorldStatic.CHUNK_SIZE, coordinate.z - WorldStatic.CHUNK_SIZE];
            _map[WorldStatic.CHUNK_SIZE, -1, -1] =
                chunkData.Map[0, WorldStatic.CHUNK_SIZE - 1, WorldStatic.CHUNK_SIZE - 1];
        }

        void SetCornerPNP(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNK_SIZE,
                coordinate.y - WorldStatic.CHUNK_SIZE, coordinate.z + WorldStatic.CHUNK_SIZE];
            _map[WorldStatic.CHUNK_SIZE, -1, WorldStatic.CHUNK_SIZE] = chunkData.Map[0, WorldStatic.CHUNK_SIZE - 1, 0];
        }

        void SetCornerPPN(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNK_SIZE,
                coordinate.y + WorldStatic.CHUNK_SIZE, coordinate.z - WorldStatic.CHUNK_SIZE];
            _map[WorldStatic.CHUNK_SIZE, WorldStatic.CHUNK_SIZE, -1] = chunkData.Map[0, 0, WorldStatic.CHUNK_SIZE - 1];
        }

        void SetCornerPPP(Vector3Int coordinate)
        {
            ChunkData chunkData = WorldStatic.World[coordinate.x + WorldStatic.CHUNK_SIZE,
                coordinate.y + WorldStatic.CHUNK_SIZE, coordinate.z + WorldStatic.CHUNK_SIZE];
            _map[WorldStatic.CHUNK_SIZE, WorldStatic.CHUNK_SIZE, WorldStatic.CHUNK_SIZE] = chunkData.Map[0, 0, 0];
        }

    }
}
