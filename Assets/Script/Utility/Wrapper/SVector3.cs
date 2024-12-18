using UnityEngine;

[System.Serializable]
public class SVector3
{
    public float x;
    public float y;
    public float z;

    public SVector3() {}
    public SVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SVector3(Vector3 vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
    }
 
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
}