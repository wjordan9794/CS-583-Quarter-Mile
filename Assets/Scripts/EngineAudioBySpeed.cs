using System.Collections;
using UnityEngine;

public class EngineAudioBySpeed : MonoBehaviour
{
    [Header("Car")]
    public Rigidbody carRigidbody;

    [Header("Engine Audio")]
    public AudioSource engineAudio;
    public float engineVolume = 0.5f;

    [Header("Pitch")]
    public float minimumPitch = 0.8f;
    public float maximumPitch = 1.8f;
    public float pitchSmoothTime = 0.2f;

    [Header("Gear Settings")]
    public int currentGear = 1;

    public float[] gearTopSpeeds =
    {
        0f,
        51f,
        96f,
        141f,
        190f
    };

    [Header("RPM Limiter")]
    public AudioSource limiterAudio;
    public bool limiterClipLoops = false;
    public bool limiterInTopGear = false;
    public float limiterVolume = 0.7f;
    public float limiterRepeatDelay = 0.25f;

    [Header("Visual RPM Settings")]
    [Range(0f, 1f)]
    public float postShiftRpmFloor = 0.28f;

    public float postShiftHoldTime = 0.45f;
    public float shiftBounceAmount = 0.08f;
    public float limiterBounceAmount = 0.1f;
    public float visualRpmSmoothTime = 0.12f;

    public float CurrentSpeedKmh { get; private set; }
    public float RpmNormalized { get; private set; }
    public float DisplayedRpmNormalized { get; private set; }
    public bool AtLimiter { get; private set; }

    private float smoothedSpeed;
    private float pitchVelocity;
    private float visualRpmVelocity;
    private float limiterTimer;

    private float postShiftTimer;
    private float bounceTimer;
    private float bounceDuration;
    private float bounceAmount;

    private bool wasAtLimiter;
    private bool engineRunning;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (carRigidbody == null)
            carRigidbody = GetComponentInParent<Rigidbody>();

        if (engineAudio != null)
        {
            engineAudio.loop = true;
            engineAudio.playOnAwake = false;
            engineAudio.volume = 0f;
            engineAudio.pitch = minimumPitch;
            engineAudio.spatialBlend = 0f;
        }

        if (limiterAudio != null)
        {
            limiterAudio.playOnAwake = false;
            limiterAudio.loop = false;
            limiterAudio.volume = limiterVolume;
            limiterAudio.spatialBlend = 0f;
        }
    }

    void Update()
    {
        if (carRigidbody == null ||
            gearTopSpeeds.Length < 2)
        {
            return;
        }

        float actualSpeed =
            Mathf.Abs(
                Vector3.Dot(
                    carRigidbody.linearVelocity,
                    transform.forward
                )
            ) * 3.6f;

        smoothedSpeed = Mathf.Lerp(
            smoothedSpeed,
            actualSpeed,
            1f - Mathf.Exp(-5f * Time.deltaTime)
        );

        CurrentSpeedKmh = smoothedSpeed;

        currentGear = Mathf.Clamp(
            currentGear,
            1,
            gearTopSpeeds.Length - 1
        );

        float lowerSpeed =
            currentGear == 1
                ? 0f
                : gearTopSpeeds[currentGear - 1];

        float upperSpeed =
            gearTopSpeeds[currentGear];

        RpmNormalized = Mathf.InverseLerp(
            lowerSpeed,
            upperSpeed,
            smoothedSpeed
        );

        float targetPitch = Mathf.Lerp(
            minimumPitch,
            maximumPitch,
            RpmNormalized
        );

        if (engineAudio != null)
        {
            engineAudio.pitch = Mathf.SmoothDamp(
                engineAudio.pitch,
                targetPitch,
                ref pitchVelocity,
                pitchSmoothTime
            );
        }

        if (engineRunning)
            UpdateLimiter();
        else
            StopLimiterAudio();

        UpdateDisplayedRpm();
    }

    public void StartEngine()
    {
        engineRunning = true;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        if (engineAudio != null &&
            engineAudio.clip != null)
        {
            engineAudio.volume = engineVolume;

            if (!engineAudio.isPlaying)
                engineAudio.Play();
        }
    }

    public void StopEngine()
    {
        engineRunning = false;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        StopLimiterAudio();

        if (engineAudio != null)
        {
            engineAudio.Stop();
            engineAudio.volume = 0f;
        }
    }

    public void FadeOutEngine(float duration)
    {
        engineRunning = false;
        StopLimiterAudio();

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine =
            StartCoroutine(FadeOutEngineRoutine(duration));
    }

    IEnumerator FadeOutEngineRoutine(float duration)
    {
        if (engineAudio == null)
            yield break;

        float startingVolume =
            engineAudio.volume;

        if (duration <= 0f)
        {
            engineAudio.Stop();
            engineAudio.volume = 0f;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            engineAudio.volume = Mathf.Lerp(
                startingVolume,
                0f,
                elapsed / duration
            );

            yield return null;
        }

        engineAudio.Stop();
        engineAudio.volume = 0f;
    }

    void UpdateLimiter()
    {
        limiterTimer -= Time.deltaTime;

        bool isTopGear =
            currentGear >= gearTopSpeeds.Length - 1;

        bool limiterAllowed =
            limiterInTopGear || !isTopGear;

        AtLimiter =
            limiterAllowed &&
            RpmNormalized >= 0.97f &&
            CurrentSpeedKmh > 5f;

        if (AtLimiter && !wasAtLimiter)
        {
            TriggerBounce(
                limiterBounceAmount,
                0.2f
            );
        }

        wasAtLimiter = AtLimiter;

        if (!AtLimiter)
        {
            StopLimiterAudio();
            return;
        }

        if (limiterAudio == null ||
            limiterAudio.clip == null)
        {
            return;
        }

        limiterAudio.volume = limiterVolume;

        if (limiterClipLoops)
        {
            limiterAudio.loop = true;

            if (!limiterAudio.isPlaying)
                limiterAudio.Play();
        }
        else
        {
            limiterAudio.loop = false;

            if (limiterTimer <= 0f)
            {
                limiterAudio.PlayOneShot(
                    limiterAudio.clip
                );

                limiterTimer =
                    limiterRepeatDelay;
            }
        }
    }

    void StopLimiterAudio()
    {
        AtLimiter = false;

        if (limiterAudio != null &&
            limiterAudio.isPlaying)
        {
            limiterAudio.Stop();
        }

        limiterTimer = 0f;
    }

    void UpdateDisplayedRpm()
    {
        postShiftTimer -= Time.deltaTime;
        bounceTimer -= Time.deltaTime;

        float targetRpm =
            RpmNormalized;

        if (postShiftTimer > 0f)
        {
            targetRpm = Mathf.Max(
                targetRpm,
                postShiftRpmFloor
            );
        }

        if (bounceTimer > 0f &&
            bounceDuration > 0f)
        {
            float bounceProgress =
                bounceTimer / bounceDuration;

            targetRpm +=
                bounceAmount * bounceProgress;
        }

        targetRpm =
            Mathf.Clamp01(targetRpm);

        DisplayedRpmNormalized =
            Mathf.SmoothDamp(
                DisplayedRpmNormalized,
                targetRpm,
                ref visualRpmVelocity,
                visualRpmSmoothTime
            );
    }

    public void SetGear(int newGear)
    {
        int previousGear =
            currentGear;

        currentGear = Mathf.Clamp(
            newGear,
            1,
            gearTopSpeeds.Length - 1
        );

        if (currentGear > previousGear)
        {
            postShiftTimer =
                postShiftHoldTime;

            TriggerBounce(
                shiftBounceAmount,
                0.25f
            );
        }
    }

    void TriggerBounce(
        float amount,
        float duration
    )
    {
        bounceAmount = amount;
        bounceDuration = duration;
        bounceTimer = duration;
    }

    void OnDisable()
    {
        StopEngine();
    }
}