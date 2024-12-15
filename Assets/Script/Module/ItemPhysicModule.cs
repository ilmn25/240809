 
using UnityEngine;

public class ItemPhysicModule : Module
{
    private Vector3 _velocity;
    private float _deltaTime;
    
    private float GRAVITY = 35;  
    private float BOUNCE_FACTOR = 0.3f;  
    private int SLIDE_RANGE = 3;   
    private float COLLISION_RANGE = 0.3f;  
 
    public void PopItem() //spawn velocity 
    {
        _velocity = new Vector3(Random.Range(-SLIDE_RANGE, SLIDE_RANGE), 7, Random.Range(-SLIDE_RANGE, SLIDE_RANGE));  
    }
 
    public void HandlePhysicsUpdate()
    { 
        _deltaTime = Game.GetDeltaTime();

        if (!IsMovable(Machine.transform.position))
        {
            Machine.transform.position += new Vector3(0, 5, 0) * _deltaTime;
            return;
        }

        _velocity += GRAVITY * _deltaTime * Vector3.down;
        _velocity.y = Mathf.Max(_velocity.y, -GRAVITY); 
        Vector3 newPosition = Machine.transform.position + _velocity * _deltaTime;
        
        if (IsMovable(newPosition))
        {
            Machine.transform.position = newPosition;
        }
        else
        { 
            _velocity = -_velocity * BOUNCE_FACTOR;
        } 
    }
    
    Collider[] tempCollisionArray = new Collider[1];
    int collisionCount; 
    private bool IsMovable(Vector3 newPosition)
    {  
        // Define an array to store the results
        tempCollisionArray[0] = null;
        collisionCount = Physics.OverlapSphereNonAlloc(newPosition + new Vector3(0,0.2f,0), COLLISION_RANGE, tempCollisionArray, Game.MaskMapAndCollision);

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
