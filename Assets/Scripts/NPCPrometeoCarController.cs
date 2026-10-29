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
    public float motorTorque = 1500f;
    public float maxSpeedKmh = 148f;
    public Vector3 centerOfMass = new Vector3(0f, 0f, 0f);

    private Rigidbody carRigidbody;
    private bool driving = false;

void Awake()
{
    carRigidbody = GetComponent<Rigidbody>();

    if (carRigidbody == null)
        carRigidbody = gameObject.AddComponent<Rigidbody>();

    carRigidbody.mass = 700f;

    // Lower center of mass to help prevent flipping.
    carRigidbody.centerOfMass = new Vector3(0f, 0f, 0f);

    // Hold the NPC still until the countdown ends.
    carRigidbody.isKinematic = true;

    // The track runs along Z.
    // Freeze sideways movement and all rotation.
    carRigidbody.constraints =
        RigidbodyConstraints.FreezePositionX |
        RigidbodyConstraints.FreezeRotation;
}

    void FixedUpdate()
    {
        // This NPC never steers.
        frontLeftCollider.steerAngle = 0f;
        frontRightCollider.steerAngle = 0f;

        if (!driving)
        {
            ApplyMotorTorque(0f);
        }
        else
        {
            float speedKmh = carRigidbody.linearVelocity.magnitude * 3.6f;

            if (speedKmh < maxSpeedKmh)
                ApplyMotorTorque(motorTorque);
            else
                ApplyMotorTorque(0f);
        }

        UpdateWheelMeshes();
    }

    public void StartDriving()
    {
        carRigidbody.isKinematic = false;
        driving = true;
    }

    public void StopDriving()
    {
        driving = false;
        ApplyMotorTorque(0f);
        carRigidbody.linearVelocity = Vector3.zero;
    }

    void ApplyMotorTorque(float torque)
    {
        frontLeftCollider.motorTorque = torque;
        frontRightCollider.motorTorque = torque;
        rearLeftCollider.motorTorque = torque;
        rearRightCollider.motorTorque = torque;
    }

    void UpdateWheelMeshes()
    {
        UpdateWheel(frontLeftCollider, frontLeftMesh);
        UpdateWheel(frontRightCollider, frontRightMesh);
        UpdateWheel(rearLeftCollider, rearLeftMesh);
        UpdateWheel(rearRightCollider, rearRightMesh);
    }

    void UpdateWheel(WheelCollider collider, GameObject wheelMesh)
    {
        collider.GetWorldPose(
            out Vector3 wheelPosition,
            out Quaternion wheelRotation
        );

        wheelMesh.transform.position = wheelPosition;
        wheelMesh.transform.rotation = wheelRotation;
    }
}