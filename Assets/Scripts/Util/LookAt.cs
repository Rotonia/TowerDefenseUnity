using UnityEngine;

namespace Util
{
    [ExecuteInEditMode]
    public class LookAt : MonoBehaviour
    {

        [SerializeField] private Transform target;
    
        // Update is called once per frame
        void Update()
        {
            this.transform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
        }
    }
}
