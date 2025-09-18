using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using GB;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

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
    [Header("Player")]
    [SerializeField] private wPlayer player;
    public WorldMainUI mainUI;
    private float pSpd = 10f;
    private Vector2 pPos;
    private bool isMove = false;
    void Awake()
    {
        WorldObjManager.I.CreateWorldArea(worldMapTile);
    }
    void Start()
    {
        //월드맵 시작
        cam = Camera.main;
        mainUI.stateGameSpd("X0");

        if (blackImg.gameObject.activeSelf)
            blackImg.gameObject.SetActive(false);
    }
    void Update()
    {
        // if(CityEnterPop.isActive)return;
        // 일시정지 상태에서도 카메라 이동이 가능하도록 Input 처리
        #region Player Act
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection.y += 1; // 위로 이동
        if (Input.GetKey(KeyCode.S)) moveDirection.y -= 1; // 아래로 이동
        if (Input.GetKey(KeyCode.A)) moveDirection.x -= 1; // 왼쪽 이동
        if (Input.GetKey(KeyCode.D)) moveDirection.x += 1; // 오른쪽 이동
        InputPlayerAct();
        UpdatePlayerAct();
        #endregion
        #region Camera Act
        // Time.unscaledDeltaTime을 사용하여 일시정지 상태에서도 일정한 속도로 이동
        cam.transform.position += moveDirection * moveSpd * Time.unscaledDeltaTime;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * zoomSpd;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        #endregion
        #region Cursor Act
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
                case "wt_x": cName = "notMove"; break;
                default: cName = "default"; break;
            }
            if (!CursorManager.I.IsCursor(cName)) CursorManager.I.SetCursor(cName);
        }
        #endregion


    }
    #region 플레이어 관련
    private void InputPlayerAct()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (CityEnterPop.isActive) return;

            pPos = cam.ScreenToWorldPoint(Input.mousePosition);
            isMove = true;
            mainUI.stateGameSpd("X1");
        }
    }

    private void UpdatePlayerAct()
    {
        if (isMove)
        {
            player.transform.position = Vector2.MoveTowards(
                player.transform.position,
                pPos,
                pSpd * Time.deltaTime
            );

            if (Vector2.Distance(player.transform.position, pPos) < 0.01f)
            {
                player.transform.position = pPos;
                stopPlayer();
            }
        }
    }
    public void stopPlayer()
    {
        isMove = false;
        mainUI.stateGameSpd("X0");

        if (PlayerManager.I.currentCity > 0)
        {
            UIManager.ShowPopup("CityEnterPop");
            GB.Presenter.Send("CityEnterPop", "EnterCity", PlayerManager.I.currentCity);
        }
    }
    #endregion
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
