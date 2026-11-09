using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>A trophy hunter who pays for the remains of a single kind of beast. The
/// quarry — and the price — is chosen fresh on every arrival.</summary>
public class HunterMachine : RequestNPCMachine
{
    private readonly struct Quarry
    {
        public readonly ID Item;
        public readonly int Amount;
        public readonly int Coins;

        public Quarry(ID item, int amount, int coins)
        {
            Item = item;
            Amount = amount;
            Coins = coins;
        }
    }

    /// <summary>Mob drops the hunter buys: what they want, how many, and what they pay.</summary>
    private static readonly Quarry[] Quarries =
    {
        new Quarry(ID.SpiderWeb, 3, 30),
        new Quarry(ID.Foul, 2, 24),
        new Quarry(ID.Wool, 2, 30),
        new Quarry(ID.BloodBlack, 1, 40),
        new Quarry(ID.Cytoplasm, 2, 24),
        new Quarry(ID.Meat, 2, 20),
    };

    private Quarry _quarry;

    public static Info CreateInfo()
    {
        PassiveInfo info = PassiveInfo.CreateNpc();
        info.CharSprite = ID.Raider;
        return info;
    }

    public override void OnStart()
    {
        _quarry = Quarries[Random.Range(0, Quarries.Length)];
        base.OnStart();
    }

    private string QuarryName => Helper.ToDisplayName(_quarry.Item, true);

    public override string Announce => $"A hunter flags you down, looking for {QuarryName}...";
    protected override string Greeting => $"\"Bring me {_quarry.Amount} {QuarryName} and I'll pay {Trade.Format(_quarry.Coins)}.\"";
    protected override string Thanks => $"\"Fine trophies. Here's {Trade.Format(_quarry.Coins)}.\"";
    protected override ID RequestItem => _quarry.Item;
    protected override int RequestAmount => _quarry.Amount;
    protected override int RewardCoins => _quarry.Coins;
}