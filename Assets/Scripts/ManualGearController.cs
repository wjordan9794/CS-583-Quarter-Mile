using UnityEngine;

public class ManualGearController : MonoBehaviour
{
    public PrometeoCarController carController;
    public EngineAudioBySpeed engineAudio;

    [Header("Gear Settings")]
    public int currentGear = 1;
    public int maximumGear = 4;

    public float[] gearTopSpeeds =
    {
        0f,
        51f,
        96f,
        141f,
        190f
    };

    public float[] gearAcceleration =
    {
        0f,
        20f,
        18f,
        16f,
        44f
    };

    [Header("Keyboard Controls")]
    public KeyCode shiftUpKey = KeyCode.E;
    public KeyCode shiftDownKey = KeyCode.Q;

    void Start()
    {
        if (carController == null)
            carController =
                GetComponent<PrometeoCarController>();

        if (engineAudio == null)
            engineAudio =
                GetComponentInChildren<
                    EngineAudioBySpeed
                >();

        ApplyGear();
    }

    void Update()
    {
        if (Input.GetKeyDown(shiftUpKey))
            Upshift();

        if (Input.GetKeyDown(shiftDownKey))
            Downshift();
    }

    public void Upshift()
    {
        if (currentGear >= maximumGear)
            return;

        currentGear++;
        ApplyGear();
    }

    public void Downshift()
    {
        if (currentGear <= 1)
            return;

        currentGear--;
        ApplyGear();
    }

    void ApplyGear()
    {
        currentGear = Mathf.Clamp(
            currentGear,
            1,
            maximumGear
        );

        if (carController != null)
        {
            carController.maxSpeed =
                Mathf.RoundToInt(
                    gearTopSpeeds[currentGear]
                );

            carController.accelerationMultiplier =
                Mathf.RoundToInt(
                    gearAcceleration[currentGear]
                );
        }

        if (engineAudio != null)
            engineAudio.SetGear(currentGear);

        Debug.Log(
            "Current gear: " + currentGear
        );
    }
}