using Actors;
using Data;
using Events;
using Services;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField]
        private float spawnRate = 1.0f;
        [SerializeField]
        private float spawnStagger = 0.5f;
        [SerializeField]
        private Transform target;
        [SerializeField]
        private int creepId;
        [SerializeField]
        private float radius = 1f;
        [SerializeField]
        private bool paused = true;
    
        private Actor<CreepController.SpawnSettings, CreepData>.Factory _creepFactory;
        private float spawnTime = 0;
        
        [Inject]
        public void InitDi(Actor<CreepController.SpawnSettings, CreepData>.Factory creepFactory, SignalBus signalBus)
        {
            _creepFactory = creepFactory;
            signalBus.Subscribe<StartBattleSignal>(OnStartBattle);
        }

        public void OnStartBattle()
        {
            paused = false;
        }
    
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(this.transform.position, radius);
        }

        void Update()
        {
            if (paused)
            {
                return;
            }
        
            spawnTime += Time.deltaTime;
            if (spawnTime >= spawnRate)
            {
                Vector2 rndPos = UnityEngine.Random.insideUnitCircle* radius;
            
                spawnTime -= spawnRate + spawnStagger * Random.value;
                _creepFactory.Create( new CreepController.SpawnSettings(){
                    dataId = creepId,
                    Team = 1,
                    startPos = new Vector3(this.transform.position.x + rndPos.x, this.transform.position.y, this.transform.position.z + rndPos.y),
                    targetPos = this.target
                });
            }
        }
    }
}
