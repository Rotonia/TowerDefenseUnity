using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Transform diceyTranslationContainer;
        [SerializeField] private Transform diceyText;
        [SerializeField] private Transform defenderText;
        [SerializeField] private CanvasGroup defenderButtonGroup;
        [SerializeField] private CanvasGroup fleeButtonGroup;
        [SerializeField] private Button defenderButton;
        [SerializeField] private Button fleeButton;
    
        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(IntroSequenceCoroutine());
        }

        public IEnumerator IntroSequenceCoroutine()
        {
            defenderButton.enabled = false;
            fleeButton.enabled = false;
            defenderButtonGroup.alpha = 0;
            fleeButtonGroup.alpha = 0;
            Vector3 diceyPos = diceyTranslationContainer.transform.position;
            diceyTranslationContainer.Translate(-Screen.width,0,0 );
            Vector3 defenderPos = defenderText.transform.position;
            defenderText.Translate(-Screen.width,0,0 );
            var diceyMove = diceyTranslationContainer.DOMove(diceyPos, 2);
            var diceyRotate = diceyText.DOLocalRotate(new Vector3(0, 0, 180), .33f).SetLoops(-1, LoopType.Incremental).SetRelative(true).SetEase(Ease.Linear);
            yield return diceyMove.WaitForCompletion();
            diceyRotate.Kill();
            diceyText.DOLocalRotate(Vector3.zero, 0);
            defenderText.DOMove(defenderPos, 2);
            var fadeIn = defenderButtonGroup.DOFade(1, 1f).SetDelay(2f);
            fleeButtonGroup.DOFade(1, 1f).SetDelay(2f);
            defenderButton.transform.DOPunchScale(Vector3.one * 1.15f, .25f).SetDelay(2.75f);
            yield return fadeIn.WaitForCompletion();
            defenderButton.enabled = true;
            fleeButton.enabled = true;
        }

        public void LoadGame()
        {
            SceneManager.LoadSceneAsync("DDBattle");
        }
    
        public void ExitGame()
        {
            Application.Quit();
        }

    
    }
}
