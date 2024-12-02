using UnityEngine;
using System.Threading.Tasks;

public class SmokeParticleModule : MonoBehaviour
{
    private Animator _animator;
    private AnimatorStateInfo _stateInfo;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SpawnSmoke(Vector3 position)
    {
        transform.position = position;
        _animator.Play(0, 0, 0);
        CheckFinish();
    }

    private async void CheckFinish()
    {
        _stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        while (_stateInfo.normalizedTime < 1 || _animator.IsInTransition(0))
        {
            await Task.Delay(100); // Check every 100 milliseconds
            if (this != null)
            {
                _stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            }
            else return; // Exit if the object has been destroyed
        }
        if (this != null)
        {
            transform.position = new Vector3(0, 500, 0);
            SmokeParticleSingleton.ReturnSmokeParticleToPool(gameObject);
        }
    }
}
