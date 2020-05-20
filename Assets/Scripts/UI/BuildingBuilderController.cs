using Data;
using Events;
using Services;
using Services.Interfaces;
using UnityEngine;
using Zenject;

namespace UI
{
    public class BuildingBuilderController : MonoBehaviour
    {
        [SerializeField] private BuildingConstructButton buttonPrefab;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private TMPro.TextMeshProUGUI moneyLabel;
        [SerializeField] private GameObject buildingPlacementInstructionContainer;
        
        private SignalBus _signals;
        private IGameDataProvider _gameData;
        private DiContainer _container;

        private int curMoneyCount = 0;
    
        [Inject]
        public void Init(SignalBus signals, InventoryService inventoryService, IGameDataProvider gameDataProvider, DiContainer container)
        {
            _gameData = gameDataProvider;
            _signals = signals;
            curMoneyCount = inventoryService.Money;
            _container = container;
            _signals.Subscribe<InventoryDiceChangeSignal>(OnPipChange);
            _signals.Subscribe<InventoryBuildingPlacementSignal>(OnBuildingPlacementStart);
            _signals.Subscribe<InventoryBuildingPurchaseCompleteSignal>(OnBuildingPlacementComplete);
            _signals.Subscribe<InventoryBuildingPurchaseCancelSignal>(OnBuildingPlacementComplete);
        }

        private void OnBuildingPlacementStart()
        {
            buildingPlacementInstructionContainer.SetActive(true);
        }
        private void OnBuildingPlacementComplete()
        {
            buildingPlacementInstructionContainer.SetActive(false);
        }
        
        private void CreateButtons()
        {
            foreach (var buildingData in _gameData.GetDataByType<BuildingData>())
            {
                GameObject gameObject = _container.InstantiatePrefab(buttonPrefab.gameObject);
                BuildingConstructButton buildingButton = gameObject.GetComponent<BuildingConstructButton>(); // _container.InstantiatePrefabForComponent<BuildingConstructButton>(buttonPrefab, buttonContainer);
                buildingButton.transform.SetParent(buttonContainer);
                buildingButton.Setup(buildingData.Id);
            }
        }

        public void Start()
        {
            buildingPlacementInstructionContainer.SetActive(false);
            CreateButtons();
            UpdateUI();
        }
        
        private void OnPipChange(InventoryDiceChangeSignal pipEvent)
        {
            curMoneyCount = pipEvent.money;
            UpdateUI();
        }

        private void UpdateUI()
        {
            moneyLabel.text = curMoneyCount.ToString();
        }
    }
}