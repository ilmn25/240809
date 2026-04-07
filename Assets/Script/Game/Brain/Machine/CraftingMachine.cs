using System.Collections;
using UnityEngine;

public abstract class CraftingMachine: StructureMachine, IActionSecondaryInteract
{ 
    public override void OnStart()
    {
        base.OnStart();
        AddState(new InCraftState());

        CraftInfo info = (CraftInfo)Info;

        IEnumerator Enumerator()
        {
            while (gameObject.activeSelf)
            {
                yield return new WaitForSeconds(3);
                if (info.IsConverting())
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
            SetState<InCraftState>();
        else 
            SetState<DefaultState>();
    }
}
public class WorkbenchMachine: CraftingMachine
{
    public static Info CreateInfo()
    {
        return CraftInfo.CreateStructureInfo(ID.Workbench, 500, SfxID.HitStone, SfxID.HitStone);
    }
}