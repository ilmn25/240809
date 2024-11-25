 
using UnityEngine;

public class ItemPhysicInst : MonoBehaviour
{
    public static ItemPhysicInst Instance { get; private set; }  
    
    private LayerMask _collisionLayer;
    private Vector3 _velocity;
    private float _deltaTime;
    private float GRAVITY = 35;  
    private float ESCAPE_VELOCITY = 5;  
    private float BOUNCE_THRESHOLD = 5f; 
    private float BOUNCE_FACTOR = 0.3f;  
    private float SLIDE_RANGE = 3;  
    private float PICKUP_RANGE = 0.8f;
    private float COLLISION_RANGE = 0.3f;  

    void Start()
    {
        Instance = this;
        _collisionLayer = LayerMask.GetMask("Collision"); 
        transform.position += new Vector3(0, 0.1f, 0);
        PopItem();
    }

    public void PopItem() //initial pop velocity 
    {
        _velocity = new Vector3(Random.Range(-SLIDE_RANGE, SLIDE_RANGE), 5f, Random.Range(-SLIDE_RANGE, SLIDE_RANGE));  
    }

    void Update()
    {
        // Move towards the player if within detection range 
        float playerDistance = Vector3.Distance(transform.position, Game.Player.transform.position);
        
        if (playerDistance <= PICKUP_RANGE)
        {
            PlayerInventoryStatic.AddItem(transform.GetComponent<EntityHandler>()._entityData.ID, 1);
            gameObject.GetComponent<EntityHandler>().WipeEntity();
        }
        else if (transform.position.y < -5) 
        {            
            gameObject.GetComponent<EntityHandler>().WipeEntity();
        } 
        else 
        {
            HandlePhysics();
        }
    }

    void HandlePhysics()
    {
        _deltaTime = GameStatic.GetDeltaTime();

        if (!IsMovable(transform.position))
        {
            transform.position += new Vector3(0, 5, 0) * _deltaTime;
            return;
        }

        _velocity += GRAVITY * _deltaTime * Vector3.down;
        _velocity.y = Mathf.Max(_velocity.y, -GRAVITY); 
        Vector3 newPosition = transform.position + _velocity * _deltaTime;
        
        if (IsMovable(newPosition))
        {
            transform.position = newPosition;
        }
        else
        { 
            _velocity = -_velocity * BOUNCE_FACTOR;
        }
        // RaycastHit hit;
        // if (Physics.SphereCast(transform.position, COLLISION_RANGE, _velocity.normalized, out hit, _velocity.magnitude * _deltaTime, _collisionLayer))
        // {
        //     if (Mathf.Abs(_velocity.y) > BOUNCE_THRESHOLD)
        //     {
        //         _velocity = Vector3.Reflect(_velocity, hit.normal) * BOUNCE_FACTOR; 
        //     } else 
        //     {
        //         _velocity = Vector3.zero;
        //     }
        // }
        // else
        // {
        //     if (IsMovable(newPosition))
        //     {
        //         transform.position = newPosition;
        //     } 
        // } 
    }
    
    Collider[] tempCollisionArray = new Collider[1];
    int collisionCount; 
    private bool IsMovable(Vector3 newPosition)
    {  
        // Define an array to store the results
        tempCollisionArray[0] = null;
        collisionCount = Physics.OverlapSphereNonAlloc(newPosition + new Vector3(0,0.2f,0), COLLISION_RANGE, tempCollisionArray, _collisionLayer);

        return !(collisionCount > 0);
    }
    
    //! DEBUG TOOLS
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.blue;
    //     Vector3 newPosition = transform.position + _velocity * Time.fixedDeltaTime;
    //     Gizmos.DrawWireSphere(newPosition, COLLISION_RANGE);
    // }
}
