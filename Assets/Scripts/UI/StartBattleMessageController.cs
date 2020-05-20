using System.Collections;
using DG.Tweening;
using Events;
using UnityEngine;
using Zenject;

namespace UI
{
    public class StartBattleMessageController : MonoBehaviour
    {
        [Inject]
        public void Inject(SignalBus signalBus)
        {
            signalBus.Subscribe<StartBattleSignal>(OnBattleStart);
        }

        public void Start()
        {
            this.gameObject.SetActive(false);
            this.transform.localScale = Vector3.zero;
        }
        
        private void OnBattleStart()
        {
            this.gameObject.SetActive(true);
            StartCoroutine(ShowMessageCoroutine());
        }

        private IEnumerator ShowMessageCoroutine()
        {
            var tween = this.transform.DOScale(1, 1f);
            yield return tween.WaitForCompletion();
            yield return new WaitForSeconds(.5f);
            tween = this.transform.DOScale(0, .5f);
            yield return tween.WaitForCompletion();
            this.gameObject.SetActive(false);
        }
    }
}