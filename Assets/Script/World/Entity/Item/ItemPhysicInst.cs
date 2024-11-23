 
using UnityEngine;

public class ItemPhysicInst : MonoBehaviour
{
    public static ItemPhysicInst Instance { get; private set; }  
    
    private LayerMask _collisionLayer;
    private Vector3 _velocity;
    private float _deltaTime;
    private float GRAVITY = 50;  
    private float ESCAPE_VELOCITY = 20;  
    private float BOUNCE_THRESHOLD = 0.5f; 
    private float BOUNCE_FACTOR = 0.4f;  
    private float SLIDE_RANGE = 1;  
    private float PICKUP_RANGE = 1.5f;
    private float COLLISION_RANGE = 0.3f;  

    void Start()
    {
        Instance = this;
        _collisionLayer = LayerMask.GetMask("Collision"); 
        transform.position = transform.position + new Vector3(0, 0.1f, 0);
        PopItem();
    }

    public void PopItem() //initial pop velocity 
    {
        _velocity = new Vector3(Random.Range(-SLIDE_RANGE, SLIDE_RANGE), 0.1f, Random.Range(-SLIDE_RANGE, SLIDE_RANGE));  
    }

    void Update()
    {
        // Move towards the player if within detection range 
        float playerDistance = Vector3.Distance(transform.position, Game.Player.transform.position);
        
        if (playerDistance <= PICKUP_RANGE)
        {
            PlayerDataStatic.Instance.AddItem(transform.GetComponent<EntityHandler>()._entityData.ID, 1);
            EntityPoolStatic.Instance.ReturnObject(gameObject);
        }
        else if (transform.position.y < -5) 
        {            
            EntityPoolStatic.Instance.ReturnObject(gameObject);
        } 
        else 
        {
            // HandlePhysics();s
        }
    }

    void HandlePhysics()
    {
        _deltaTime = GameStatic._deltaTime;

        // Check if the object is inside a block
        if (Physics.CheckSphere(transform.position, COLLISION_RANGE/1.5f, _collisionLayer))
        {   
            _velocity = Vector3.up * ESCAPE_VELOCITY;
            transform.position = transform.position + _velocity * _deltaTime; 
            return;
        }

        _velocity += Vector3.down * GRAVITY * _deltaTime;
        _velocity.y = Mathf.Max(_velocity.y, -GRAVITY); 
        Vector3 newPosition = transform.position + _velocity * _deltaTime;
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, COLLISION_RANGE, _velocity.normalized, out hit, _velocity.magnitude * Time.fixedDeltaTime, _collisionLayer))
        { 
            if (Mathf.Abs(_velocity.y) < BOUNCE_THRESHOLD)
            {
                _velocity = Vector3.Reflect(_velocity, hit.normal) * BOUNCE_FACTOR; 
            } else 
            {
                _velocity = Vector3.zero;
            }
        }
        else
        {
            transform.position = newPosition;
        } 
    }
    
    //! DEBUG TOOLS
    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.blue;
    //     Vector3 newPosition = transform.position + _velocity * Time.fixedDeltaTime;
    //     Gizmos.DrawWireSphere(newPosition, COLLISION_RANGE);
    // }
}
