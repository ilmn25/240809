using UnityEngine;

public class MovementModule : Module
{
    public Vector3 Velocity = Vector3.zero;
    
    public void KnockBack(Vector3 position, float force, bool isAway)
    {
        Velocity += (isAway? Machine.transform.position - position : Machine.transform.position + position).normalized * force;
    }
}