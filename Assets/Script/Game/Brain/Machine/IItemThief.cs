/// <summary>A mob whose hits knock the player's held item to the ground (so an
/// accomplice can grab it). Marking the machine lets the player's hit logic stay
/// generic instead of knowing each concrete thief type.</summary>
public interface IItemThief { }
