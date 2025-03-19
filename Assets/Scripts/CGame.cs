using System.Collections;
using System.Collections.Generic;
using GB;
using UnityEngine;

public class CGame : AutoSingleton<CGame>
{
    private float moveSpeed = 20f; // 카메라 이동 속도
    private float zoomSpeed = 10f; // 줌 속도
    private float minZoom = 5f;   // 최소 줌 (가까운 거리)
    private float maxZoom = 10f;  // 최대 줌 (멀리 보기)
    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        GB.Presenter.Send("Map","worldMap");

    }

    // Update is called once per frame
    void Update()
    {

        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection.y += 1; // 위로 이동
        if (Input.GetKey(KeyCode.S)) moveDirection.y -= 1; // 아래로 이동
        if (Input.GetKey(KeyCode.A)) moveDirection.x -= 1; // 왼쪽 이동
        if (Input.GetKey(KeyCode.D)) moveDirection.x += 1; // 오른쪽 이동
        cam.transform.position += moveDirection * moveSpeed * Time.deltaTime;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

    }
}
