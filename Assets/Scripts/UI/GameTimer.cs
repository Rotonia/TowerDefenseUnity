using Events;
using TMPro;
using UnityEngine;
using Zenject;

namespace UI
{
    public class GameTimer : MonoBehaviour
    {
        [SerializeField] 
        private TextMeshProUGUI timeText;
        private float startTime;
        private int lastTime;
    
        [Inject]
        public void Init(SignalBus signalBus)
        {
            signalBus.Subscribe<StartBattleSignal>(OnStartBattle);
            signalBus.Subscribe<EndBattleSignal>(OnEndBattle);
        }

        public void Start()
        {
            timeText.text = "";
        }

        private void OnStartBattle()
        {
            startTime = Time.timeSinceLevelLoad;
            UpdateTimer(false);
        }
        private void OnEndBattle()
        {
            startTime = 0;
        }

        void Update()
        {   
            UpdateTimer(false);
        }

        private void UpdateTimer(bool force)
        {
            if (startTime == 0)
            {
                return;
            }
        
            float timeElapsed = Time.timeSinceLevelLoad - startTime;
            if (!force && lastTime == (int) timeElapsed)
            {
                return;
            }

            lastTime = (int) timeElapsed;
            int minutes = (int) (timeElapsed / 60);
            int seconds = (int)(timeElapsed - minutes * 60);
            timeText.text = $"Time: {minutes:D2}:{seconds:D2}";
        }
    }
}
