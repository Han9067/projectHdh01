using UnityEngine;

public class CameraFollow2DSmooth : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private float smoothTime = 0.3f;

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (player == null) return;

        // 목표 위치 계산
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, offset.z);

        // 부드럽게 이동
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}