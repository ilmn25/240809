using UnityEngine;
using System.Threading.Tasks;

public class ParticleComponent : MonoBehaviour
{
    private Animator _animator;
    private AnimatorStateInfo _stateInfo;
    private Light _glow;
    private Transform _follow;
    private Vector3 _followOffset;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        // World-space particles have no anchor; anchored ones ride their entity.
        if (_follow) transform.position = _follow.position + _followOffset;
    }

    public void Spawn(Vector3 position, Transform follow, Particles id, int max)
    {
        _follow = follow;
        _followOffset = follow != null ? position - follow.position : Vector3.zero;
        transform.position = position;
        transform.rotation = ViewPort.CurrentRotation;
        if (_glow == null)
        {
            _glow = transform.Find("Sprite/Glow")?.GetComponent<Light>();
            WorldLights.Register(_glow);
        }
        if (_glow != null)
            _glow.enabled = id == Particles.Fire;
        string variation = max > 0 ? UnityEngine.Random.Range(1, max+1).ToString() : "";
        _animator.Play("Particle" + id + variation, 0, 0f);
        CheckFinish();
    }

    private async void CheckFinish()
    {
        _stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        while (_stateInfo.normalizedTime < 1 || _animator.IsInTransition(0))
        {
            await Task.Delay(100); // Check every 100 milliseconds
            if (this)
            {
                _stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            }
            else return; // Exit if the object has been destroyed
        }
        if (this)
        {
            transform.position = new Vector3(0, 500, 0);
            Particle.ReturnToPool(gameObject);
        }
    }
}
