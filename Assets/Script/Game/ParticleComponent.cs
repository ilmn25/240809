using UnityEngine;
using System.Threading.Tasks;

public class ParticleComponent : MonoBehaviour
{
    private Animator _animator;
    private AnimatorStateInfo _stateInfo;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Spawn(Vector3 position, Particles id)
    {
        transform.position = position;
        transform.rotation = ViewPort.CurrentRotation;
        _animator.Play("Particle" + id, 0, 0f);;
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
