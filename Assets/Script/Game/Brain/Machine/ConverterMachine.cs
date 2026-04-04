using System.Collections;
using UnityEngine;

public abstract class ConverterMachine: StructureMachine, IActionSecondaryInteract
{ 
    public override void OnStart()
    {
        base.OnStart();
        AddState(new InConverterState());

        IEnumerator Enumerator()
        {
            while (gameObject.activeSelf)
            {
                yield return new WaitForSeconds(3);
                if (((ConverterInfo)Info).IsConverting())
                {
                    Particle.Create(transform.position, Particles.Smoke, false);
                    Particle.Create(transform.position, Particles.Fire, false);
                }
            }
        }

        StartCoroutine(Enumerator());
    } 

    public void OnActionSecondary(Info info)
    {
        if (IsCurrentState<DefaultState>())
            SetState<InConverterState>();
        else
        {
            SetState<DefaultState>();
        } 
    }
}