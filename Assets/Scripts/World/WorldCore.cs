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
using System.Linq;
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
    [Header("Monster")]
    [SerializeField] private Transform wMonParent;
    private Dictionary<int, GameObject> wMonObj = new Dictionary<int, GameObject>();
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

        if (!PlayerManager.I.isObjCreated)
        {
            PlayerManager.I.isObjCreated = true;
            foreach (var area in WorldObjManager.I.areaDataList)
            {
                if (area.Value.curCnt < area.Value.maxCnt)
                {
                    CreateWorldMon(area.Value.areaID, area.Value.maxCnt - area.Value.curCnt);
                    area.Value.curCnt = area.Value.maxCnt;
                }
            }
        }
        else
        {
            LoadPlayerPos();
            LoadWorldMon();
        }
        cam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);
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

    void LoadPlayerPos()
    {
        player.transform.position = PlayerManager.I.worldPos;
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
    #region 월드맵 몬스터 체크 및 생성
    void CreateWorldMon(int areaID, int remain)
    {
        int[] grpList = WorldObjManager.I.areaDataList[areaID].grpByGrade[PlayerManager.I.pData.Grade].ToArray();


        for (int i = 0; i < remain; i++)
        {
            bool on = true;
            int uId = 0;
            while (on)
            {
                uId = Random.Range(10000000, 99999999);
                if (!WorldObjManager.I.worldMonDataList.ContainsKey(uId)) on = false;
            }
            var grpData = grpList.Length == 1 ? WorldObjManager.I.monGrpData[grpList[0]]
                        : WorldObjManager.I.monGrpData[grpList[Random.Range(0, grpList.Length)]]; //몬스터 그룹 데이터 중 랜덤으로 1개 선택
            int leaderID = grpData.LeaderID;
            int mTot = Random.Range(grpData.Min, grpData.Max);
            List<int> mType = grpData.List.Split(',').Select(int.Parse).ToList(); //몬스터 그룹 내에 존재하는 몬스터들을 분류해서 배열로 저장
            List<int> mList = new List<int>();
            if (mType.Count == 1)
            {
                for (int a = 0; a < mTot; a++)
                    mList.Add(mType[0]);
            }
            else
            {
                int back = mTot;
                int idx = 0;
                while (back > 0)
                {
                    bool aOn = Random.Range(0, 2) == 0;
                    if (aOn)
                        mList.Add(mType[idx]);
                    back--;
                    idx++;
                    if (mType.Count <= idx) idx = 0;
                }
            }

            var obj = Instantiate(ResManager.GetGameObject("wMonObj"), wMonParent);
            obj.name = "wMon_" + uId;
            var wm = obj.GetComponent<wMon>();
            wm.SetMonData(uId, leaderID, mList);
            wm.transform.position = WorldObjManager.I.GetSpawnPos(areaID); //구역에 맞춰 몬스터 좌표 갱신
            wMonObj.Add(uId, obj);

            WorldObjManager.I.AddWorldMonData(uId, areaID, leaderID, mList, wm.transform.position);
        }
    }
    void LoadWorldMon()
    {
        wMonObj.Clear();
        foreach (var wMon in WorldObjManager.I.worldMonDataList)
        {
            var obj = Instantiate(ResManager.GetGameObject("wMonObj"), wMonParent);
            obj.name = "wMon_" + wMon.Key;
            var wm = obj.GetComponent<wMon>();
            wm.SetMonData(wMon.Key, wMon.Value.ldID, wMon.Value.monList);
            wm.transform.position = wMon.Value.pos;
            wMonObj.Add(wMon.Key, obj);
        }
    }
    #endregion
    #region 씬 이동
    public void SceneFadeOut()
    {
        PlayerManager.I.worldPos = player.transform.position;
        WorldObjManager.I.UpdateWorldMonData(wMonObj);
        UIManager.ChangeScene("Battle");
        // blackImg.gameObject.SetActive(true);
        // blackImg.color = new Color(0, 0, 0, 0f);
        // blackImg.DOFade(1f, 0.5f).SetUpdate(true).OnComplete(() => {
        //     UIManager.ChangeScene("Battle");
        // });
    }
    #endregion
}
