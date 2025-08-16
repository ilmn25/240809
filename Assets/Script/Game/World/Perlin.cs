using UnityEngine;

public class DensityGenerator
{
    public float noiseScale = 0.1f;
    public float caveNoiseScale = 0.2f;
    public float threshold = 0.0f;

    // Sample density at a given 3D point
    public float GetDensity(Vector3 position)
    {
        // Base terrain noise (optional if you're layering terrain + caves)
        float baseNoise = Perlin3D(position.x * noiseScale, position.y * noiseScale, position.z * noiseScale);

        // Cave height control
        float caveMinHeight = 5;
        float caveMaxHeight = 65;
        float t = Mathf.InverseLerp(caveMinHeight, caveMaxHeight, position.y);
        float heightFalloff = 1f - Mathf.Pow(t, 3); // Strong falloff near surface

        // Cave noise
        float caveNoise = Perlin3D(position.x * caveNoiseScale, position.y * caveNoiseScale, position.z * caveNoiseScale);
        float caveThreshold = 0.6f;

        // Cave carving logic
        if (caveNoise * heightFalloff > caveThreshold)
            return -1f; // Carve out cave
        else
            return baseNoise; // Use base terrain density
    }


    // Custom 3D Perlin Noise using Unity's 2D PerlinNoise
    float Perlin3D(float x, float y, float z)
    {
        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float xz = Mathf.PerlinNoise(x, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zy = Mathf.PerlinNoise(z, y);
        float zx = Mathf.PerlinNoise(z, x);

        // Average the samples for pseudo-3D noise
        return (xy + yz + xz + yx + zy + zx) / 6f;
    }

    // Example usage: check if a voxel is solid
    public bool IsSolid(Vector3 position)
    {
        return GetDensity(position) > threshold;
    }
}