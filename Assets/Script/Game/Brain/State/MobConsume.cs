using UnityEngine;

/// <summary>Consuming state: plays the swing animation, then on impact restores
/// hunger (and health if full) and consumes one consumable item.</summary>
class MobConsume : MobState
{
    public MobConsume() { updateMode = global::Module.UpdateMode.Everyone; }

    private Item _consumable;

    public override void OnEnterState()
    {
        Info.Animator.speed = Main.PlayerInfo == Info ? 0.7f : 0.3f;
        Info.SpriteToolEffect.localPosition = new Vector3(0.8f, -0.3f, 0);
        _consumable = Info.Equipment?.Info;
        Info.SpeedModifier = 0.25f;
        Info.Animator.Play("EquipSwingTelegraph", 0, 0f);
    }

    public override void OnUpdateState()
    {
        // Host processes consuming for all entities; client only for owned entities.
        if (!Helper.IsHost() && !Info.IsOwner()) return;

        AnimatorStateInfo stateInfo = Info.Animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime >= 1f)
        {
            if (stateInfo.IsName("EquipSwingTelegraph"))
            {
                Info.Animator.speed = 1;
                Info.Animator.Play("EquipSwing", 0, 0f);
                Consume();
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

    private void Consume()
    {
        if (_consumable == null ||
            (_consumable.HungerValue <= 0 && _consumable.HealValue <= 0 && _consumable.DamageValue <= 0 && _consumable.MaxHpBonus <= 0 && _consumable.MaxHungerBonus <= 0)) return;
        if (Info is not PlayerInfo player) return;

        Tutorial.OnConsume();

        // Permanent max-health boost (cradle of blood) — also heals for the same amount.
        if (_consumable.MaxHpBonus > 0)
        {
            player.BaseHealthMax += _consumable.MaxHpBonus;
            player.HealthMax += _consumable.MaxHpBonus;
            player.Health += _consumable.MaxHpBonus;
        }

        // Permanent max-hunger boost (horn of plenty) — also fills hunger for the same amount.
        if (_consumable.MaxHungerBonus > 0)
        {
            player.HungerMax += _consumable.MaxHungerBonus;
            player.Hunger += _consumable.MaxHungerBonus;
        }

        // Direct heal (bandages, cooked mushroom) applies first.
        if (_consumable.HealValue > 0)
            player.Health = Mathf.Min(player.HealthMax, player.Health + _consumable.HealValue);

        // Poisonous food (raw deathcap) hurts the eater.
        if (_consumable.DamageValue > 0)
        {
            player.Health -= _consumable.DamageValue;
            Audio.PlaySFX(SfxID.HitPlayer);
        }

        // Restore hunger first; overflow goes to health.
        int hungerGain = Mathf.Min(_consumable.HungerValue, player.HungerMax - player.Hunger);
        player.Hunger += hungerGain;
        int overflow = _consumable.HungerValue - hungerGain;
        if (overflow > 0)
            player.Health = Mathf.Min(player.HealthMax, player.Health + overflow);

        // Consume one consumable item from the held slot (cursor when non-empty, else hotbar).
        if (Inventory.CurrentItem != null && Inventory.CurrentItem.Stack > 0)
        {
            Inventory.CurrentItem.Stack--;
            if (Inventory.CurrentItem.Stack <= 0) Inventory.CurrentItem.clear();
            Inventory.RefreshInventory();
        }

        GUIBar.Update();
    }

    public override void OnExitState()
    {
        Info.Animator.speed = 1f;
        Info.Animator.Play("EquipIdle", 0, 0f);
        Info.SpeedModifier = 1f;
    }
}
