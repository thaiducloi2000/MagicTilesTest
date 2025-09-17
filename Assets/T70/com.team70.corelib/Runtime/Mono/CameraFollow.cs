using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public bool follow;
    public Vector3 speed;
    public Transform cameraTransform;
    public Transform followTarget;
    public Vector3 offset;


    [ContextMenu("Read Offset")]
    public void ReadOffset()
    {
        offset = cameraTransform.position - followTarget.position;
    }

    private void Update()
    {
        if (follow == false) return;
        if (followTarget == null) return;
        if (cameraTransform == null) return;

        var cPos = cameraTransform.position;
        var tPos = followTarget.position + offset;

        cPos.x = Mathf.Lerp(cPos.x, tPos.x, speed.x * Time.deltaTime);
        cPos.y = Mathf.Lerp(cPos.y, tPos.y, speed.y * Time.deltaTime);
        cPos.z = Mathf.Lerp(cPos.z, tPos.z, speed.z * Time.deltaTime);

        cameraTransform.position = cPos;
    }
}