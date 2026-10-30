using System.Collections;
using UnityEngine;

public class NPCPrometeoCarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    [Header("Wheel Meshes")]
    public GameObject frontLeftMesh;
    public GameObject frontRightMesh;
    public GameObject rearLeftMesh;
    public GameObject rearRightMesh;

    [Header("Driving")]
    public float motorTorque = 500f;
    public float maxSpeedKmh = 190f;
    public Vector3 centerOfMass = new Vector3(0f, 0f, 0f);

    [Header("NPC Audio")]
    public EngineAudioBySpeed engineAudio;
    [Range(0.05f, 1f)]
    public float npcEngineVolume = 0.32f;

    [Header("NPC Gears")]
    public int currentGear = 1;
    public int maximumGear = 4;

    // Element 0 is unused.
    public float[] gearTopSpeeds =
    {
        0f,
        51f,
        96f,
        141f,
        190f
    };

    // Reduces acceleration in higher gears.
    // Element 0 is unused.
    public float[] gearTorqueMultipliers =
    {
        0f,
        1.00f,
        0.85f,
        0.70f,
        0.55f
    };

    [Header("Random Shift Timing")]
    [Range(0.8f, 0.99f)]
    public float minimumShiftPoint = 0.90f;

    [Range(0.8f, 0.99f)]
    public float maximumShiftPoint = 0.97f;

    public float minimumShiftDelay = 0.15f;
    public float maximumShiftDelay = 0.45f;

    public float minimumShiftDuration = 0.12f;
    public float maximumShiftDuration = 0.3f;

    private Rigidbody carRigidbody;
    private bool driving;
    private bool shiftScheduled;
    private bool shifting;

    void Awake()
    {
        carRigidbody = GetComponent<Rigidbody>();

        if (carRigidbody == null)
            carRigidbody = gameObject.AddComponent<Rigidbody>();

        carRigidbody.mass = 700f;
        carRigidbody.centerOfMass = centerOfMass;
        carRigidbody.isKinematic = true;

        carRigidbody.constraints =
            RigidbodyConstraints.FreezePositionX |
            RigidbodyConstraints.FreezeRotation;
    }

    void Start()
    {
        if (engineAudio == null)
            engineAudio = GetComponentInChildren<EngineAudioBySpeed>();

        if (engineAudio != null)
        {
            engineAudio.engineVolume = npcEngineVolume;

            if (engineAudio.engineAudio != null)
                engineAudio.engineAudio.volume =
                    npcEngineVolume;

            engineAudio.SetGear(currentGear);
        }
    }

    void FixedUpdate()
    {
        SetNoSteering();

        if (!driving || shifting)
        {
            ApplyMotorTorque(0f);
        }
        else
        {
            float speedKmh =
                Mathf.Abs(carRigidbody.linearVelocity.z) * 3.6f;

            float currentGearTopSpeed =
                gearTopSpeeds[currentGear];

            if (speedKmh < currentGearTopSpeed)
            {
                float gearTorque =
                    motorTorque *
                    gearTorqueMultipliers[currentGear];

                ApplyMotorTorque(gearTorque);
            }
            else
            {
                ApplyMotorTorque(0f);
            }

            if (currentGear < maximumGear &&
                !shiftScheduled)
            {
                float shiftPoint = Random.Range(
                    minimumShiftPoint,
                    maximumShiftPoint
                );

                float shiftSpeed =
                    currentGearTopSpeed * shiftPoint;

                if (speedKmh >= shiftSpeed)
                    StartCoroutine(RandomShift());
            }
        }

        UpdateWheelMeshes();
    }

    IEnumerator RandomShift()
    {
        shiftScheduled = true;

        float delay = Random.Range(
            minimumShiftDelay,
            maximumShiftDelay
        );

        yield return new WaitForSeconds(delay);

        if (!driving)
        {
            shiftScheduled = false;
            yield break;
        }

        shifting = true;
        ApplyMotorTorque(0f);

        float shiftDuration = Random.Range(
            minimumShiftDuration,
            maximumShiftDuration
        );

        yield return new WaitForSeconds(shiftDuration);

        currentGear++;

        if (engineAudio != null)
            engineAudio.SetGear(currentGear);

        shifting = false;
        shiftScheduled = false;
    }

    public void StartDriving()
    {
        carRigidbody.isKinematic = false;
        driving = true;
        currentGear = 1;

        if (engineAudio != null)
            engineAudio.SetGear(currentGear);
    }

    public void StopDriving()
    {
        driving = false;
        shifting = false;
        shiftScheduled = false;

        StopAllCoroutines();
        ApplyMotorTorque(0f);

        carRigidbody.linearVelocity = Vector3.zero;
        carRigidbody.angularVelocity = Vector3.zero;
    }

    void SetNoSteering()
    {
        if (frontLeftCollider != null)
            frontLeftCollider.steerAngle = 0f;

        if (frontRightCollider != null)
            frontRightCollider.steerAngle = 0f;
    }

    void ApplyMotorTorque(float torque)
    {
        if (frontLeftCollider != null)
            frontLeftCollider.motorTorque = torque;

        if (frontRightCollider != null)
            frontRightCollider.motorTorque = torque;

        if (rearLeftCollider != null)
            rearLeftCollider.motorTorque = torque;

        if (rearRightCollider != null)
            rearRightCollider.motorTorque = torque;
    }

    void UpdateWheelMeshes()
    {
        UpdateWheel(frontLeftCollider, frontLeftMesh);
        UpdateWheel(frontRightCollider, frontRightMesh);
        UpdateWheel(rearLeftCollider, rearLeftMesh);
        UpdateWheel(rearRightCollider, rearRightMesh);
    }

    void UpdateWheel(
        WheelCollider wheelCollider,
        GameObject wheelMesh
    )
    {
        if (wheelCollider == null || wheelMesh == null)
            return;

        wheelCollider.GetWorldPose(
            out Vector3 wheelPosition,
            out Quaternion wheelRotation
        );

        wheelMesh.transform.position = wheelPosition;
        wheelMesh.transform.rotation = wheelRotation;
    }
}