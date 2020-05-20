using System.Collections.Generic;
using Data;
using DefaultNamespace;
using Events;
using ModelData;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Actors
{
    using BuildingActorTypes = Actor<BuildingActor.SpawnSettings, BuildingData>;
    public class BuildingActor : BuildingActorTypes
    { 
        public new class SpawnSettings: BuildingActorTypes.SpawnSettings
        {
            public Vector3 position;
            public Quaternion rotation;
        }
        
        private BuildingModelData _modelData;
        private Actor<TurretActor.SpawnSettings, TurretData>.Factory _turretFactory;

        private List<TurretActor> _turrets = new List<TurretActor>();

        private int health;
        
        [Inject]
        public void Init(Actor<TurretActor.SpawnSettings, TurretData>.Factory turretFactory)
        {
            _turretFactory = turretFactory;
        }
        
        protected override void Spawning()
        {
            base.Spawning();
            health = actorData.health;
            transform.position = settings.position;
            transform.rotation = settings.rotation;
            AssetLoader.LoadBuildingAsset(actorData.buildingModelPrefabName, OnBuildingLoaded);
        }

        protected override void OnPause()
        {
            if (_modelData != null )
            {
                _modelData.obstacle.enabled = false;
            }
            foreach(var turret in _turrets)
            {
                turret.Pause();
            }
        }

        protected override void OnResume()
        {
            if (_modelData != null)
            {
                _modelData.obstacle.enabled = true;
            }
            
            foreach(var turret in _turrets)
            {
                turret.Resume();
            }
        }
        
        public NavMeshObstacle ObstacleBounds()
        {
            if (_modelData != null && _modelData.obstacle)
            {
                return _modelData.obstacle;
            }

            return null;
        }
        
        public override bool InRange(Vector3 pos, float range, out Vector3 closestPoint, out float distanceBetween)
        {
            if (_modelData != null)
            {
                return RangeCheckUtils.InRangeBounds(pos, _modelData.hitbox.bounds, range, out closestPoint,
                    out distanceBetween);
            }
            
            return RangeCheckUtils.InRangePos(pos, this.transform.position, range, out closestPoint,
                out distanceBetween);
        }

        protected override void Despawning()
        {
            foreach (var turret in _turrets)
            {
                turret.Despawn();
            }
            _turrets.Clear();

            if (_modelData != null)
            {
                Destroy(_modelData.gameObject);
                _modelData = null;
            }
        }
        
        public override bool IsAlive()
        {
            return health > 0;
        }
        
        public override void OnHit(int damage, Vector3 knockBack)
        {
            health -= damage;
            if (health <= 0)
            {   
                Despawn();
            }
        }

        private void SetupBuilding()
        {
            if (_modelData == null)
            {
                return;
            }

            int turretIndex = 0;
            foreach (var mountPoint in _modelData.turretMountPoints)
            {
                 var turret = _turretFactory.Create(new TurretActor.SpawnSettings
                 {
                    dataId = actorData.turretDefinitions[turretIndex],
                    mountPoint = mountPoint,
                    Team = Team()
                 })as TurretActor;
                 
                 if (turret == null)
                 {
                     continue;
                 }
                 
                 if (paused)
                 {
                     turret.Pause();
                 }
                 turretIndex++;
                 _turrets.Add(turret);
            }
        }

        private void OnBuildingLoaded(Object asset)
        {
            if (asset == null)
            {
                Debug.Log("Failed to load building with id: " + actorData.Id + " prefba:" + actorData.buildingModelPrefabName);
            }
            
            GameObject building = Instantiate(asset, this.transform) as GameObject;
            if (building != null)
            {
                _modelData = building.GetComponent<BuildingModelData>();
                if (paused)
                {
                    _modelData.obstacle.enabled = false;
                }
            }
            else
            {
                Debug.Log("Failed to instantiate building with id: " + actorData.Id + " prefba:" + actorData.buildingModelPrefabName);
            }

            SetupBuilding();
        }
    }
}