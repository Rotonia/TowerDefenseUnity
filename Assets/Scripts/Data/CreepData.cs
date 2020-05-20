using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewCreepData", menuName = "ScriptableObjects/Data/CreepData", order = 1)]

    public class CreepData : GameDataBase
    {
        public int creepId;
        public string creepName;
        public string creepPrefabName;
        public int health;
        public int[] turretIds;
        public float mass;
        public float speed;
        public float acceleration;
        public float radius;
        public int pipGain;
        public override string category => "creep";
        public override string nameInHierarchy => creepName;
        public override int Id => creepId;
    }
}
