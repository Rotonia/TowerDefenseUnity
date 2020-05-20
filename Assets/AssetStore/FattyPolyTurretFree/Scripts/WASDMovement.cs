using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASDMovement : MonoBehaviour
{
    [SerializeField]
    public Vector2 speedRange = new Vector2(10,40);
    
    [SerializeField]
    public Vector2 minMaxDistance = new Vector2(5,40);
    
    [SerializeField]
    public Vector2 rotationSpeedRange = new Vector2( 30,5);
    
    [SerializeField]
    public float zoomSpeed = 2f;

    [SerializeField]
    public Collider cameraBounds;
    
    void Update () 
    {
        Vector3 pos = transform.position;
        if (Input.mouseScrollDelta.y != 0)
        {
            pos.y = Mathf.Clamp(pos.y + Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime, minMaxDistance.x,
                minMaxDistance.y);
        }

        float zoomScale = pos.y / (minMaxDistance.y - minMaxDistance.x);
        float speed = Mathf.Lerp(this.speedRange.x, this.speedRange.y, zoomScale);
        
        if (Input.GetKey ("w")) {
            pos.z += speed * Time.deltaTime;
        }

        if (Input.GetKey ("s")) {
            pos.z -= speed * Time.deltaTime;
        }

        if (Input.GetKey ("d")) {
            pos.x += speed * Time.deltaTime;
        }

        if (Input.GetKey ("a")) {
            pos.x -= speed * Time.deltaTime;
        }

        if (!cameraBounds.bounds.Contains(pos))
        {
            pos = cameraBounds.ClosestPointOnBounds(pos);
        }
        
        transform.position = pos;
       /* float rotationSpeed = Mathf.Lerp(this.rotationSpeedRange.x, this.rotationSpeedRange.y, zoomScale);
        Quaternion rot = transform.rotation;
        if (Input.GetKey ("q")) {
            rot *= Quaternion.AngleAxis(-rotationSpeed * Time.deltaTime, Vector3.up);
        }
        
        if (Input.GetKey ("e")) {
            rot *= Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up);
        }
        transform.rotation = rot;*/
    }
}
