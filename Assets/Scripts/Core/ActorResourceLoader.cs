using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Core
{
    public interface IAssetLoader
    {
        void LoadAsset(string assetPath, Action<Object> assetCallback);
        void LoadProjectileAsset(string assetName, Action<Object> assetCallback);
        void LoadEffectAsset(string assetName, Action<Object> assetCallback);
        void LoadTurretAsset(string assetPath, Action<Object> assetCallback);
        void LoadBuildingAsset(string assetPath, Action<Object> assetCallback);
        void LoadWeaponAsset(string assetPath, Action<Object> assetCallback);
        void LoadCreepAsset(string assetPath, Action<Object> assetCallback);
    }
    
    public class AssetResourceLoader: IAssetLoader, ITickable
    {
        private interface IResourceRequest
        {
            bool isComplete { get; }
            Action<Object> pendingCallbacks { get; set; }
            Object asset { get; }
        }
        
        private class ResourceResult:IResourceRequest
        {
            private ResourceRequest _resourceRequest;

            public ResourceResult(ResourceRequest request)
            {
                _resourceRequest = request;
            }
            
            public bool isComplete => _resourceRequest.isDone;
            public Action<Object> pendingCallbacks { get; set; }
            public Object asset => _resourceRequest.asset;
        }
        
        private Dictionary<string, IResourceRequest> _resourcesLoaded = new Dictionary<string, IResourceRequest>();
        private List<string> _pendingNotifications = new List<string>();
        private List<string> _completedNotifications = new List<string>();

        public void LoadProjectileAsset(string assetName, Action<Object> assetCallback)
        {
            LoadAsset("Projectiles/" + assetName, assetCallback);
        }
        
        public void LoadEffectAsset(string assetName, Action<Object> assetCallback)
        {
            LoadAsset("Effects/" + assetName, assetCallback);
        }
        
        public void LoadWeaponAsset(string assetName, Action<Object> assetCallback)
        {
            LoadAsset("Weapons/" + assetName, assetCallback);
        }
        
        public void LoadBuildingAsset(string assetName, Action<Object> assetCallback)
        {
            LoadAsset("Buildings/" + assetName, assetCallback);
        }
        
        public void LoadTurretAsset(string assetName, Action<Object> assetCallback)
        {
            LoadAsset("Turrets/" + assetName, assetCallback);
        }
        
        public void LoadCreepAsset(string assetName, Action<Object> assetCallback)
        {
            LoadAsset("Creeps/" + assetName, assetCallback);
        }
        
        public void LoadAsset(string assetPath, Action<Object> assetCallback)
        {
            if(!_resourcesLoaded.TryGetValue(assetPath, out IResourceRequest request))
            {
                ResourceRequest resourceRequest = Resources.LoadAsync(assetPath);
                request = new ResourceResult(resourceRequest);
                _resourcesLoaded[assetPath] = request;
            }
            if(request.pendingCallbacks == null)
            {
                _pendingNotifications.Add(assetPath);
            }
            
            request.pendingCallbacks += assetCallback;
        }

        public void Tick()
        {
            for(int i = 0; i < _pendingNotifications.Count; i++)
            {
                string asset = _pendingNotifications[i];
                if (_resourcesLoaded.TryGetValue(asset, out IResourceRequest request))
                {
                    if (request!= null && request.isComplete)
                    {
                        request.pendingCallbacks?.Invoke(request.asset);
                        request.pendingCallbacks = null;
                        _completedNotifications.Add(asset);
                    }
                }
                else
                { 
                    _completedNotifications.Add(asset);
                }
            }

            foreach (var asset in _completedNotifications)
            {
                _pendingNotifications.Remove(asset);
            }
        }
    }
}