 
using UnityEngine;

public class ItemPhysicsHandler : MonoBehaviour
{
    private GameObject _player;
    private PlayerDataSystem _playerDataSystem;
    private LayerMask _collisionLayer;
    private Vector3 _velocity;
    private float _deltaTime;
    private float GRAVITY = 50;  
    private float ESCAPE_VELOCITY = 8;  
    private float BOUNCE_THRESHOLD = 0.5f; 
    private float BOUNCE_FACTOR = 0.4f;  
    private float SLIDE_RANGE = 1;  
    private float PICKUP_RANGE = 1.5f;
    private float COLLISION_RANGE = 0.5f;  

    void Start()
    { 
        _player = GameObject.Find("player");
        _playerDataSystem = GameObject.Find("user_system").GetComponent<PlayerDataSystem>();
        _collisionLayer = LayerMask.GetMask("Collision"); 
    }

    public void PopItem() //initial pop velocity 
    {
        _velocity = new Vector3(Random.Range(-SLIDE_RANGE, SLIDE_RANGE), 0.3f, Random.Range(-SLIDE_RANGE, SLIDE_RANGE));  
    }

    void Update()
    {
        // Move towards the player if within detection range 
        float playerDistance = Vector3.Distance(transform.position, _player.transform.position);
        
        if (Input.GetKeyDown(KeyCode.F) && playerDistance <= PICKUP_RANGE)
        {
            _playerDataSystem.AddItem(int.Parse(transform.name), 1);
            Destroy(gameObject);
        }
        else if (transform.position.y < -5) 
        {
            Destroy(gameObject);
        } 
        else HandlePhysics();
    }

    void HandlePhysics()
    {
        _deltaTime = GameSystem._deltaTime;

        _velocity += Vector3.down * GRAVITY * _deltaTime;
        _velocity.y = Mathf.Max(_velocity.y, -GRAVITY); 
        Vector3 newPosition = transform.position + _velocity * _deltaTime;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, COLLISION_RANGE, _velocity.normalized, out hit, _velocity.magnitude * Time.fixedDeltaTime, _collisionLayer))
        {
            Vector3 collisionNormal = hit.normal;
            _velocity = Vector3.Reflect(_velocity, collisionNormal) * BOUNCE_FACTOR;
            if (Mathf.Abs(_velocity.y) < BOUNCE_THRESHOLD)
            {
                _velocity = Vector3.zero;
            }
        }
        else
        {
            transform.position = newPosition;
        }

        // Check if the object is inside a block
        if (Physics.CheckSphere(transform.position, COLLISION_RANGE/1.5f, _collisionLayer))
        { 
            _velocity = Vector3.up * ESCAPE_VELOCITY;
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
