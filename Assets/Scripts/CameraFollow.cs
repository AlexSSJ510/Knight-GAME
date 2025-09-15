using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;       // El Player
    public float smoothSpeed = 0.125f;
    public Vector3 offset;         // Distancia de la cámara al Player
    public bool isStatic = false;  // Si estática, no se mueve

    private void LateUpdate()
    {
        if (target == null || isStatic) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
    }
}
