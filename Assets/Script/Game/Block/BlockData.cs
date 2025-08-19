[System.Serializable]
public partial class Block
{ 
    public ID StringID { get; set; }
    public int BreakThreshold { get; set; }
    public int BreakCost { get; set; }

    public Block(ID stringID, int breakThreshold, int breakCost)
    { 
        StringID = stringID;
        BreakThreshold = breakThreshold;
        BreakCost = breakCost;
    }
}