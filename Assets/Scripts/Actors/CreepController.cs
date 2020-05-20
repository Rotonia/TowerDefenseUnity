using System.Collections.Generic;
using Data;
using DefaultNamespace;
using Events;
using ModelData;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Object = UnityEngine.Object;

namespace Actors
{
    public class CreepController : Actor<CreepController.SpawnSettings, CreepData>
    {
        public new class SpawnSettings : Actor<SpawnSettings, CreepData>.SpawnSettings
        {
            public Vector3 startPos;
            public Transform targetPos;
        }
    
        [SerializeField]
        private NavMeshAgent navAgent;
    
        [SerializeField]
        private Transform characterRoot;

        private Transform _start;
        private Transform _target;
    
        private int _health = 2;
        private Vector3 _impact = Vector3.zero;
        private bool _moveQueued = false;
        private bool _dying = false;
    
        private CreepModelData _modelData;
        private readonly List<ITurret> _turrets = new List<ITurret>();
    
        private Actor<TurretActor.SpawnSettings, TurretData>.Factory _turretFactory;
   
        [Inject]
        public void Init(Actor<TurretActor.SpawnSettings, TurretData>.Factory turretFactory)
        {
            _turretFactory = turretFactory;
        }

        private void Warp(Vector3 position)
        {
            navAgent.Warp(new Vector3(position.x, position.y, position.z));
        }
    
        private void AddImpact(Vector3 force){
            var dir = force.normalized;
            dir.y = 0.5f; // add some velocity upwards - it's cooler this way
            _impact += dir.normalized * force.magnitude / actorData.mass;
        }

        private void Die()
        {
            if (actorData.pipGain > 0)
            {
                SignalBus.Fire(new PipsGainedSignal
                {
                    pips = actorData.pipGain
                });
            }

            _dying = true;
            navAgent.isStopped = true;
        
            _target = null;
            if (_modelData != null && _modelData.dissolver != null)
            {
                _modelData.dissolver.dissolve = true;
            }
            else
            {
                Despawn();
            }
        }
    
        // Update is called once per frame
        void Update()
        {
            if (_moveQueued)
            {
                MoveTowardsBase();
            }
        
            if (_dying && _modelData != null && _modelData.dissolver.amount >= 1)
            {
                Despawn();
            }
        
            if (navAgent.isOnNavMesh && _impact.magnitude > 0.2)
            {
                navAgent.Move(_impact * Time.deltaTime); 
            }

            if (navAgent.hasPath)
            {
                // Display the explosion radius when selected
                Vector3 prevCorner = this.transform.position;
                foreach (var segment in navAgent.path.corners)
                {
                    Debug.DrawLine(prevCorner, segment, Color.cyan);
                    prevCorner = segment;
                }
            }

            _impact = Vector3.Lerp(_impact, Vector3.zero, 5 * Time.deltaTime);
        }
    
        private void MoveTowardsBase()
        {
            if (_target != null && navAgent.isOnNavMesh)
            {
                navAgent.SetDestination(_target.position);
                _moveQueued = false;
            }
        } 
    
        public override void OnHit(int damage, Vector3 knockBack)
        {
            _health -= damage;
            if (_health <= 0)
            {
                Die();
            }

            AddImpact(knockBack);
        }
    
        public override bool InRange(Vector3 pos, float range, out Vector3 closestPoint, out float distanceBetween)
        {
            if (_modelData != null)
            {
                return RangeCheckUtils.InRangeBounds(pos, _modelData.hitBox.bounds, range, out closestPoint,
                    out distanceBetween);
            }
            
            return RangeCheckUtils.InRangePos(pos, this.transform.position, range, out closestPoint,
                out distanceBetween);
        }

        protected override void Despawning()
        {
            base.Despawning();

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

            navAgent.ResetPath();
            navAgent.isStopped = false;
        }

        protected override void Spawning()
        {
            base.Spawning();
        
            AssetLoader.LoadCreepAsset(actorData.creepPrefabName, OnCreepAssetLoaded);
            navAgent.speed = actorData.speed;
            navAgent.acceleration = actorData.acceleration;
            navAgent.radius = actorData.radius;
            _target = settings.targetPos;
            _health = actorData.health;
            _impact  = Vector3.zero;
            Warp(settings.startPos);
        
            _moveQueued = true;
        }

        public override Transform GetTransform()
        {
            if(_modelData != null && _modelData.target != null)
            {
                return _modelData.target;
            }
            return this.transform;
        }

        private void OnCreepAssetLoaded(Object creep)
        {
            _modelData = (Instantiate(creep, characterRoot) as GameObject).GetComponent<CreepModelData>();
            SetupTurrets();
        }

        private void SetupTurrets()
        {
            int weaponDefs = actorData.turretIds.Length;
            if (weaponDefs > 0 && _modelData != null && actorData.turretIds.Length > 0 )
            {
                int weaponIndex = 0;
                foreach (var mountPoint in _modelData.turretMountPoints)
                {
                    var turret =_turretFactory.Create(new TurretActor.SpawnSettings
                    {
                        dataId = actorData.turretIds[weaponIndex],
                        mountPoint = mountPoint,
                        Team = 1
                    });
                    _turrets.Add(turret as ITurret);
                
                    weaponIndex++;
                    if (weaponIndex >= actorData.turretIds.Length)
                    {
                        break;
                    }
                }
            }
        }
    
        public override bool IsAlive()
        {
            return _health > 0;
        }
    }
}


