using System.Collections;
using UnityEngine;

public abstract class CraftingMachine: StructureMachine, IActionSecondaryInteract
{ 
    public CraftInfo GetCraftInfo()
    {
        if (Info is CraftInfo craftInfo)
            return craftInfo;

        ContainerInfo containerInfo = (ContainerInfo)Info;
        CraftInfo upgradedInfo = new CraftInfo()
        {
            id = containerInfo.id,
            position = containerInfo.position,
            Health = containerInfo.Health,
            threshold = containerInfo.threshold,
            SfxHit = containerInfo.SfxHit,
            SfxDestroy = containerInfo.SfxDestroy,
            Loot = containerInfo.Loot,
            operationType = containerInfo.operationType,
            Storage = containerInfo.Storage,
        };

        if (upgradedInfo.Storage != null)
            upgradedInfo.Storage.info = upgradedInfo;

        Modules.Clear();
        AddModule(upgradedInfo);
        return upgradedInfo;
    }

    public override void OnStart()
    {
        base.OnStart();
        AddState(new InCraftState());

        CraftInfo info = GetCraftInfo();

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
        Storage storage = new NoRefreshStorage(9);
        storage.CreateAndAddItem(ID.Spear);
        storage.CreateAndAddItem(ID.StonePickaxe); 
        storage.CreateAndAddItem(ID.StoneHatchet);
        storage.CreateAndAddItem(ID.Hammer); 
        return new CraftInfo()
        {
            Health = 500,
            Loot = ID.Workbench,
            SfxHit = SfxID.HitStone,
            SfxDestroy = SfxID.HitStone,
            Storage = storage
        };
    }
}