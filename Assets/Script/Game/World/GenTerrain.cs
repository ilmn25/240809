// using UnityEngine;
//
// public abstract class Gen3D
// {  
//     public abstract bool Valid(int x, int y, int z);
// }
//
// public class GenCave : Gen3D
// {
//     private readonly float _scale = 0.06f;
//     private readonly float _offset = (float)WorldGen.Random.NextDouble() * 1000;
//     public override bool Valid(int x, int y, int z)
//     {
//         float caveNoiseValue = WorldGen.Generate3DPerlinNoise(x, y, z, _scale, _offset);
//         return caveNoiseValue < 0.35f;
//     }
// }
//
// public class GenCrater 
// {
//     private readonly float _scale = 0.06f;
//     private readonly int _centerX = 100;
//     private readonly int _centerZ = 100;
//     private const int CraterRadius = 35;
//     private readonly float _offset = (float)WorldGen.Random.NextDouble() * 1000;
//     private float _value;
//     private float _distanceFromCenter;
//     
//     public void Set(int x, int z)
//     {
//         int distanceX = Mathf.Abs(_centerX - x);
//         int distanceZ = Mathf.Abs(_centerZ - z);
//         _distanceFromCenter = Mathf.Sqrt(distanceX * distanceX + distanceZ * distanceZ);
//         
//         _value = Mathf.PerlinNoise(x * _scale + _offset, z * _scale + _offset) * 2.0f - 1.0f;;
//     }
//
//     public bool Valid(int y)
//     { 
//         float taperedRadius = CraterRadius * ((float)y / WorldGen.WorldHeight) + _value;
//
//         return _distanceFromCenter <= taperedRadius;;
//     }
// }
//
// public abstract class GenTerrain
// {
//     private float _value; 
//     public int ID;
//     protected float Scale;
//     private readonly float _offset = (float)WorldGen.Random.NextDouble() * 1000;
//     public void Set(int x, int z)
//     {
//         _value = GetHeight(WorldGen.GeneratePerlinNoise(x, z, Scale, _offset));
//     }
//
//     protected abstract float GetHeight(float value);
//     
//     // public void Load(Chunk chunk)
//     // {
//     //     for (int y = 0; y < Mathf.Clamp(_value - chunk.Position.y, 0, World.ChunkSize); y++)
//     //     {
//     //         chunk[_position.x, y, _position.y] = ID;
//     //     }
//     // }
//
//     public bool Valid(int y)
//     {
//         return y < _value;
//     }
// }
//
// public class GenMarble : GenTerrain
// {
//     public GenMarble()
//     {
//         Scale = 0.007f;
//         ID = Block.ConvertID("marble");
//     }
//
//     protected override float GetHeight(float value)
//     {
//         return  value * WorldGen.WorldHeight / 4 + WorldGen.WorldHeight * 3 / 4 - 50;
//     }
//  
// }
//
// public class GenStone : GenTerrain
// {
//     public GenStone()
//     {
//         Scale = 0.01f;
//         ID = Block.ConvertID("stone");
//     }
//
//     protected override float GetHeight(float value)
//     {
//         return value * WorldGen.WorldHeight / 4 + WorldGen.WorldHeight * 3 / 4 - 5;
//     } 
// }
//
// public class GenDirt : GenTerrain
// {
//     public GenDirt()
//     {
//         Scale = 0.005f;
//         ID = Block.ConvertID("dirt");
//     }
//
//     protected override float GetHeight(float value)
//     {
//         return value * WorldGen.WorldHeight / 4 + WorldGen.WorldHeight * 3 / 4;
//     } 
// }
//
// public class GenSand : GenTerrain
// {
//     public GenSand() {
//         Scale = 0.02f;
//         ID = Block.ConvertID("sand");
//     }
//
//     protected override float GetHeight(float value)
//     {
//         return value * WorldGen.WorldHeight / 4 + WorldGen.WorldHeight * 3 / 4;
//     } 
// }