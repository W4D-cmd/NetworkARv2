using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Transform head;

    void LateUpdate()
    {
        if (head == null) return;
        Vector3 dir = transform.position - head.position;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
    }
}