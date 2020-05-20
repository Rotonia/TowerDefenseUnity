using Core;
using Data;
using Events;
using Services.Interfaces;
using UnityEngine;
using Zenject;

namespace Actors
{
    /*
     * The actor class is intended to be a base implementation that provides core functionality and management
     * to any of interacting game play elements.
     */
    public abstract class Actor<TV, TD>
        : FactoriedMonoBehaviour<TV>, ITargetable
        where TV : IActorSpawnSettings
        where TD : GameDataBase
    {
        protected TD actorData { get; private set; }

        protected IGameDataProvider GameData { get; private set; }
        protected IAssetLoader AssetLoader { get; private set; }
        protected SignalBus SignalBus { get; private set; }
        protected bool paused = false;
        public string Category => actorData.category;

        [Inject]
        public void InitDi(
            IGameDataProvider gameDataProvider, 
            IAssetLoader assetLoader, 
            SignalBus signalBus)
        {
            GameData = gameDataProvider;
            this.AssetLoader = assetLoader;
            this.SignalBus = signalBus;
        }
        
        public class SpawnSettings : IActorSpawnSettings
        {
            public int dataId { get; set; }
            public int Team { get; set; }
        }
        
        public int Team()
        {
            return settings != null ? settings.Team : 0;
        }

        public bool IsPaused()
        {
            return paused;
        }
        
        public virtual void Pause()
        {
            if (!paused)
            {
                paused = true;
                OnPause();
            }
        }
        public virtual void Resume()
        {
            if (paused)
            {
                paused = false;
                OnResume();
            }
        }

        protected virtual void OnPause()
        {
            
        }

        protected virtual void OnResume()
        {
            
        }
        

        public virtual bool InRange(Vector3 pos, float range, out Vector3 closestPoint, out float distanceBetween)
        {
            closestPoint = Vector3.zero;
            distanceBetween = 0;
            return false;
        }
        

        public virtual Transform GetTransform()
        {
            return this.transform;
        }
        
        public virtual bool IsAlive()
        {
            return true;
        }

        protected override void Spawning()
        {
            GetActorData();
            SetGameObjectName();
            SignalBus.Fire(new ActorCreatedSignal
            {
                target  =  this
            });
        }

        private void GetActorData()
        {
            actorData = GameData.GetDataById<TD>(settings.dataId);
        }

        private void SetGameObjectName()
        {
            this.gameObject.name = actorData.nameInHierarchy;
        }

        protected override void Despawning()
        {
            SignalBus.Fire(new ActorRemovedSignal
            {
                target  =  this
            });
        }
        
        public abstract void OnHit(int damage, Vector3 knockBack);
    }
}