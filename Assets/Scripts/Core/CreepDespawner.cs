using Actors;
using UnityEngine;

namespace Core
{
    public class CreepDespawner : MonoBehaviour
    {
        public void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Creep"))
            {
                other.attachedRigidbody.gameObject.GetComponent<CreepController>().Despawn();
            }
        }
    }
}
