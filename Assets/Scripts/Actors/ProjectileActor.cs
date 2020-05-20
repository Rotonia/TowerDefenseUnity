using Data;
using DefaultNamespace;
using ModelData;
using Services;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Actors
{
    using ProjectileActorTypes = Actor<ProjectileActor.SpawnSettings, ProjectileData>;

    public interface ITargetable
    {
        Transform GetTransform();
        void OnHit(int damage,  Vector3 knockBack);
        bool IsAlive();
        int Team();
        string Category { get; }
        bool InRange(Vector3 pos, float range, out Vector3 closestPoint, out float distanceBetween);
        bool IsPaused();
    }
   
    public class ProjectileActor : ProjectileActorTypes
    {
        public new class SpawnSettings: ProjectileActorTypes.SpawnSettings
        {
            public Transform startPos; 
            public ITargetable target;
            public int damage;
            public float maxRange;
            public float splashRadius;
            public int splashDamage;
            public float splashKnockback;
            public string[] targetsCategories;
        }
        
        private float _startTime;
        private float _speed;
        private float _lifeTime;
        private int damage;
        private float maxRange;
        private ITargetable _target;

        private ProjectileModelData projectileModel;

        private ParticleSystem _explosionEffect;
        private Vector3 startPosTarget;
        private Vector3 startPos;
        private Vector3 travelDir;

        private ITargetFinder _targetFinder;

        [Inject]
        public void Inject(ITargetFinder targetFinder)
        {
            _targetFinder = targetFinder;
        }
        
        public void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Creep"))
            {
                var controller = other.attachedRigidbody.gameObject.GetComponent<CreepController>();
                if (controller.Team() != Team())
                {
                    controller.OnHit(damage, transform.forward * actorData.knockBack);
                    Explosion();
                }
            }
            else if(other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Building"))
            {
                var controller = other.attachedRigidbody.gameObject.GetComponent<BuildingActor>();
                if (controller.Team() != Team() && !controller.IsPaused())
                {
                    controller.OnHit(damage, transform.forward * actorData.knockBack);
                    Explosion();
                }
            }
            else if(other.transform.CompareTag("Ground"))
            {
                Explosion();
            }
        }

        public void Explosion()
        {
            if (settings.splashDamage > 0 && settings.splashRadius > 0)
            {
                RangeCheckResults results = _targetFinder.GetTargetsInRangeForCategories(Team() == 0 ? 1 : 0,
                    settings.targetsCategories, this.transform.position, settings.splashRadius);

                float splashRadiusSqr = settings.splashRadius * settings.splashRadius;
                foreach (var result in results.rangeResults)
                {
                    float splashRatio = 1 - (result.Distance * result.Distance) / splashRadiusSqr;
                    float splashDamage = settings.splashDamage * splashRatio;
                    Vector3 splashKnockback = (result.ClosestPoint - results.Position).normalized * settings.splashKnockback;
                    result.Target.OnHit((int) splashDamage, splashKnockback);
                }
            }

            if (_explosionEffect != null)
            {
                _explosionEffect.gameObject.SetActive(true);
                _explosionEffect.gameObject.transform.SetParent(null, true);
                _explosionEffect.Simulate(0, true, true);
                _explosionEffect.Play();
            }
            
            Despawn();
        }

        public void Update()
        {
            if (Time.realtimeSinceStartup - _startTime > actorData.lifetime)
            {
                if (projectileModel != null)
                {
                    Destroy(_explosionEffect);
                }
                Despawn();
                return;
            }
            
            Move();
        }

        public void OnProjectileLoaded(Object projLoad)
        {
            projectileModel = (Instantiate(projLoad, this.transform) as GameObject).GetComponent<ProjectileModelData>();
        }

        private void OnExplosionLoaded(Object explosionAsset)
        {
            //@TODO this should be pooled
            _explosionEffect = (Instantiate(explosionAsset, this.transform ) as GameObject).GetComponent<ParticleSystem>();
            _explosionEffect.gameObject.SetActive(false);
        }

        protected override void Spawning()
        {
            base.Spawning();
            _target = settings.target;
            _startTime = Time.realtimeSinceStartup;
            _speed = actorData.speed;
            maxRange = settings.maxRange;
            this.damage = settings.damage;

            startPosTarget = settings.target.GetTransform().position;
            startPos = settings.startPos.position;
            this.transform.position = settings.startPos.position;
            this.transform.rotation = settings.startPos.rotation;
            
            AssetLoader.LoadProjectileAsset(actorData.projectilePrefabName, OnProjectileLoaded);
            AssetLoader.LoadEffectAsset(actorData.explosion, OnExplosionLoaded);

            travelDir = (_target.GetTransform().position - this.startPos).normalized;
        }

        public override void OnHit(int damage, Vector3 knockBack)
        {
        }

        
        //This would eventually get broken up into separate mover components
        public void Move()
        {
            _speed = Mathf.Min(actorData.maxSpeed, _speed + actorData.acceleration * Time.deltaTime);
            
            
            if (actorData.arcing)
            {
                if (actorData.lockOn && _target.IsAlive())
                {
                    startPosTarget = _target.GetTransform().position;
                }
                
                Vector3 dirNormal = startPosTarget - startPos;
                dirNormal = dirNormal.normalized;
                dirNormal.y = 0;
                Vector3 dPos = dirNormal * (_speed * Time.deltaTime); 
                float distTraveled = Vector2.Distance(new Vector2(startPos.x, startPos.z), new Vector2(transform.position.x + dPos.x, transform.position.z + dPos.z));
                float totalDistance = Vector2.Distance(new Vector2(startPos.x, startPos.z), new Vector2(startPosTarget.x, startPosTarget.z));
                float t = Mathf.Clamp01(distTraveled / totalDistance);

                float arcHeight = actorData.arcHeight * totalDistance / maxRange;
                
                Vector3 middle = Vector3.Lerp(startPos, startPosTarget, 0.5f) + Vector3.up * arcHeight;
                Vector3 newPos = Mathf.Pow(1-t,2)*startPos + 2*t*(1-t)*middle + Mathf.Pow(t,2)*startPosTarget;
                SmoothLookAt(newPos);
                transform.position = newPos;
                // Do something when we reach the target
                if (newPos == startPosTarget)
                {
                    Explosion();
                }
            }
            else if(actorData.lockOn && _target != null)
            {
                if (_target.IsAlive())
                {
                    Vector3 dir = _target.GetTransform().position - transform.position;

                    Vector3 newDirection = Vector3.RotateTowards(transform.forward, dir,
                        Time.deltaTime * actorData.turnSpeed, 0.0f);
                    Debug.DrawRay(transform.position, newDirection, Color.red);

                    transform.Translate(Vector3.forward * (Time.deltaTime * _speed));
                    transform.rotation = Quaternion.LookRotation(newDirection);
                }
                else
                {
                    _target = null;
                }
            }
            else
            {
                float singleSpeed = _speed * Time.deltaTime;
                Vector3 newPos = travelDir * singleSpeed;
                SmoothLookAt( newPos + transform.position);
                transform.Translate(newPos, Space.World);
            }
        }
        
        void SmoothLookAt(Vector3 target)
        {
            Vector3 dir = target - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 100);
        }

        protected override void Despawning()
        {
            if (projectileModel != null)
            {
               Destroy(projectileModel.gameObject);
            }

            _target = null;
        }
    }
}
