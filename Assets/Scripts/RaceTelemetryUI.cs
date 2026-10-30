using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RaceTelemetryUI : MonoBehaviour
{
    [Header("References")]
    public EngineAudioBySpeed engineAudio;
    public ManualGearController gearController;

    [Header("Text")]
    public TMP_Text speedText;
    public TMP_Text gearText;
    public TMP_Text shiftText;

    [Header("RPM Display")]
    public Slider rpmSlider;
    public Image rpmFillImage;

    [Header("Shift Settings")]
    [Range(0f, 1f)]
    public float shiftWarningPoint = 0.82f;

    void Start()
    {
        if (shiftText != null)
            shiftText.transform.SetAsLastSibling();
    }

    void Update()
    {
        if (engineAudio == null)
            return;

        // Convert kilometers per hour to miles per hour.
        float speedMph =
            engineAudio.CurrentSpeedKmh * 0.621371f;

        int roundedSpeed =
            Mathf.RoundToInt(speedMph);

        if (speedText != null)
            speedText.text = roundedSpeed + " MPH";

        if (gearText != null)
        {
            gearText.text =
                "GEAR " + engineAudio.currentGear;
        }

        if (rpmSlider != null)
        {
            rpmSlider.value =
                engineAudio.DisplayedRpmNormalized;
        }

        bool canShift =
            gearController != null &&
            engineAudio.currentGear <
            gearController.maximumGear;

        bool shouldShift =
            canShift &&
            engineAudio.RpmNormalized >=
            shiftWarningPoint;

        if (shiftText != null)
        {
            shiftText.text =
                shouldShift ? "SHIFT!" : "";

            shiftText.color =
                shouldShift
                    ? Color.red
                    : Color.white;
        }

        if (rpmFillImage != null)
        {
            float rpm =
                engineAudio.DisplayedRpmNormalized;

            if (rpm >= 0.85f)
                rpmFillImage.color = Color.red;
            else if (rpm >= 0.65f)
                rpmFillImage.color = Color.yellow;
            else
                rpmFillImage.color = Color.green;
        }
    }
}