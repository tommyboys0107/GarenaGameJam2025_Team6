using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AttackRange : MonoBehaviour
{
    public int damage;
    public float pushForce;
    public GameObject colliderObj;
    public float delayOpenRangeTime;
    public float rangeOpenDuration;
    [Header("Camera Shake")]
    public bool enableCameraShake = true;
    public float cameraShakeAmplitude;
    public float cameraShakeFrequency;
    public float cameraShakeDuration;

    public async UniTask StartAttack(Vector3 direction)
    {
        await UniTask.WaitForSeconds(delayOpenRangeTime);
        colliderObj.SetActive(true);
        transform.forward = direction.normalized;
        await UniTask.WaitForSeconds(rangeOpenDuration);
        colliderObj.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.GetComponent<Enemy>())
        {
            other.transform.parent.GetComponent<Enemy>().OnHit(damage, pushForce);
            if (enableCameraShake)
            {
                CameraShaker.Instance.Shake(cameraShakeAmplitude, cameraShakeFrequency, cameraShakeDuration);
            }
        }
    }
}
