using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using GB;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

// public class WorldGrid
// {
//     public int x, y, tId;
// }
public class WorldCore : AutoSingleton<WorldCore>
{
    [Header("Main")]
    [SerializeField] private Image blackImg;
    private Camera cam;
    private float moveSpd = 20f, zoomSpd = 10f; // 카메라 이동 속도, 줌 속도
    private float minZoom = 5f, maxZoom = 10f;  // 줌 범위
    [Header("Tile")]
    [SerializeField] private Tilemap worldMapTile;
    private Vector3Int lastCellPos = Vector3Int.zero;
    private TileBase lastTile = null;

    void Start()
    {
        //월드맵 시작
        cam = Camera.main;
        WorldMainUI worldMainUI = FindObjectOfType<WorldMainUI>();
        worldMainUI.stateGameSpd("X0");

        if (blackImg.gameObject.activeSelf)
            blackImg.gameObject.SetActive(false);
    }
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
        cam.transform.position += moveDirection * moveSpd * Time.unscaledDeltaTime;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * zoomSpd;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);

        Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int currentCellPos = worldMapTile.WorldToCell(mPos);
        if (currentCellPos != lastCellPos)
        {
            lastCellPos = currentCellPos;
            lastTile = worldMapTile.GetTile(currentCellPos);
            if (lastTile == null) return;
            string cName = "";
            switch (lastTile.name)
            {
                case "wt2": cName = "notMove"; break;
                default: cName = "default"; break;
            }
            if (!CursorManager.I.IsCursor(cName)) CursorManager.I.SetCursor(cName);
        }
    }

    #region 씬 이동
    public void SceneFadeOut()
    {
        SceneManager.LoadScene("Battle");
        // blackImg.gameObject.SetActive(true);
        // blackImg.color = new Color(0, 0, 0, 0f);
        // blackImg.DOFade(1f, 0.5f).SetUpdate(true).OnComplete(() => {
        //     SceneManager.LoadScene("Battle");
        // });
    }
    #endregion
}
