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
using Unity.VisualScripting;
public class WorldCore : AutoSingleton<WorldCore>
{
    #region ==== Global Variable ====
    [Header("Main")]
    [SerializeField] private Image blackImg;
    private Camera cam;
    private float moveSpd = 20f, zoomSpd = 10f; // 카메라 이동 속도, 줌 속도
    private float minZoom = 5f, maxZoom = 10f;  // 줌 범위
    public static int intoCity = 0, worldWorkId = 0, mOverObjUid = 0, mTraceObjUid = 0;
    //도시 진입, 일 작업, 마우스 오버 몬스터, 추적 몬스터

    [Header("City")]
    [SerializeField] private Transform cityParent;
    [SerializeField] private Dictionary<int, GameObject> cityObjList = new Dictionary<int, GameObject>();
    [Header("Tile")]
    [SerializeField] private Tilemap worldMapTile;
    private Vector3Int lastCellPos = Vector3Int.zero;
    private TileBase lastTile = null;
    [Header("Player")]
    [SerializeField] private wPlayer player;
    private float pSpd = 2f, traceItv = 0f; //플레이어 이동 속도
    private bool isMove = false, isTrace = false;
    private List<Vector3> movePath = new List<Vector3>();
    private int movePathIdx = 0;
    private Vector3 moveTgPos;
    [Header("Monster")]
    [SerializeField] private Transform wMonParent;
    private Dictionary<int, wMon> wMonObj = new Dictionary<int, wMon>();
    [Header("Marker")]
    [SerializeField] private Transform wMarkerParent;
    private Dictionary<int, wMarker> wMarkerObj = new Dictionary<int, wMarker>();
    #endregion
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

        LoadCityObj();

        if (!PlayerManager.I.isObjCreated)
        {
            PlayerManager.I.isObjCreated = true;
            CheckAllAreaWorldMon();
        }
        else
        {
            LoadPlayerPos();
            LoadWorldMon();
            LoadWorldMarker();
        }

        MoveCamera(player.transform.position);

        if (ItemManager.I.isDrop)
        {
            UIManager.ShowPopup("InvenPop");
            Presenter.Send("InvenPop", "OpenRwdPop");
            ItemManager.I.isDrop = false;
        }
    }
    void Update()
    {
        if (InvenPop.isActive || CityEnterPop.isActive || CharInfoPop.isActive || SkillPop.isActive || JournalPop.isActive)
        {
            if (!GsManager.I.IsCursor("default")) GsManager.I.SetCursor("default");
            return;
        }
        Vector3 mPos = cam.ScreenToWorldPoint(Input.mousePosition); //마우스 월드 좌표
        mPos.z = 0f;
        #region Ect Act
        if (mOverObjUid == 0)
            CheckMouseOverMon(mPos);
        else
            CheckCurMouseOverMon(mPos);
        CheckWorldObj();
        SetCursor(mPos); //커서 설정
        #endregion
        #region Player Act
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) moveDirection.y += 1; // 위로 이동
        if (Input.GetKey(KeyCode.S)) moveDirection.y -= 1; // 아래로 이동
        if (Input.GetKey(KeyCode.A)) moveDirection.x -= 1; // 왼쪽 이동
        if (Input.GetKey(KeyCode.D)) moveDirection.x += 1; // 오른쪽 이동
        if (Input.GetMouseButtonDown(0))
        {
            InputPlayerAction();
        }
        if (Input.GetMouseButtonDown(1))
        {
            // 월드 좌표를 셀 좌표로 변환
            Vector3Int cellPos = worldMapTile.WorldToCell(mPos);

            // wGridDic에서 타일 데이터 조회
            if (WorldObjManager.I.wGridDic.TryGetValue((cellPos.x, cellPos.y), out WorldObjManager.wmGrid grid))
            {
                // grid 객체를 사용할 수 있습니다
                // grid.x, grid.y, grid.tName, grid.worldPos, grid.tCost 등의 정보에 접근 가능
                // Debug.Log($"타일 이름: {grid.tName}, 타일 타입: {grid.tType}, 비용: {grid.tCost}");
                if (grid.tType == "f")
                {
                    UIManager.ShowPopup("SelectPop");
                    Presenter.Send("SelectPop", "SetList", 101);
                }
            }
        }
        if (isMove)
            MovePlayer();
        if (isTrace)
            TracePlayer();
        #endregion
        #region Camera Act
        // Time.unscaledDeltaTime을 사용하여 일시정지 상태에서도 일정한 속도로 이동
        cam.transform.position += moveDirection * moveSpd * Time.unscaledDeltaTime;
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize -= scroll * zoomSpd;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        #endregion
    }
    #region 플레이어 관련
    private void MoveCamera(Vector3 v)
    {
        cam.transform.position = new Vector3(v.x, v.y, -10f);
    }
    private void InputPlayerAction()
    {
        if (GsManager.I.IsCursor("notMove")) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;
        SetMovePlayer();
    }
    public void SetMovePlayer()
    {
        if (isTrace)
        {
            wMonObj[mTraceObjUid].TraceObj(false);
            isTrace = false;
        }
        if (isMove) isMove = false;
        mTraceObjUid = 0; //추적 몬스터 초기화
        movePath.Clear(); //이동 경로 초기화
        movePathIdx = 0; //이동 경로 인덱스 초기화
        Vector2 pPos = player.transform.position; //플레이어 위치
        if (mOverObjUid > 0)
        {
            wMonObj[mOverObjUid].OverObj(false);
            wMonObj[mOverObjUid].TraceObj(true);
            mTraceObjUid = mOverObjUid;
            moveTgPos = wMonObj[mOverObjUid].transform.position;
            mOverObjUid = 0;
            isTrace = true;
        }
        else
        {
            moveTgPos = cam.ScreenToWorldPoint(Input.mousePosition);
            isMove = true;
        }
        moveTgPos.z = 0f;
        Vector3Int start = worldMapTile.WorldToCell(pPos);
        Vector3Int end = worldMapTile.WorldToCell(moveTgPos);
        movePath = GetWorldMovePath(start, end, pPos, moveTgPos);
        int xSpd = GsManager.worldSpd;
        if (xSpd == 0)
            Presenter.Send("WorldMainUI", "ChangeGameSpd", "X1");
        else
            Presenter.Send("WorldMainUI", "ChangeGameSpd", "X" + xSpd);
    }
    private void MovePlayer()
    {
        Vector3 curPos = player.transform.position;
        Vector3 tgPos = movePath[movePathIdx];

        Vector3 dir = tgPos - curPos;
        float distToTg = dir.magnitude; //목표 지점까지의 거리
        float moveDist = pSpd * Time.deltaTime;
        // 목표 지점에 도달했는지 확인 (작은 임계값 사용)
        if (distToTg <= moveDist || distToTg < 0.01f)
        {
            movePathIdx++; // 다음 웨이포인트로
            if (movePathIdx >= movePath.Count)
            {
                player.transform.position = tgPos;
                StopPlayer();
            }
        }
        else
        {
            Vector3 moveVector = dir.normalized * moveDist;
            player.transform.position += moveVector;
        }
    }
    private void TracePlayer()
    {
        Vector3 curPos = player.transform.position;
        Vector3 tgPos = movePath[0];
        Vector3 dir = tgPos - curPos;
        Vector3 moveVector = dir.normalized * pSpd * Time.deltaTime;
        player.transform.position += moveVector;
        traceItv += Time.deltaTime;
        if (traceItv >= 0.2f)
        {
            traceItv = 0f;
            Vector3Int sc = worldMapTile.WorldToCell(curPos);
            Vector3Int ec = worldMapTile.WorldToCell(wMonObj[mTraceObjUid].transform.position);
            movePath = GetWorldMovePath(sc, ec, curPos, wMonObj[mTraceObjUid].transform.position);
        }
    }
    private void InitMovingPlayer()
    {
        if (isTrace)
        {
            wMonObj[mTraceObjUid].TraceObj(false);
            isTrace = false;
            mTraceObjUid = 0;
        }
        if (isMove)
            isMove = false;
        movePath.Clear();
        movePathIdx = 0;
        moveTgPos = Vector3.zero;
        SavePlayerPos();
    }
    private void SavePlayerPos()
    {
        PlayerManager.I.worldPos = player.transform.position;
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
        InitMovingPlayer();
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
        else if (worldWorkId > 0)
        {
            //토스트팝업
            UIManager.ShowPopup("WorkPop");
            Presenter.Send("WorkPop", "SetWork", worldWorkId);
            worldWorkId = 0;
        }
    }
    public void StatePlayer(bool on)
    {
        if (player == null) return;
        player.gameObject.SetActive(on);
    }
    public void SetWorldCoreForTutorial()
    {
        AllRemoveWorldMon();
        MoveCamera(new Vector3(-12f, -37f, -10f));
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
    #region 월드맵 오브젝트 생성 관련
    public void CheckAllAreaWorldMon()
    {
        foreach (var area in WorldObjManager.I.areaDataList)
        {
            if (area.Value.curCnt < area.Value.maxCnt)
            {
                CreateWorldMon(area.Value.areaID, area.Value.maxCnt - area.Value.curCnt);
                area.Value.curCnt = area.Value.maxCnt;
            }
        }
    }
    private void CreateWorldMon(int areaID, int remain)
    {
        int[] grpList = WorldObjManager.I.areaDataList[areaID].grpByGrade[PlayerManager.I.pData.Grade].ToArray();

        for (int i = 0; i < remain; i++)
        {
            bool on = true;
            int uId = 0;
            while (on)
            {
                uId = Random.Range(10000000, 99999999); //몬스터 그룹은 8자리 숫자로 구분
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
            wm.SetMonData(uId, areaID, leaderID, mList);
            wm.myPos = WorldObjManager.I.GetSpawnPos(areaID); //현재 몬스터의 위치 -> 몬스터 이동 경로 계산에 사용
            wm.tgPos = SetWorldMonNextPos(wm);
            wm.tgPos.z = 0f;
            wm.SetObjLayer(wm.myPos.y);
            wm.pathIdx = 0;
            wMonObj.Add(uId, wm);
            wm.path = GetWorldMovePath(worldMapTile.WorldToCell(wm.myPos), worldMapTile.WorldToCell(wm.tgPos), wm.myPos, wm.tgPos); //이동경로 설정
            WorldObjManager.I.AddWorldMonData(uId, areaID, leaderID, mList, wm.myPos, wm.tgPos, wm.path);
            // wm.transform.position = wm.myPos;
            wm.transform.position = wm.path[0];
        }
    }
    public void AllRemoveWorldMon()
    {
        foreach (var wMon in wMonObj)
            Destroy(wMon.Value);
        wMonObj.Clear();
        WorldObjManager.I.worldMonDataList.Clear();
        foreach (var area in WorldObjManager.I.areaDataList)
            area.Value.curCnt = 0;
    }
    private void LoadWorldMon()
    {
        foreach (var wMon in WorldObjManager.I.worldMonDataList)
        {
            var obj = Instantiate(ResManager.GetGameObject("wMonObj"), wMonParent);
            obj.name = "wMon_" + wMon.Key;
            var wm = obj.GetComponent<wMon>();
            wm.SetMonData(wMon.Key, wMon.Value.areaID, wMon.Value.ldID, wMon.Value.monList);
            wm.transform.position = wMon.Value.pos;
            wMonObj.Add(wMon.Key, wm);
        }
    }
    private Vector3 SetWorldMonNextPos(wMon wm)
    {
        return WorldObjManager.I.wAreaPos[wm.areaID][Random.Range(0, WorldObjManager.I.wAreaPos[wm.areaID].Count)];
    }
    private void ReCreateWorldMon()
    {
        AllRemoveWorldMon();
        CheckAllAreaWorldMon();
    }
    public void CreateWorldMarker(Vector3 pos, int type)
    {
        int uId = Random.Range(1000000, 9999999); //마커 그룹은 7자리로 구분
        var obj = Instantiate(ResManager.GetGameObject("wMarker"), wMarkerParent);
        obj.name = "Marker" + uId;
        obj.transform.position = pos;
        var wm = obj.GetComponent<wMarker>();
        wm.SetMarkerData(uId, type);
        wMarkerObj.Add(uId, wm);
        WorldObjManager.I.AddWorldMarkerData(uId, type, pos);
        if (type == 1)
            QuestManager.I.curMkUid = uId;
    }
    public void RemoveMarker(int uId)
    {
        Destroy(wMarkerObj[uId].gameObject);
        wMarkerObj.Remove(uId);
    }
    private void LoadWorldMarker()
    {
        foreach (var wMarker in WorldObjManager.I.worldMarkerDataList)
        {
            var obj = Instantiate(ResManager.GetGameObject("wMarker"), wMarkerParent);
            obj.name = "Marker" + wMarker.Key;
            var wm = obj.GetComponent<wMarker>();
            wm.SetMarkerData(wMarker.Key, wMarker.Value.type);
            wm.transform.position = wMarker.Value.pos;
            wMarkerObj.Add(wMarker.Key, wm);
        }
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
    #region 월드맵 오브젝트 제어 관련
    private List<Vector3> GetWorldMovePath(Vector3Int sc, Vector3Int ec, Vector3 sp, Vector3 ep)
    {
        //sc : 시작 셀, ec : 목표 셀, sp : 시작 월드 좌표, ep : 목표 월드 좌표
        List<Vector3> path = WorldObjManager.I.FindPathOptimized(sc, ec);
        if (path == null) return new List<Vector3> { ep };
        if (path.Count > 1)
        {
            path.RemoveAt(0);
            path.RemoveAt(path.Count - 1);
            path.Add(ep);
        }
        else
            path.Add(ep);
        return path;
    }
    private void CheckWorldObj()
    {
        //몬스터
        foreach (var mon in wMonObj)
        {
            MoveMon(mon.Value);
            if (mon.Value.gameObject.activeSelf)
            {
                mon.Value.gameObject.transform.position = mon.Value.myPos;
            }
            // else
            // {

            // }
        }
        //마커
        foreach (var mk in wMarkerObj)
        {
        }
    }
    private void MoveMon(wMon wm)
    {
        Vector3 curPos = wm.myPos;
        Vector3 dir = wm.path[wm.pathIdx] - curPos;
        float distToTg = dir.magnitude; //목표 지점까지의 거리
        float moveDist = wm.mSpd * Time.deltaTime;
        if (distToTg <= moveDist || distToTg < 0.01f)
        {
            wm.pathIdx++;
            if (wm.pathIdx >= wm.path.Count)
            {
                wm.tgPos = SetWorldMonNextPos(wm);
                wm.path = GetWorldMovePath(worldMapTile.WorldToCell(wm.myPos), worldMapTile.WorldToCell(wm.tgPos), wm.myPos, wm.tgPos);
                wm.pathIdx = 0;
            }
        }
        else
            wm.myPos = curPos + dir.normalized * moveDist;
    }
    private void CheckMouseOverMon(Vector3 mPos)
    {
        float val = 0.1f;
        foreach (var mon in wMonObj)
        {
            // if (mon.Value == null || mon.Value.transform == null) continue;
            if (!mon.Value.gameObject.activeSelf || mon.Value.traceSpr.gameObject.activeSelf) continue;
            Vector3 diff = mPos - mon.Value.transform.position;
            float sqr = diff.sqrMagnitude;  // 제곱 거리
            if (sqr <= val)  // 0.2f의 제곱값과 비교
            {
                mOverObjUid = mon.Key;
                mon.Value.TraceObj(true);
                return;
            }
        }
        mOverObjUid = 0;
    }
    private void CheckCurMouseOverMon(Vector3 mPos)
    {
        Vector3 diff = mPos - wMonObj[mOverObjUid].transform.position;
        float sqr = diff.sqrMagnitude;
        if (sqr > 0.1f)
        {
            wMonObj[mOverObjUid].TraceObj(false);
            mOverObjUid = 0;
        }
    }
    #endregion
    #region 커서 관련
    private void SetCursor(Vector3 mPos)
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (!GsManager.I.IsCursor("default")) GsManager.I.SetCursor("default");
            return;
        }
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
    #region 씬 이동
    public void SceneFadeOut()
    {
        SavePlayerPos();
        WorldObjManager.I.UpdateWorldMonData(wMonObj); //몬스터 위치 갱신
        Presenter.Send("WorldMainUI", "SaveAllTime");
        DOTween.KillAll();
        WorldObjManager.I.RemoveWorldMonGrp();
        GsManager.I.gameState = GameState.Battle; //스테이터스 변경
        UIManager.ChangeScene("Battle");
        // blackImg.gameObject.SetActive(true);
        // blackImg.color = new Color(0, 0, 0, 0f);
        // blackImg.DOFade(1f, 0.5f).SetUpdate(true).OnComplete(() => {
        // });
    }
    #endregion
    public void ShowToastPopup()
    {
        Presenter.Send("WorldMainUI", "ShowToastPopup", "Tst_NotEnoughEnergy");
    }
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
        if (GUILayout.Button("토스트 팝업 테스트"))
        {
            myScript.ShowToastPopup();
        }
    }
}