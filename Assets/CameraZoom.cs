using UnityEngine;
using Unity.Cinemachine;

public class CameraZoom : MonoBehaviour
{
    public CinemachineCamera virtualCamera; // 비워두면 이 오브젝트에서 자동으로 찾습니다.

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minDistance = 5f;
    public float maxDistance = 30f;

    private CinemachineFollow follow;
    private Vector3 offsetDirection;
    private float currentDistance;

    void Start()
    {
        if (virtualCamera == null) virtualCamera = GetComponent<CinemachineCamera>();
        follow = virtualCamera.GetComponent<CinemachineFollow>();

        currentDistance = follow.FollowOffset.magnitude;
        offsetDirection = follow.FollowOffset.normalized;
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            follow.FollowOffset = offsetDirection * currentDistance;
        }
    }
}
