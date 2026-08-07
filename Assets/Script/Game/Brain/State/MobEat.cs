using UnityEngine;

/// <summary>Eating state: plays the swing animation, then on impact restores
/// hunger (and health if full) and consumes one food item.</summary>
class MobEat : MobState
{
    public MobEat() { updateMode = global::Module.UpdateMode.Everyone; }

    private Item _food;

    public override void OnEnterState()
    {
        Info.Animator.speed = Main.PlayerInfo == Info ? 0.7f : 0.3f;
        Info.SpriteToolEffect.localPosition = new Vector3(0.8f, -0.3f, 0);
        _food = Info.Equipment?.Info;
        Info.SpeedModifier = 0.25f;
        Info.Animator.Play("EquipSwingTelegraph", 0, 0f);
    }

    public override void OnUpdateState()
    {
        // Host processes eating for all entities; client only for owned entities.
        if (!Helper.IsHost() && !Info.IsOwner()) return;

        AnimatorStateInfo stateInfo = Info.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1f)
        {
            if (stateInfo.IsName("EquipSwingTelegraph"))
            {
                Info.Animator.speed = 1;
                Info.Animator.Play("EquipSwing", 0, 0f);
                Eat();
            }
            else if (stateInfo.IsName("EquipSwing"))
            {
                Info.Animator.speed = 1f;
                Audio.PlaySFX(SfxID.Item);
                Info.SpeedModifier = 0.8f;
                Info.Animator.Play("EquipSwingCooldown", 0, 0f);
            }
            else if (stateInfo.IsName("EquipSwingCooldown"))
            {
                Info.Animator.speed = 1f;
                Info.Animator.Play("EquipIdle", 0, 0f);
                Machine.SetState<DefaultState>();
            }
        }
    }

    private void Eat()
    {
        if (_food == null || _food.FoodValue <= 0) return;
        if (Info is not PlayerInfo player) return;

        // Restore hunger first; overflow goes to health.
        int hungerGain = Mathf.Min(_food.FoodValue, player.HungerMax - player.Hunger);
        player.Hunger += hungerGain;
        int overflow = _food.FoodValue - hungerGain;
        if (overflow > 0)
            player.Health = Mathf.Min(player.HealthMax, player.Health + overflow);

        // Consume one food item from the selected slot.
        if (player.Storage != null)
            player.Storage.RemoveItem(_food.ID, 1);

        GUIBar.Update();
    }

    public override void OnExitState()
    {
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipIdle", 0, 0f);
        Info.SpeedModifier = 1f;
    }
}
