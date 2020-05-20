using Data;
using Events;
using Services.Interfaces;
using Zenject;

namespace Services
{
    public class InventoryService: IInitializable
    {
        private readonly SignalBus _signalBus;
        private readonly IGameDataProvider _gameData;

        public const int PipsPerDice = 6;
        private const int StartingMoney = 0;
        private const int StartingDice = 5;
        
        public int Pips { get; private set; }
        public int DiceCount { get; private set; }
        public int Money { get; private set; }
        
        public InventoryService(SignalBus signals, IGameDataProvider gameDataProvider)
        {
            _signalBus = signals;
            _gameData = gameDataProvider;
            Money = StartingMoney;
            DiceCount = StartingDice;
        }
        
        public void Initialize()
        {
           _signalBus.Subscribe<PipsGainedSignal>(OnPipsGained);
           _signalBus.Subscribe<BuildingPlacedSignal>(OnBuildingPlaced);
           _signalBus.Subscribe<DiceSpentSignal>(OnDiceSpent);
           _signalBus.Subscribe<InventoryBuildingPurchaseSignal>(OnHandleBuildingConstructionStart);
           _signalBus.Subscribe<InventoryBuildingPurchaseCompleteSignal>(OnHandleBuildingConstructionComplete);
        }

        private void OnPipsGained(PipsGainedSignal pipGainEvent)
        {
            Pips += pipGainEvent.pips;
            if (Pips > PipsPerDice)
            {
                int newDice = Pips / PipsPerDice;
                Pips = Pips - PipsPerDice * newDice;

                DiceCount += newDice;
            }

            BroadcastInventoryStatus();
        }

        private void OnBuildingPlaced(BuildingPlacedSignal buildingPlaced)
        {
            Money -= buildingPlaced.cost;
            BroadcastInventoryStatus();
        }
        
        private void OnDiceSpent(DiceSpentSignal diceSpent)
        {
            Money += diceSpent.moneyGained;
            DiceCount -= diceSpent.diceCount;

            BroadcastInventoryStatus();
        }

        private void BroadcastInventoryStatus()
        {
            _signalBus.Fire(new InventoryDiceChangeSignal
            {
                dicePips = Pips,
                diceCount = DiceCount,
                money = Money
            });
        }

        private void OnHandleBuildingConstructionStart(InventoryBuildingPurchaseSignal purchaseEvent)
        {
            BuildingData data = _gameData.GetDataById<BuildingData>(purchaseEvent.buildingId);
            if (data.moneyCost <= Money
                && data.diceCost <= DiceCount)
            {
                _signalBus.Fire(new InventoryBuildingPlacementSignal
                {
                    buildingId = purchaseEvent.buildingId
                });
            }
        }

        private void OnHandleBuildingConstructionComplete(InventoryBuildingPurchaseCompleteSignal purchaseEvent)
        {
            BuildingData data = _gameData.GetDataById<BuildingData>(purchaseEvent.buildingId);
            Money -= data.moneyCost;
            DiceCount -= data.diceCost;
            BroadcastInventoryStatus();
        }
    }
}