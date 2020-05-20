using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "ScriptableObjects/Data/WeaponData", order = 1)]
    public class WeaponData : GameDataBase
    {
        public int weaponId;
        public string weaponPrefab;
        public int projectileDataId;
        public string muzzleEff;
        public float attackDist = 10.0f;
        public int attackDamage;
        public float reloadTime;
        public float aimTime;
        public int clipSize;
        public float refireTime;
        public float firingArc;
        public string WeaponName;
        public float splashRadius;
        public int splashDamage;
        public float splashknockBack;
        public string[] targetsCategories = {"creep", "building"};
        public override string category => "weapon";
        public override string nameInHierarchy => WeaponName;
        public override int Id => weaponId;
    }
}