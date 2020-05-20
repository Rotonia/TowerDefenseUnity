using Actors;
using Data;
using UnityEngine;
using Zenject;

namespace Gameplay
{
    public class TurretSpawner : MonoBehaviour
    {
        [SerializeField]
        private int turretId;
    
        [SerializeField]
        private int buildingId;
    
        [SerializeField]
        private int team;
    
        private Actor<TurretActor.SpawnSettings, TurretData>.Factory _turretFactory;
        private Actor<BuildingActor.SpawnSettings, BuildingData>.Factory _buildingFactory;
    
        [Inject]
        public void Init(Actor<TurretActor.SpawnSettings, TurretData>.Factory turretFactory,
            Actor<BuildingActor.SpawnSettings, BuildingData>.Factory buildingFactory)
        {
            _turretFactory = turretFactory;
            _buildingFactory = buildingFactory;
        }
    
        public void Start()
        {
            if (buildingId > 0)
            {
                _buildingFactory.Create(new BuildingActor.SpawnSettings
                {
                    dataId = buildingId,
                    Team = team,
                    position = this.transform.position,
                    rotation = this.transform.rotation
                });
            }
            else if (turretId > 0)
            {
                _turretFactory.Create(new TurretActor.SpawnSettings
                {
                    dataId = turretId,
                    mountPoint = this.transform,
                    Team = team,
                });
            }
        }
    }
}
