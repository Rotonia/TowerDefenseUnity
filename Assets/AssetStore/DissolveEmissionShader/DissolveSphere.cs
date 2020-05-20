using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DissolveSphere : MonoBehaviour {

    Material mat;
    public bool dissolve = false;
    public float dissolveTime = 1f;
    public float amount;
    private void Start() {
        mat = GetComponent<Renderer>().material;
        Reset();
    }

    public void Reset()
    {
        dissolve = false;
        SetDissolveAmount(0);
        amount = 0;
    }
    
    private void SetDissolveAmount(float newAmount)
    {
        if (mat != null)
        {
            mat.SetFloat("_DissolveAmount", newAmount);
        }
    }
    
    private void Update() {
        if (dissolve)
        {
            amount += Time.deltaTime;
            SetDissolveAmount(amount / dissolveTime);
            if (amount > dissolveTime * 1.1f)
            {
                dissolve = false;
            }
        }
    }
}