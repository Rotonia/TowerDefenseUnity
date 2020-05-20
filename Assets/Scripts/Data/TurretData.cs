using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewTurretData", menuName = "ScriptableObjects/Data/TurretData", order = 1)]
    public class TurretData : GameDataBase
    {
        public int turretId;
        public string turretName;
        public string turretModelPrefabName;
        public float turretRotationSpeed;
        public int[] weaponDefinitions;
        public override string category => "turret";
        public override string nameInHierarchy => turretName;
        public override int Id => turretId;
    }
}