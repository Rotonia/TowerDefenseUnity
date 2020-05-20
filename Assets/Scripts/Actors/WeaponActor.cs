using Data;
using DefaultNamespace;
using ModelData;
using Services;
using UnityEngine;
using Zenject;

namespace Actors
{
    using WeaponActorTypes = Actor<WeaponActor.SpawnSettings, WeaponData>;

    public interface IWeapon
    {
        ITargetable CurrentTarget { get; }
        void Despawn();
    }
   
    public class WeaponActor:WeaponActorTypes, IWeapon
    {
        public new class SpawnSettings : WeaponActorTypes.SpawnSettings
        {
            public Transform mountPoint;
            public float degreesPitch;

        }
        
        private const float TargetFindDelay = 0.25f;
        
        private WeaponModelData _modelData;
        private ITargetable _currentTarget;
        private float _targetFireDelayCountDown = 0;
        private int _clip;
        private float _reloadTime = 0;
        private float _aimTime = 0;
        private float _refireTime = 0;

        public ParticleSystem fireEffect;
        
        private Actor<ProjectileActor.SpawnSettings, ProjectileData>.Factory _projFactory;
        private ITargetFinder _targetFinder;
        
        [Inject]
        public void Init( 
            Actor<ProjectileActor.SpawnSettings, ProjectileData>.Factory projFactory,
            ITargetFinder targetFinder)
        {   
            _projFactory = projFactory;
            _targetFinder = targetFinder;
        }
        
        protected override void Spawning()
        {
            base.Spawning();
            
            this.transform.SetParent(settings.mountPoint, false);
            
            _clip = actorData.clipSize;
            AssetLoader.LoadWeaponAsset(actorData.weaponPrefab,OnWeaponLoaded);
            
            if (!string.IsNullOrEmpty(actorData.muzzleEff))
            {
                AssetLoader.LoadEffectAsset(actorData.muzzleEff,OnEffectLoaded);
            }
        }

        private void OnWeaponLoaded(Object asset)
        {    
            _modelData = (Instantiate(asset, this.transform) as GameObject).GetComponent<WeaponModelData>();
            WeaponSetup();
        }

        private void OnEffectLoaded(Object asset)
        {
            fireEffect = (Instantiate(asset, this.transform) as GameObject).GetComponent<ParticleSystem>();
            SetupFireEffect();
        }

        private void WeaponSetup()
        {
            SetupFireEffect();
        }

        private void SetupFireEffect()
        {
            if (_modelData != null && fireEffect != null)
            {
                fireEffect.transform.SetParent(_modelData.shotEffectSpawnPos);
            }
        }

        protected override void Despawning()
        {
            base.Despawning();
            if (_modelData != null)
            {
                Destroy(_modelData.gameObject);
                _modelData = null;
            }
        }

        public void Update()
        {
            if (paused)
            {
                return;
            }
            
            _targetFireDelayCountDown -= Time.deltaTime;

            if (_targetFireDelayCountDown <= 0 || _currentTarget != null && !_currentTarget.IsAlive())
            {
                _targetFireDelayCountDown = TargetFindDelay;
                FindTarget();
            }

            Debug.DrawRay(transform.position, transform.forward * actorData.attackDist, Color.green);
            if (_currentTarget != null)
            {
                Debug.DrawRay(transform.position, _currentTarget.GetTransform().position - transform.position, Color.magenta);
            }

            if (NeedsToReload())
            {
                Reload();
            }
            else if (!RefireReady())
            {
                ReadyNextShot();
            }   

            Aim();
            
            if (TargetInRange() && CanFire())
            {
                Fire();
            }
        }

        public void FindTarget()
        {
            RangeCheckResults results = _targetFinder.GetTargetsInRangeForCategories(Team() == 0 ? 1 : 0, actorData.targetsCategories,
                transform.position, actorData.attackDist);
            _currentTarget = results.GetClosestTarget()?.Target;
        }

        public bool NeedsToReload()
        {
            return _clip <= 0 || _reloadTime > 0;
        }
        
        public bool AimedShot()
        {
            return _aimTime >= actorData.aimTime;
        }

        public bool RefireReady()
        {
            return  _refireTime <= 0;
        }

        public bool TargetInRange()
        {
            if (_currentTarget == null)
            {
                return true;
            }
            
            return Vector3.Distance(transform.position, _currentTarget.GetTransform().position) <= actorData.attackDist;
        }
        
        public bool CanFire()
        {
            if (_currentTarget == null)
            {
                return false;
            }
            
            if (NeedsToReload())
            {
                return false;
            }

            if (!AimedShot())
            {
                return false;
            }

            if (!RefireReady())
            {
                return false;
            }

            return true;
        }
        
        public void Fire()
        {
            _clip--;
            _projFactory.Create(new ProjectileActor.SpawnSettings
            {
                dataId = actorData.projectileDataId,
                startPos = _modelData.projSpawnPos,
                target = _currentTarget,
                damage = actorData.attackDamage,
                maxRange = actorData.attackDist,
                Team = Team(),
                splashRadius = actorData.splashRadius,
                splashDamage = actorData.splashDamage,
                splashKnockback = actorData.splashknockBack,
                targetsCategories =  actorData.targetsCategories,
            });

            if (fireEffect != null)
            {
                fireEffect.Simulate(0, true, true);
                fireEffect.Play();
            }

            if (_modelData.fireEffect != null)
            {
                fireEffect.Simulate(0, true, true);
                _modelData.fireEffect.Play();
            }
            
            if (_clip == 0)
            {
                _reloadTime = actorData.reloadTime;
                _aimTime = 0;
            }
            else
            {
                _refireTime = actorData.refireTime;
            }
        }

        public void Reload()
        {
            if (_clip == 0 || _reloadTime > 0)
            {
                _reloadTime -= Time.deltaTime;
                if (_reloadTime <= 0)
                {
                    _reloadTime = 0;
                    _clip = actorData.clipSize;
                }
            }
        }

        public void ReadyNextShot()
        {
            _refireTime = Mathf.Max(0, _refireTime - Time.deltaTime);
        }

        public void Aim()
        {
            if (!NeedsToReload() && TargetInLos())
            {
                _aimTime = Mathf.Min(actorData.aimTime, _aimTime + Time.deltaTime);
                
            }
            else
            {
                _aimTime = Mathf.Max(0, _aimTime - Time.deltaTime);
            }
        }

        public bool TargetInLos()
        {
            if (_currentTarget == null)
            {
                return false;
            }

            Transform target = _currentTarget.GetTransform();
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            float angle = Vector2.Angle(new Vector2(dirToTarget.x, dirToTarget.z), new Vector2(transform.forward.x, transform.forward.z));
            return angle <= actorData.firingArc;
        }

        public override void OnHit(int damage, Vector3 knockBack)
        {
        }
        
        public ITargetable CurrentTarget => _currentTarget;
    }
}