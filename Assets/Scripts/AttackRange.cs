using System;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    public int damage;
    public float pushForce;
    public bool enableCameraShake = true;
    public float cameraShakeAmplitude;
    public float cameraShakeFrequency;
    public float cameraShakeDuration;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.GetComponent<Enemy>())
        {
            other.transform.parent.GetComponent<Enemy>().OnHit(damage, pushForce);
            if (enableCameraShake)
            {
                CameraShaker.Instance.Shake(cameraShakeAmplitude, cameraShakeFrequency, cameraShakeDuration);
            }
        }
    }
}
