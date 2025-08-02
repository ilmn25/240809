[System.Serializable]
public partial class Block
{ 
    public string StringID { get; set; }
    public int BreakThreshold { get; set; }
    public int BreakCost { get; set; }

    public Block(string stringID, int breakThreshold, int breakCost)
    { 
        StringID = stringID;
        BreakThreshold = breakThreshold;
        BreakCost = breakCost;
    }
}