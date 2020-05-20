using UnityEngine;
using Zenject;

namespace Core
{
    public abstract class FactoriedMonoBehaviour<T>: MonoBehaviour, IPoolable<T, IMemoryPool> 
        where T:IActorSpawnSettings 
    {
        private IMemoryPool _pool;
        protected T settings { get; set; }
        
        public void Despawn()
        {
            if (_pool != null)
            {
                _pool.Despawn(this);   
            }
        }

        protected abstract void Despawning();
        public void OnDespawned()
        {
            Despawning();
            _pool = null;
        }

        protected abstract void Spawning();
        public void OnSpawned(T spawnSettings, IMemoryPool pool)
        {
            _pool = pool;
            settings = spawnSettings;
            Spawning();
        }
        
        
        public class Factory :  PlaceholderFactory<T, FactoriedMonoBehaviour<T>>
        { }
        
        public class ActorPool : MonoPoolableMemoryPool<T, IMemoryPool, FactoriedMonoBehaviour<T>>
        { }

        public static void BindActor(DiContainer container, int size, FactoriedMonoBehaviour<T> obj, string transformName)
        {
            container.BindFactory<T, FactoriedMonoBehaviour<T>, Factory>() 
                // We could just use FromMonoPoolableMemoryPool here instead, but
                // for IL2CPP to work we need our pool class to be used explicitly here
                .FromPoolableMemoryPool<T, FactoriedMonoBehaviour<T>, ActorPool>(poolBinder => poolBinder
                    // Spawn 20 right off the bat so that we don't incur spikes at runtime
                    .WithInitialSize(size)
                    // Bullets are simple enough that we don't need to make a subcontainer for them
                    // The logic can all just be in one class
                    .FromComponentInNewPrefab(obj)
                    .UnderTransformGroup(transformName)); 
        }
    }
}