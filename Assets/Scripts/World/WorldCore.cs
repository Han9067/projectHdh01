using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using GB;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class WorldCore : AutoSingleton<WorldCore>
{
    private float moveSpeed = 20f; // 카메라 이동 속도
    private float zoomSpeed = 10f; // 줌 속도
    private float minZoom = 5f;   // 최소 줌 (가까운 거리)
    private float maxZoom = 10f;  // 최대 줌 (멀리 보기)
    private Camera cam;
    public Image blackImg;
    // Start is called before the first frame update
    void Start()
    {
        //월드맵 시작
        cam = Camera.main;
        WorldMainUI worldMainUI = FindObjectOfType<WorldMainUI>();
        worldMainUI.stateGameSpd("X0");

        if(blackImg.gameObject.activeSelf)
            blackImg.gameObject.SetActive(false);
    }
    public void SceneFadeOut()
    {
        SceneManager.LoadScene("Battle");
        // blackImg.gameObject.SetActive(true);
        // blackImg.color = new Color(0, 0, 0, 0f);
        // blackImg.DOFade(1f, 0.5f).SetUpdate(true).OnComplete(() => {
        //     SceneManager.LoadScene("Battle");
        // });
    }

    // Update is called once per frame
    void Update()
    {   
        // if(CityEnterPop.isActive)return;
        // 일시정지 상태에서도 카메라 이동이 가능하도록 Input 처리
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection.y += 1; // 위로 이동
        if (Input.GetKey(KeyCode.S)) moveDirection.y -= 1; // 아래로 이동
        if (Input.GetKey(KeyCode.A)) moveDirection.x -= 1; // 왼쪽 이동
        if (Input.GetKey(KeyCode.D)) moveDirection.x += 1; // 오른쪽 이동
        
        // Time.unscaledDeltaTime을 사용하여 일시정지 상태에서도 일정한 속도로 이동
        cam.transform.position += moveDirection * moveSpeed * Time.unscaledDeltaTime;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * zoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
    }
}
