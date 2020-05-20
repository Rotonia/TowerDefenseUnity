using Data;
using Events;
using Services;
using Services.Interfaces;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class BuildingConstructButton:MonoBehaviour
    {
        [SerializeField] 
        private TextMeshProUGUI nameLabel;
        [SerializeField] 
        private TextMeshProUGUI moneyCostLabel;
        [SerializeField] 
        private TextMeshProUGUI diceCostLabel;
        [SerializeField] 
        private GameObject moneyCostContainer;
        [SerializeField] 
        private GameObject diceCostContainer;
        
        private BuildingData data;

        private IGameDataProvider _gameData;
        private SignalBus _signalBus;

        private int _curMoney;
        private int _curDice;

        private bool paused;
        
        [Inject]
        public void Init(IGameDataProvider gameData, SignalBus signalBus, InventoryService inventoryService)
        {
            _gameData = gameData;
            _signalBus = signalBus;
            _signalBus.Subscribe<InventoryDiceChangeSignal>(OnInventoryChange);
            _signalBus.Subscribe<StartBattleSignal>(OnBattleStart);
            _curDice = inventoryService.DiceCount;
            _curMoney = inventoryService.Money;
        }

        public void Setup(int buildingId)
        {
           data = _gameData.GetDataById<BuildingData>(buildingId);
           moneyCostContainer.SetActive(data.moneyCost > 0);
           nameLabel.text = data.nameInHierarchy;
           UpdateUIState();
        }

        private void OnBattleStart()
        {
            paused = false;
        }
        
        private void OnInventoryChange(InventoryDiceChangeSignal inventoryDiceChangeSignal)
        {
            _curMoney = inventoryDiceChangeSignal.money;
            _curDice = inventoryDiceChangeSignal.diceCount;
            UpdateUIState();
        }
        
        private void UpdateUIState()
        {
            SetupCostContainer(data.moneyCost, _curMoney, moneyCostContainer, moneyCostLabel);
            SetupCostContainer(data.diceCost, _curDice, diceCostContainer, diceCostLabel);
        }

        private void SetupCostContainer(int cost, int curVal, GameObject container, TextMeshProUGUI costLabel)
        {
            if (cost <= 0)
            {
                container.SetActive(false);
                return;
            }
            container.SetActive(true);
            costLabel.text = cost.ToString();
            costLabel.color = curVal >= cost ? Color.white : Color.gray;
        }

        public void Build()
        {
            if (paused)
            {
                return;
            }
            
            if (data.moneyCost <= _curMoney
                && data.diceCost <= _curDice)
            {
                _signalBus.Fire( new InventoryBuildingPurchaseSignal
                {
                    buildingId = data.Id
                });
            }
        }
    }
}