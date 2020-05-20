using Actors;
using Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Gameplay
{
    public class GameEndController : MonoBehaviour
    {
        [SerializeField]
        private GameObject gameEndContainer;

        private SignalBus _signalBus;
    
        [Inject]
        public void Init(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
    
        public void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Creep"))
            {
                other.attachedRigidbody.gameObject.GetComponent<CreepController>().Despawn();
                _signalBus.Fire<EndBattleSignal>();
                gameEndContainer.SetActive(true);
            }
        }
    
        public void ReturnToMainMenu()
        {
            SceneManager.LoadSceneAsync("DDMain");
        }
    }
}
