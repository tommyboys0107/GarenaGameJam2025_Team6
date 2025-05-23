﻿using UnityEngine;
using Unity.Cinemachine;

public class CameraShaker : MonoBehaviour
{
    public static CameraShaker Instance { get; private set; } = null;

    public NoiseSettings noiseProfile = null;
    [Header("Override CameraShaker param")]
    public bool canOverrideParam = false;
    public float overrideAmplitute = 1.0f;
    public float overrideFrequency = 1.0f;
    public float overrideDuration = 0.5f;

    CinemachineCamera virtualCamera = null;
    CinemachineBasicMultiChannelPerlin perlinNoise = null;
    float maxAmplitute = 1.0f;
    float maxDuration = 1.0f;
    float elapsedTime = 0.0f;

    public void Shake(float amplitute = 1.0f, float frequency = 1.0f, float duration = 0.5f)
    {
        if (!this.enabled)
        {
            return;
        }

        if (canOverrideParam)
        {
            perlinNoise.AmplitudeGain = overrideAmplitute;
            perlinNoise.FrequencyGain = overrideFrequency;
            maxAmplitute = overrideAmplitute;
            maxDuration = overrideDuration;
            elapsedTime = overrideDuration;
        }
        else
        {
            perlinNoise.AmplitudeGain = amplitute;
            perlinNoise.FrequencyGain = frequency;
            maxAmplitute = amplitute;
            maxDuration = duration;
            elapsedTime = duration;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
        DontDestroyOnLoad(gameObject);

        virtualCamera = GetComponent<CinemachineCamera>();
        perlinNoise = virtualCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
        perlinNoise.NoiseProfile = noiseProfile;
        ResetToRest();
    }

    void Update()
    {
        if (elapsedTime >= 0.0f)
        {
            perlinNoise.AmplitudeGain = maxAmplitute * (elapsedTime / maxDuration);
            elapsedTime -= Time.deltaTime;
        }
        else
        {
            ResetToRest();
        }
    }

    private void ResetToRest()
    {
        perlinNoise.AmplitudeGain = 0.0f;
        perlinNoise.FrequencyGain = 0.0f;
        elapsedTime = 0.0f;
    }
}
