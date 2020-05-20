using UnityEngine;

// Makes a transform oscillate relative to its start position
public class Rotator : MonoBehaviour
{
    public float m_Amplitude = 1.0f;
    public float m_Period = 1.0f;
    public Vector3 m_Direction = Vector3.up;
    Vector3 m_StartRotation;
    private float timeOffsetRng;
    void Start()
    {
        m_StartRotation = transform.localRotation.eulerAngles;
        timeOffsetRng = Random.value;
    }

    void Update()
    {
        var pos = m_StartRotation + m_Direction * m_Amplitude * Mathf.Sin(2.0f * Mathf.PI * (Time.time + timeOffsetRng) / m_Period);
        transform.localRotation = Quaternion.Euler(pos);
    }
}
