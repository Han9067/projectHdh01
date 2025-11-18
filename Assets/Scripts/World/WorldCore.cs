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
using UnityEditor;
public class WorldCore : AutoSingleton<WorldCore>
{
    [Header("Main")]
    [SerializeField] private Image blackImg;
    private Camera cam;
    private float moveSpd = 20f, zoomSpd = 10f; // 카메라 이동 속도, 줌 속도
    private float minZoom = 5f, maxZoom = 10f;  // 줌 범위
    [Header("City")]
    [SerializeField] private Transform cityParent;
    [SerializeField] private Dictionary<int, GameObject> cityObjList = new Dictionary<int, GameObject>();
    [Header("Tile")]
    [SerializeField] private Tilemap worldMapTile;
    private Vector3Int lastCellPos = Vector3Int.zero;
    private TileBase lastTile = null;
    [Header("Player")]
    [SerializeField] private wPlayer player;
    private float pSpd = 5f; //플레이어 이동 속도
    private Vector2 pPos;
    private bool isMove = false;
    [Header("Monster")]
    [SerializeField] private Transform wMonParent;
    private Dictionary<int, GameObject> wMonObj = new Dictionary<int, GameObject>();

    void Awake()
    {
        WorldObjManager.I.CreateWorldMapGrid(worldMapTile); //월드맵 그리드부터 지역, 도로 등 생성
    }
    void Start()
    {
        //월드맵 시작
        cam = Camera.main;
        Presenter.Send("WorldMainUI", "ChangeGameSpd", "X0");
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

        LoadCityObj();

        MoveCamera(player.transform.position);
    }
    void Update()
    {
        if (InvenPop.isActive || CityEnterPop.isActive || CharInfoPop.isActive || SkillPop.isActive || JournalPop.isActive) return;

        #region Player Act
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection.y += 1; // 위로 이동
        if (Input.GetKey(KeyCode.S)) moveDirection.y -= 1; // 아래로 이동
        if (Input.GetKey(KeyCode.A)) moveDirection.x -= 1; // 왼쪽 이동
        if (Input.GetKey(KeyCode.D)) moveDirection.x += 1; // 오른쪽 이동
        InputPlayerAct();
        #endregion
        #region Camera Act
        // Time.unscaledDeltaTime을 사용하여 일시정지 상태에서도 일정한 속도로 이동
        cam.transform.position += moveDirection * moveSpd * Time.unscaledDeltaTime;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * zoomSpd;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        #endregion
        #region Cursor Act
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (!GsManager.I.IsCursor("default")) GsManager.I.SetCursor("default");
        }
        else
        {
            Vector3 mPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int currentCellPos = worldMapTile.WorldToCell(mPos);
            if (currentCellPos != lastCellPos)
            {
                lastCellPos = currentCellPos;
                lastTile = worldMapTile.GetTile(currentCellPos);
                if (lastTile == null) return;
                string cName;
                switch (lastTile.name)
                {
                    case "wt_x": cName = "notMove"; break;
                    default: cName = "default"; break;
                }
                if (!GsManager.I.IsCursor(cName)) GsManager.I.SetCursor(cName);
            }
        }
        #endregion


    }
    #region 플레이어 관련
    private void MoveCamera(Vector3 v)
    {
        cam.transform.position = new Vector3(v.x, v.y, -10f);
    }
    private void InputPlayerAct()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            if (CityEnterPop.isActive) return;
            if (GsManager.I.IsCursor("notMove")) return;
            Vector2 clickWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 playerWorldPos = player.transform.position;

            Vector3Int startCell = worldMapTile.WorldToCell(playerWorldPos);
            Vector3Int endCell = worldMapTile.WorldToCell(clickWorldPos);

            // 기존 이동 취소
            if (isMove)
            {
                player.transform.DOKill();
                isMove = false;
                Presenter.Send("WorldMainUI", "ChangeGameSpd", "X0");
            }
            List<Vector3> path = WorldObjManager.I.FindPathOptimized(startCell, endCell);
            path.RemoveAt(0);
            path.RemoveAt(path.Count - 1);
            path.Add(clickWorldPos);
            path.Insert(0, playerWorldPos);
            MovePlayerPath(path);
        }
    }
    // 경로 이동
    private void MovePlayerPath(List<Vector3> path)
    {
        pPos = path[path.Count - 1];
        isMove = true;
        Presenter.Send("WorldMainUI", "ChangeGameSpd", "X1");
        DOVirtual.DelayedCall(0.08f, () =>
        {
            player.transform.DOPath(path.ToArray(), pSpd, PathType.Linear)
            .SetEase(Ease.Linear)
            .SetSpeedBased(true)
            .OnUpdate(() =>
            {
                player.SetObjLayer(GsManager.I.GetObjLayer(player.transform.position.y));
            })
            .OnComplete(() =>
            {
                StopPlayer();
            });
        }, false);
    }
    private void LoadPlayerPos()
    {
        player.transform.position = PlayerManager.I.worldPos;
    }
    public Vector3 GetPlayerPos()
    {
        return player.transform.position;
    }
    public void StopPlayer()
    {
        isMove = false;
        Presenter.Send("WorldMainUI", "ChangeGameSpd", "X0");

        if (PlayerManager.I.currentCity > 0)
        {
            int id = PlayerManager.I.currentCity;
            Vector3 pos = cityObjList[id].transform.position;
            MoveCamera(pos);
            player.transform.position = pos;
            UIManager.ShowPopup("CityEnterPop");
            Presenter.Send("CityEnterPop", "EnterCity", id);
            StatePlayer(false);
            GsManager.I.SetCursor("default");
        }
    }
    public void StatePlayer(bool on)
    {
        if (player == null) return;
        player.gameObject.SetActive(on);
    }
    #endregion
    #region 월드맵 도시 관련
    void LoadCityObj()
    {
        int idx = 1;
        foreach (Transform child in cityParent)
        {
            cityObjList.Add(idx, child.gameObject);
            idx++;
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
            List<int> mType = grpData.List; //몬스터 그룹 내에 존재하는 몬스터들을 분류해서 배열로 저장 //
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
            wm.SetObjLayer(GsManager.I.GetObjLayer(wm.transform.position.y));
            wMonObj.Add(uId, obj);

            WorldObjManager.I.AddWorldMonData(uId, areaID, leaderID, mList, wm.transform.position);
        }
    }
    void LoadWorldMon()
    {
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
    void ReCreateWorldMon()
    {
        //등급 상향 및 특정 조건으로 인한 몬스터 재배치
    }
    #endregion
    #region 월드맵 도로 관련
    public void MoveRoad(int s, int e)
    {
        List<Vector3> road = PlaceManager.I.CityDic[s].Road[$"{s}_{e}"];
        player.transform.position = road[0];
        MoveCamera(player.transform.position);
        Presenter.Send("WorldMainUI", "ChangeGameSpd", "X1");
        MoveObjectAlongPath(player.transform, road, true, () =>
        {
            Debug.Log("도착 완료!");
            StopPlayer();
        });
    }

    // 확장 가능한 버전
    private void MoveObjectAlongPath(Transform target, List<Vector3> path, bool updateCamera = false, System.Action onComplete = null)
    {
        target.DOPath(path.ToArray(), pSpd, PathType.Linear)
            .SetEase(Ease.Linear)
            .SetSpeedBased(true)
            .SetUpdate(false) // ← timeScale 영향 받음 (또는 생략 가능, 기본값이 false)
            .OnUpdate(() =>
            {
                if (updateCamera)
                    MoveCamera(target.position);
            })
            .OnComplete(() => onComplete?.Invoke());
    }
    #endregion
    #region 씬 이동
    public void SceneFadeOut()
    {
        PlayerManager.I.worldPos = player.transform.position;
        WorldObjManager.I.UpdateWorldMonData(wMonObj);
        Presenter.Send("WorldMainUI", "SaveAllTime");
        UIManager.ChangeScene("Battle");
        // blackImg.gameObject.SetActive(true);
        // blackImg.color = new Color(0, 0, 0, 0f);
        // blackImg.DOFade(1f, 0.5f).SetUpdate(true).OnComplete(() => {
        //     UIManager.ChangeScene("Battle");
        // });
    }
    #endregion
}

[CustomEditor(typeof(WorldCore))]
public class WorldCoreEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldCore myScript = (WorldCore)target;

        if (GUILayout.Button("플레이어 마을 이동"))
        {
            myScript.MoveRoad(1, 2);
        }

    }
}
