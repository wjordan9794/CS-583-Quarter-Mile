using UnityEngine;

public class NPCDragCar : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    [Header("Visible Wheel Meshes")]
    public Transform frontLeftMesh;
    public Transform frontRightMesh;
    public Transform rearLeftMesh;
    public Transform rearRightMesh;

    [Header("Driving")]
    public float motorTorque = 250f;
    public float maxSpeedKmh = 90f;

    private Rigidbody rb;
    private bool driving = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = 1500f;
        rb.isKinematic = true;

        // Keep the NPC in its lane and prevent steering rotation.
        rb.constraints =
            RigidbodyConstraints.FreezePositionX |
            RigidbodyConstraints.FreezeRotationY;
    }

    void FixedUpdate()
    {
        SetSteering(0f);

        if (driving)
        {
            float speedKmh = rb.linearVelocity.magnitude * 3.6f;

            float torque = 0f;

            if (speedKmh < maxSpeedKmh)
                torque = motorTorque;

            ApplyMotorTorque(torque);
        }
        else
        {
            ApplyMotorTorque(0f);
        }

        UpdateWheelMeshes();
    }

    public void StartDriving()
    {
        rb.isKinematic = false;
        driving = true;
    }

    public void StopDriving()
    {
        driving = false;
        ApplyMotorTorque(0f);
        rb.linearVelocity = Vector3.zero;
    }

    void SetSteering(float angle)
    {
        frontLeftCollider.steerAngle = angle;
        frontRightCollider.steerAngle = angle;
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

    void UpdateWheel(WheelCollider collider, Transform wheelMesh)
    {
        collider.GetWorldPose(out Vector3 position, out Quaternion rotation);
        wheelMesh.position = position;
        wheelMesh.rotation = rotation;
    }
}