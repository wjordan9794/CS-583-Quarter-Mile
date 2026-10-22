using UnityEngine;

public class CameraFollowerMine : MonoBehaviour
{
    public Transform target;
    public float zOffset = 0f;

    void LateUpdate()
    {
        if (target == null)
            return;

        // Keep the camera's X and Y fixed.
        // Match the car's Z exactly.
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            target.position.z + zOffset
        );
    }
}