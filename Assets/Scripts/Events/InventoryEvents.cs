namespace Events
{
    public struct InventoryBuildingPurchaseSignal
    {
        public int buildingId;
    }
    
    public struct InventoryBuildingPlacementSignal
    {
        public int buildingId;
    }

    public struct InventoryBuildingPurchaseCompleteSignal
    {
        public int buildingId;
    }
    
    public struct InventoryBuildingPurchaseCancelSignal
    {
        public int buildingId;
    }
    
    public struct InventoryDiceChangeSignal
    {
        public int dicePips;
        public int diceCount;
        public int money;
    }

    public struct BuildingPlacedSignal
    {
        public int cost;
    }
    
    public struct PipsGainedSignal
    {
       public int pips;
    }
    
    public struct DiceSpentSignal
    {
        public int diceCount;
        public int moneyGained;
    }
}