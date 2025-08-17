 
using UnityEngine;

public class ItemPhysicModule : Module
{
    private Vector3 _velocity;
    private float _deltaTime;

    private const float Gravity = 35;
    private const float BounceFactor = 0.3f;
    private const int SlideRange = 2;
    private const float CollisionRange = 0.3f;

    public void PopItem() //spawn velocity 
    {
        _velocity = new Vector3(Random.Range(-SlideRange, SlideRange), 0, Random.Range(-SlideRange, SlideRange));  
    }
 
    public void HandlePhysicsUpdate()
    { 
        _deltaTime = Helper.GetDeltaTime();

        if (!IsMovable(Machine.transform.position))
        {
            Machine.transform.position += new Vector3(0, 5, 0) * _deltaTime;
            return;
        }

        _velocity += Gravity * _deltaTime * Vector3.down;
        _velocity.y = Mathf.Max(_velocity.y, -Gravity); 
        Vector3 newPosition = Machine.transform.position + _velocity * _deltaTime;
        
        if (IsMovable(newPosition))
        {
            Machine.transform.position = newPosition;
        }
        else
        { 
            _velocity = -_velocity * BounceFactor;
        } 
    }
    
    Collider[] tempCollisionArray = new Collider[1];
    int collisionCount; 
    private bool IsMovable(Vector3 newPosition)
    {  
        // Define an array to store the results
        tempCollisionArray[0] = null;
        collisionCount = Physics.OverlapSphereNonAlloc(newPosition + new Vector3(0,0.2f,0), CollisionRange, tempCollisionArray, Game.MaskStatic);

        return !(collisionCount > 0);
    }
    
    //! DEBUG TOOLS
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.blue;
    //     Vector3 newPosition = Machine.transform.position + _velocity * Time.fixedDeltaTime;
    //     Gizmos.DrawWireSphere(newPosition, COLLISION_RANGE);
    // }
}
