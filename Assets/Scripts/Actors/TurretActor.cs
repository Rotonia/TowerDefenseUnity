using System.Collections.Generic;
using Core;
using Data;
using ModelData;
using Services;
using UnityEngine;
using Zenject;

namespace Actors
{
    using TurretActorTypes = Actors.Actor<TurretActor.SpawnSettings, TurretData>;

    public interface ITurret
    {
        void Despawn();
    }

    public class TurretActor : TurretActorTypes, ITurret
    {
        public new class SpawnSettings : TurretActorTypes.SpawnSettings
        {
            public Transform mountPoint;
        }
        
        private TurretModelData modelData;
        
        private ITargetable currentTarget;
        
        private Actor<WeaponActor.SpawnSettings, WeaponData>.Factory _weaponFactory;

        private List<IWeapon> _weapons = new List<IWeapon>();
        
        [Inject]
        public void Init(Actor<WeaponActor.SpawnSettings, WeaponData>.Factory weaponFactory )
        {
            _weaponFactory = weaponFactory;
        }

        private void OnTurretLoaded(Object turret)
        {
            modelData = (Instantiate(turret, this.transform) as GameObject).GetComponent<TurretModelData>();
            SetupTurret();
        }

        private void SetupTurret()
        {
            int weaponDataCount = actorData.weaponDefinitions.Length;
            if (weaponDataCount > 0)
            {
                int curWeaponDataIndex = -1;
                foreach (var weaponMount in modelData.weaponMountPoints)
                {
                    curWeaponDataIndex = Mathf.Min(curWeaponDataIndex + 1, actorData.weaponDefinitions.Length - 1);
                    FactoriedMonoBehaviour<WeaponActor.SpawnSettings> weapon = _weaponFactory.Create(new WeaponActor.SpawnSettings
                    {
                        dataId = actorData.weaponDefinitions[curWeaponDataIndex],
                        Team = Team(),
                        mountPoint = weaponMount
                    }); 
                        
                    _weapons.Add(weapon as IWeapon);
                }
            }
        }
        
        protected override void Spawning()
        {
            base.Spawning();
            _weapons.Clear();
            modelData = null;
            currentTarget = null;
            
            this.transform.SetParent(settings.mountPoint,false);
            AssetLoader.LoadTurretAsset(actorData.turretModelPrefabName, OnTurretLoaded);
        }

        protected override void Despawning()
        {
            base.Despawning();
            foreach (var weapon in _weapons)
            {
                weapon.Despawn();
            }
            _weapons.Clear();

            if (modelData != null)
            {
                Destroy(modelData.gameObject);
                modelData = null;
            }
        }

        public void FindTargetFromWeapons()
        {
            foreach (var weapon in _weapons)
            {
                if (weapon.CurrentTarget != null)
                {
                    currentTarget = weapon.CurrentTarget;
                    return;
                }
            }
        }
        
        public void Update()
        {
            if (paused)
            {
                return;
            }
            
            FindTargetFromWeapons();
            if (modelData != null)
            {
                Debug.DrawRay(modelData.turretBase.position, modelData.turretBase.forward * 10, Color.red);
            }

            if (currentTarget != null)
            {
                FollowTarget();
                Debug.DrawRay( modelData.turretBase.position, (currentTarget.GetTransform().position -  modelData.turretBase.position), Color.yellow);
            }
        }
        
        private void FollowTarget() //todo : smooth rotate
        {
            Vector3 targetDir = currentTarget.GetTransform().position - modelData.turretBase.position;
            targetDir.y = 0;
            modelData.turretBase.rotation = Quaternion.RotateTowards( 
                modelData.turretBase.rotation, 
                Quaternion.LookRotation(targetDir), 
                actorData.turretRotationSpeed * Time.deltaTime);
        }

        public override void OnHit(int damage, Vector3 knockBack)
        {
        }
    }
}