[System.Serializable]
public class BlockData
{ 
    public string StringID { get; set; }
    public int BreakThreshold { get; set; }
    public int BreakCost { get; set; }

    public BlockData(string stringID, int breakThreshold, int breakCost)
    { 
        StringID = stringID;
        BreakThreshold = breakThreshold;
        BreakCost = breakCost;
    }
}