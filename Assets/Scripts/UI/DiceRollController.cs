using System.Collections;
using System.Collections.Generic;
using Events;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class DiceRollController : MonoBehaviour
    {
        [SerializeField]
        private int rollCount = 3;
        [SerializeField]
        private Button rollButton;
        [SerializeField]
        private TMPro.TextMeshProUGUI rollButtonText;
        [SerializeField]
        private Button[] holdButtons;
        [SerializeField]
        private SpriteRenderer[] holdIcons;
        [SerializeField]
        private Die_d6[] dice;
        [SerializeField]
        private int[] diceValues = new int[5];
        [SerializeField]
        private HashSet<int> held = new HashSet<int>();
        [SerializeField]
        private TextMeshProUGUI diceCountLabel;
        [SerializeField]
        private GameObject gameStartInstructions;
        [SerializeField]
        private float rollDuration = 3;
    
        private bool rolling = false;
        private bool playing = false;
        private bool nextRoll = false;

        private SignalBus _signals;

        private int _curDiceCount = 0;
        private int _curPipCount = 0;
        private bool _firstRoll = true;
    
        [Inject]
        public void Init(SignalBus signals, InventoryService inventoryService)
        {
            _signals = signals;
            _curDiceCount = inventoryService.DiceCount;
            _curPipCount = inventoryService.Pips;
            _signals.Subscribe<InventoryDiceChangeSignal>(OnPipChange);
        }

        private void OnPipChange(InventoryDiceChangeSignal pipEvent)
        {
            _curDiceCount = pipEvent.diceCount;
            _curPipCount = pipEvent.dicePips;
            diceCountLabel.text = _curDiceCount.ToString();
            if (!playing)
            {
                UpdatePlayButtonText();
            }
        }

        private void SetHoldButtonVisibility(bool active)
        {
            foreach (var button in holdButtons)
            {
                button.gameObject.SetActive(active);
            }
        }

        public void Update()
        {
            AdjustDiceSizes();
        }

        private void UpdatePlayButtonText()
        {
            if (_curDiceCount < 5)
            {
                rollButtonText.text = _curDiceCount + "/5 To Roll";
            }
            else if (_curDiceCount >= 5)
            {
                rollButtonText.text = "Play";
            }
        }

        private void AdjustDiceSizes()
        {
            if (playing)
            {
                return;
            }

            for (int i = 0; i < dice.Length; i++)
            {
                var die = dice[i];
                var dieTransform = die.transform;
            
                float targetScale;
                if (i < _curDiceCount)
                {
                    targetScale = 1;
                }
                else if( i == _curDiceCount)
                {
                    targetScale = _curPipCount/(float)InventoryService.PipsPerDice;
                }
                else
                {
                    targetScale = 0;
                }
            
                dieTransform.localScale = Vector3.one * targetScale;
            }
        }
    
        public void Start()
        {
            UpdatePlayButtonText();
            HideLocks();
            SetHoldButtonVisibility(false);
        }

        private void HideLocks()
        {
            foreach(var sprite in holdIcons)
            {
                sprite.gameObject.SetActive(false);
            }
        }
        public void Roll()
        {
        
            if (playing)
            {
                if (!rolling)
                {
                    nextRoll = true;
                }
                return;
            }
        
            if (_curDiceCount < 5)
            {
                return;
            }
            StartCoroutine(StartGame());
        }

        private int CalculateMoneyGainedFromDice()
        {
            int money = 0;
            foreach (var die in diceValues)
            {
                money += die;
            }

            return money;
        }
    
        private IEnumerator StartGame()
        {
            int money = 0;
            playing = true;
            SetHoldButtonVisibility(true);
            _signals.Fire(new DiceSpentSignal
            {
                diceCount = 5,
                moneyGained = 0,
            });
            for (int i = 0; i < rollCount; i++)
            {   
                rollButtonText.text = (rollCount - (i+1)) + " Rolls left ";
                yield return RollCoroutine();
                nextRoll = false;
                if (i == rollCount - 1)
                {
                    money = CalculateMoneyGainedFromDice();
                    rollButtonText.text = "Collect " + money;
                }
                yield return new WaitUntil(() => nextRoll);
            }
        
            _signals.Fire(new DiceSpentSignal
            {
                diceCount = 0,
                moneyGained = money,
            });
        
            SetHoldButtonVisibility(false);
            HideLocks();
            held.Clear();
            rollButtonText.text = "Play";
            playing = false;

            if (_firstRoll)
            {
                _firstRoll = false;
                gameStartInstructions.SetActive(false);
                _signals.Fire<StartBattleSignal>();
            }
        
        }

        public IEnumerator RollCoroutine()
        {
            rolling = true;
            rollButton.enabled = false;
            foreach (var holdButton in holdButtons)
            {
                holdButton.interactable = false;
            }
        
            for(int i =0; i < dice.Length ; i++)
            {
                if (held.Contains(i))
                {
                    continue;
                }
                var die = dice[i];
                die.Roll();
            }

            yield return new WaitForSeconds(rollDuration*.7f);

            float timeToWait = rollDuration*.3f;
            for(int i =0; i < dice.Length ; i++)
            {
                if (held.Contains(i))
                {
                    continue;
                }
                var die = dice[i];
                int side = Random.Range(1, 7);
                diceValues[i] = side;
                die.speed = 1 / timeToWait;
                die.ShowSide(side);
            }
            yield return new WaitForSeconds(timeToWait);
            rolling = false;

            rollButton.enabled = true;
            foreach (var holdButton in holdButtons)
            {
                holdButton.interactable = true;
            }
        }

        public void ToggleHold(int i)
        {
            if (rolling)
            {
                return;
            }
        
            if (held.Contains(i))
            {
                holdIcons[i].gameObject.SetActive(false);
                held.Remove(i);
            }
            else
            {
                holdIcons[i].gameObject.SetActive(true);
                held.Add(i);
            }
        }
    }
}
