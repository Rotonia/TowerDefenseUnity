using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewProjectileData", menuName = "ScriptableObjects/Data/ProjectileData", order = 1)]
    public class ProjectileData:GameDataBase 
    {
        public int projectileDataId;
        public string projectileName;
        public string projectilePrefabName;
        public float knockBack = 0.1f;
        public float boomTimer = 1;
        public bool lockOn;
        public bool arcing;
        public float arcHeight;
        public float speed = 1;
        public float turnSpeed = 1;
        public string explosion;
        public float lifetime;
        public float maxSpeed;
        public float acceleration;
        public override string category => "proj";
        public override string nameInHierarchy => projectileName;
        public override int Id => projectileDataId;
    }
}