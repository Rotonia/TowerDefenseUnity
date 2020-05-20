using UnityEngine;
using UnityEngine.AI;

namespace ModelData
{
    public class BuildingModelData : MonoBehaviour
    {
        public Transform[] turretMountPoints;
        public Collider hitbox;
        public NavMeshObstacle obstacle;
    }
}
