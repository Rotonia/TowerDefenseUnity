using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewBuildingData", menuName = "ScriptableObjects/Data/BuildingData", order = 1)]
    public class BuildingData : GameDataBase
    {
        public int buildingId;
        public string buildingModelPrefabName;
        public int[] turretDefinitions;
        public int health;
        public string buildingName;
        public int moneyCost;
        public int diceCost;
        public override string category => "building";
        public override string nameInHierarchy => buildingName;
        public override int Id => buildingId;
    }
}
