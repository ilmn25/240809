[System.Serializable]
public class BlockData
{ 
    public string StringID { get; set; }
    public int BreakThreshold { get; set; }
    public int BreakCost { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public BlockData(string stringID, int breakThreshold, int breakCost, string name, string description)
    { 
        StringID = stringID;
        BreakThreshold = breakThreshold;
        BreakCost = breakCost;
        Name = name;
        Description = description;
    }
}