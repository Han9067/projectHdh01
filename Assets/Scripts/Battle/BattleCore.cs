using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using GB;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System;

public class BtGrid
{
    public float x, y;
    public int tId;
}
public enum BtObjState
{
    IDLE, DEAD, READY, MOVE, ATTACK, TRACK
}
public enum BtObjType
{
    NONE, PLAYER, MONSTER, NPC
}
public enum BtFaction
{
    PLAYER, ALLY, ENEMY
}
public static class Directions
{
    public static readonly Vector2Int[] Dir8 = {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };
}
public class TurnData
{
    public int objId, mIdx = 0, tgId = 0; // 해당 턴 오브젝트 아이디, 이동 인덱스, 타깃 아이디
    public Vector2Int pos; // 해당 턴 오브젝트 위치
    public BtObjState state;
    public BtObjType type;
    public BtFaction faction;
    public Vector2Int[] mPath;
    public bool isAction = false;
    public TurnData(int objId, BtObjState state, BtObjType type, BtFaction faction, Vector2Int pos)
    {
        this.objId = objId;
        this.state = state;
        this.type = type;
        this.faction = faction;
        this.pos = pos;
    }
}

public class BattleCore : AutoSingleton<BattleCore>
{
    #region ==== Global Variable ====
    [Header("====Camera====")]
    public Camera cmr; // 전투씬 메인 카메라
    private Vector3 velocity = Vector3.zero; //카메라 속도
    [Header("====UI====")]
    public GameObject dmgTxtParent;
    private List<Text> dmgTxtList = new List<Text>();
    [Header("====Map====")]
    [SerializeField] private GameObject tileMapObj; // 맵 타일 오브젝트
    public int mapSeed, pDir; // [맵 시드], [플레이어 방향 상,하,좌,우]
    public BtGrid[,] gGrid; // 땅타일 그리드
    // ========================================
    // 🎮 gGrid 내부 tId 관련 내용 => 0~99 -> 타일종류, 3자리 숫자 -> 환경 오브젝트, 4자리 숫자 -> 플레이어(1000 고정), NPC, 몬스터
    // ========================================
    private Tilemap gMap; // 땅 타일 맵
    private int mapW, mapH; // 맵 너비, 맵 높이, 플레이어 x,y좌표
    private Vector2Int cpPos = new Vector2Int(0, 0); //현재 플레이어 위치 좌표
    private float tileOffset = 0.6f, tileItv = 1.2f; // 타일 오프셋, 타일 간격
    float[] mapLimit = new float[4]; // 0 : 상, 1 : 하, 2 : 좌, 3 : 우 맵 타일 제한
    public GameObject[,] guide; // 길찾기 가이드 오브젝트

    [Header("====Player====")]
    [SerializeField] private GameObject pObj;
    public GameObject focus, propParent; // 플레이어, 포커스, 환경, 물건 프리팹 부모, 프리팹

    private SpriteRenderer focusSrp;
    private bPlayer pData;
    private bool isActionable = false, isMove = false; // 플레이어 행동 가능 여부, 플레이어 이동 중인지 여부

    [Header("====Monster====")]
    private Dictionary<int, GameObject> mObj = new Dictionary<int, GameObject>();
    private Dictionary<int, bMonster> mData = new Dictionary<int, bMonster>();
    public Transform monsterParent;

    // [Header("====NPC====")]
    // public GameObject npcPrefab;
    // Dictionary<int, GameObject> nObj = new Dictionary<int, GameObject>();
    // Dictionary<int, bNPC> nData = new Dictionary<int, bNPC>();
    // public Transform npcParent;

    [Header("====Common====")]
    [SerializeField] private int objId;
    private List<TurnData> objTurn = new List<TurnData>();
    private int tIdx = 0; // 턴 인덱스
    // float dTime = 0;
    #endregion
    void Awake()
    {
        if (Time.timeScale == 0) Time.timeScale = 1;
        CheckMainManager();
        //맵 타일 로드
        LoadFieldMap(); // 맵 타일 로드

        focusSrp = focus.GetComponent<SpriteRenderer>();

        objId = 1000;
        LoadPlayerGrp(); // 플레이어 및 플레이어편의 NPC 생성 후 전원 배치
        objId = 2000;
        LoadEnemyGrp(); // 몬스터, 적 NPC 생성 후 전원 배치

        //ps. 여기에서는 아니지만 나중에 맵이 변경 또는 이동되는 특수 지형 및 던전도 대응해야함....ㅠㅠ
    }
    void Start()
    {
        if (cmr == null) cmr = Camera.main;
        MoveCamera(true);
        // FadeIn(); // 페이드인 효과
        isActionable = true;
    }
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // UI 오버레이 영역에서는 포커스 커서 비활성화
            if (!CursorManager.I.IsCursor("default")) CursorManager.I.SetCursor("default");
            if (focus.activeSelf) focus.SetActive(false);
            return;
        }
        if (isActionable)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector2Int t = FindTilePos(mouseWorldPos);
            if (t.x == -1 && t.y == -1)
            {
                if (focus.activeSelf) focus.SetActive(false);
                return;
            }
            focus.transform.position = new Vector3(gGrid[t.x, t.y].x, gGrid[t.x, t.y].y, 0);
            string cName = "";
            if (gGrid[t.x, t.y].tId == 0)
            {
                cName = "default";
                if (!CursorManager.I.IsCursor(cName)) CursorManager.I.SetCursor(cName);
                if (!focus.activeSelf) focus.SetActive(true);
                if (focusSrp.color != Color.white) focusSrp.color = Color.white;
            }
            else
            {
                if (gGrid[t.x, t.y].tId > 2000)
                {
                    cName = "attack";
                    if (focus.activeSelf) focus.SetActive(false);
                    //추후에 해당 타깃 이미지 주변에 아웃라인 강조를 추가하여 선택중이다라는 느낌을 줄 예정

                }
                else if (gGrid[t.x, t.y].tId >= 1000)
                {
                    cName = "default";
                    if (focus.activeSelf) focus.SetActive(false);
                    //추후에 해당 타깃 이미지 주변에 아웃라인 강조를 추가하여 선택중이다라는 느낌을 줄 예정
                }
                else
                {
                    cName = "notMove";
                    if (!focus.activeSelf) focus.SetActive(true);
                    if (focusSrp.color != Color.red) focusSrp.color = Color.red;
                }
                if (!CursorManager.I.IsCursor(cName)) CursorManager.I.SetCursor(cName);
                //공격 커서 상황일때도 가이드라인이 생성되어야할지는 추후 고민
            }

            if (Input.GetMouseButtonDown(0))
            {
                switch (cName)
                {
                    case "attack":
                        if (GetAttackTarget(gGrid[t.x, t.y].tId, cpPos))
                        {
                            objTurn[0].state = BtObjState.ATTACK;
                            objTurn[0].tgId = gGrid[t.x, t.y].tId;
                            TurnAction();
                        }
                        else
                        {
                            OnMovePlayer(t, 1);
                        }
                        break;
                    case "default":
                        if (!focus.activeSelf) return;
                        OnMovePlayer(t);
                        break;
                }
            }

        }
        if (isMove)
            MoveCamera(false);
    }
    #region ==== 🎨 LOAD BATTLE SCENE ====
    void LoadFieldMap()
    {
        if (gGrid != null)
            gGrid = null;
        if (mapSeed != 0)
        {
            //추후에 맵 시드에 맞춰서 맵 생성
        }
        else
        {
            if (tileMapObj == null)
                tileMapObj = GameObject.FindGameObjectWithTag("tileMapObj");
        }

        gMap = tileMapObj.transform.Find("Ground")?.GetComponent<Tilemap>();
        var pMap = tileMapObj.transform.Find("Prop")?.GetComponent<Tilemap>();

        if (gMap != null)
        {
            var bounds = gMap.cellBounds;
            // 실제 타일이 배치된 최소/최대 좌표 찾기
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    if (gMap.HasTile(pos))
                    {
                        minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                        minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                    }
                }
            }
            // 실제 크기 계산
            mapW = maxX - minX + 1; mapH = maxY - minY + 1;
            gGrid = new BtGrid[mapW, mapH];
            for (int x = 0; x < mapW; x++)
            {
                for (int y = 0; y < mapH; y++)
                {
                    var tilePos = new Vector3Int(minX + x, minY + y, 0);
                    // TileBase gTile = gMap.GetTile(tilePos), pTile = pMap.GetTile(tilePos);
                    var pTile = pMap.GetTile(tilePos);
                    gGrid[x, y] = new BtGrid() { x = tilePos.x * tileItv + tileOffset, y = tilePos.y * tileItv + tileOffset, tId = 0 };
                    if (pTile != null)
                    {
                        gGrid[x, y].tId = int.Parse(pTile.name.Split('_')[2]);
                        var prop = Instantiate(ResManager.GetGameObject("PropObj"), propParent.transform);
                        prop.name = pTile.name;
                        prop.transform.position = new Vector3(gGrid[x, y].x, gGrid[x, y].y, 0);
                        prop.GetComponent<SpriteRenderer>().sprite = ResManager.GetSprite(pTile.name);
                    }
                }
            }
            mapLimit[0] = (mapW / 2) * -1.2f; mapLimit[1] = (mapW / 2) * 1.2f;
            mapLimit[2] = (mapH / 2) * -1.2f; mapLimit[3] = (mapH / 2) * 1.2f;
        }
        pMap.gameObject.SetActive(false);
    }
    void LoadPlayerGrp()
    {
        if (pObj == null)
            pObj = GameObject.FindGameObjectWithTag("Player");
        pData = pObj.GetComponent<bPlayer>();
        //추후 NPC 생성
        if (mapSeed < 1000) // 맵 시드가 1000 미만이면 일반 필드 1001부터는 특수 장소(던전이나 숲 등)
        {
            //추후엔 특정 이벤트(기습, 매복 등) 으로 배치 상황이 특수해질 경우도 대응해야함
            pDir = Random.Range(0, 2); // 0:좌, 1:우 -> 파티의 방향 설정
            int cx = 0, cy = 0;
            switch (pDir)
            {
                case 0:
                    cx = 10; cy = 12;
                    pObj.transform.localScale = new Vector3(-1, 1, 1); //좌측 방향 설정 -> 추후 스케일 변동으로 문제가 있을시 세부작업
                    break;
                case 1: cx = 20; cy = 12; break;
            }

            pObj.transform.position = new Vector3(gGrid[cx, cy].x, gGrid[cx, cy].y, 0);
            cpPos = new Vector2Int(cx, cy);
            gGrid[cx, cy].tId = 1000;
            pData.SetObjLayer(mapH - cy);
            objTurn.Add(new TurnData(1000, BtObjState.READY, BtObjType.PLAYER, BtFaction.PLAYER, cpPos));
        }

    }
    void LoadEnemyGrp()
    {
        // MonManager.I.TestCreateMon(); //테스트용

        if (MonManager.I.BattleMonList.Count > 0)
        {
            int cx = 0, cy = 0;
            //플레이어와 반대 방향에 배치 0:좌, 1:우
            switch (pDir)
            {
                case 0: cx = 20; cy = 12; break;
                case 1: cx = 10; cy = 12; break;
            }
            //추후 핵심 시스템 끝나면 중심점과 rng 값을 조정할 생각 
            int mCnt = MonManager.I.BattleMonList.Count, rx = (mCnt / 2) + 1, ry = (mCnt / 4) + 1;
            var mPos = new List<Vector2Int>();
            while (mCnt > 0)
            {
                int mx = cx + Random.Range(-rx, rx + 1);
                int my = cy + Random.Range(-ry, ry + 1);
                if (mx < 0 || mx >= mapW || my < 0 || my >= mapH)
                    continue;
                if (mPos.Contains(new Vector2Int(mx, my)))
                    continue;
                mPos.Add(new Vector2Int(mx, my));
                mCnt--;
            }
            foreach (var p in mPos)
            {
                var mon = Instantiate(ResManager.GetGameObject("MonObj"), monsterParent);
                var data = mon.GetComponent<bMonster>();
                data.SetDirObj(pDir == 0 ? 1 : -1);
                data.SetMonData(++objId, MonManager.I.BattleMonList[0], gGrid[p.x, p.y].x, gGrid[p.x, p.y].y);
                data.SetObjLayer(mapH - p.y);
                mon.name = "Mon_" + objId;
                mObj.Add(objId, mon);
                mData.Add(objId, data);
                gGrid[p.x, p.y].tId = objId;
                objTurn.Add(new TurnData(objId, BtObjState.IDLE, BtObjType.MONSTER, BtFaction.ENEMY, p));
                //나중에 몬스터가 2x2 또는 3x3 타일 형태로 생성되는데 그때는 왼쪽 상단을 기준으로 좌표가 갱신되도록 함
            }
        }
    }
    #endregion
    Vector2Int FindTilePos(Vector3 worldPos)
    {
        float minDistance = float.MaxValue;
        var result = new Vector2Int(0, 0);

        if (worldPos.x < mapLimit[0] || worldPos.x > mapLimit[1] || worldPos.y < mapLimit[2] || worldPos.y > mapLimit[3])
            return new Vector2Int(-1, -1);

        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                var tilePos = new Vector3(gGrid[x, y].x, gGrid[x, y].y, 0);
                float distance = Vector3.Distance(worldPos, tilePos);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    result.x = x; result.y = y;
                }
            }
        }
        return result;
    }
    #region ==== Object Action ====
    void OnMovePlayer(Vector2Int t, int state = 0)
    {
        focus.SetActive(false);
        isActionable = false; isMove = true;
        //추후 포커스, 가이드 라인 초기화 및 비활성화
        Vector2Int[] pPath = BattlePathManager.I.GetPath(cpPos, t, gGrid);
        if (state == 1)
            pPath = new Vector2Int[] { pPath[0] };
        TurnData pTurn = objTurn[0];
        pTurn.state = BtObjState.MOVE;
        pTurn.mPath = pPath; pTurn.mIdx = 0;
        TurnAction();
    }
    void TurnAction()
    {
        //모든 오브젝트의 턴을 여기서 관리해야할듯...플레이어 떄문에 여기저기 분산으로 제어하니까 코드가 더러워짐
        int tgId = 0;
        var ot = objTurn[tIdx];
        if (ot.state == BtObjState.DEAD)
        {
            NextTurn();
            TurnAction();
            return;
        }
        switch (ot.type)
        {
            case BtObjType.PLAYER:
                switch (ot.state)
                {
                    case BtObjState.READY:
                        isMove = false;
                        isActionable = true;
                        return;
                    case BtObjState.IDLE:
                        break;
                    case BtObjState.MOVE:
                        if (ot.mIdx >= ot.mPath.Length || GetNearbyEnemy() || gGrid[ot.mPath[ot.mIdx].x, ot.mPath[ot.mIdx].y].tId != 0)
                        {
                            //플레이어 이동 종료
                            ot.state = BtObjState.IDLE;
                            isMove = false;
                            isActionable = true;
                            return;
                        }
                        ot.isAction = true;
                        StartCoroutine(MoveObj(pObj, cpPos, ot.mPath[ot.mIdx], 0.3f, () =>
                        {
                            UpdateGrid(cpPos.x, cpPos.y, ot.mPath[ot.mIdx].x, ot.mPath[ot.mIdx].y, 1, 1, 1000);
                            cpPos = ot.mPath[ot.mIdx];
                            pData.SetObjLayer(mapH - cpPos.y);
                            ot.mIdx++;
                        }, () =>
                        {
                            ot.isAction = false;
                        }));
                        break;
                    case BtObjState.ATTACK:
                        StartCoroutine(PlayNormalAttack(pObj, BtObjType.PLAYER, 1000, ot.tgId));
                        ot.state = BtObjState.READY;
                        break;
                }
                break;
            case BtObjType.MONSTER:
                int mId = ot.objId;
                if (ot.tgId == 0)
                {
                    tgId = SearchNearbyAiObj(ot.pos, BtObjType.MONSTER);
                    if (tgId != 0)
                        ot.tgId = tgId;
                    //추후에 버그 발생을 대응하기 위해 타깃이 없으면 해당 타입을 제외한 다른 타입들을 검색하여 씬을 종료시키든 마무리해야함.
                }
                switch (ot.state)
                {
                    case BtObjState.IDLE:
                        if (GetAttackTarget(ot.tgId, ot.pos))
                            StartCoroutine(PlayNormalAttack(mObj[mId], BtObjType.MONSTER, mId, ot.tgId));
                        else
                        {
                            //추적 시작
                            ot.state = BtObjState.TRACK;
                            TrackMon(ot, mId, tIdx == 0 ? 0.3f : 0);
                        }
                        break;
                    case BtObjState.TRACK:
                        TrackMon(ot, mId, tIdx == 0 ? 0.3f : 0);
                        break;
                }
                break;
            case BtObjType.NPC:
                break;
        }
        NextTurn();
    }
    void NextTurn()
    {
        tIdx++; //다음 턴을 위해 턴 인덱스 증가
        if (tIdx >= objTurn.Count) tIdx = 0;
    }
    bool GetAttackTarget(int tId, Vector2Int pos)
    {
        for (int i = 0; i < Directions.Dir8.Length; i++)
        {
            Vector2Int t = pos + Directions.Dir8[i];
            if (t.x < 0 || t.x >= mapW || t.y < 0 || t.y >= mapH)
                continue;
            if (gGrid[t.x, t.y].tId == tId)
                return true;
        }
        return false;
    }
    bool GetNearbyEnemy()
    {
        foreach (var t in objTurn)
        {
            if (t.state == BtObjState.DEAD) continue;
            if (t.faction == BtFaction.ENEMY)
            {
                float dist = Vector2.Distance(cpPos, t.pos);
                if (dist < 1.5f)
                    return true;
            }
        }
        return false;
    }
    IEnumerator MoveObj(GameObject obj, Vector2Int cv, Vector2Int mv, float ct, Action callA = null, Action callB = null)
    {
        var pos = new Vector3(gGrid[mv.x, mv.y].x, gGrid[mv.x, mv.y].y, 0);
        float dir = cv.x == mv.x ? obj.transform.localScale.x : (cv.x > mv.x ? 1f : -1f); //캐릭터 방향 설정
        obj.transform.localScale = new Vector3(dir, 1, 1);
        obj.transform.DOMove(pos, 0.3f); //트윈으로 이동
        callA?.Invoke();
        yield return new WaitForSeconds(ct);
        callB?.Invoke();
        TurnAction();
    }
    void TrackMon(TurnData data, int mId, float ct)
    {
        data.isAction = true; //행동 시작
        if (data.tgId == 1000)
        {
            Vector2Int[] path = BattlePathManager.I.GetPath(data.pos, cpPos, gGrid);
            StartCoroutine(MoveObj(mObj[mId], data.pos, path[0], ct, () =>
            {
                data.isAction = false; //행동 종료
                UpdateGrid(data.pos.x, data.pos.y, path[0].x, path[0].y, 1, 1, mId);
                data.pos = path[0]; //몬스터 위치 업데이트
                mData[mId].SetObjLayer(mapH - path[0].y); //몬스터 레이어 업데이트
            }, () =>
            {
                if (GetAttackTarget(data.tgId, data.pos) || gGrid[path[0].x, path[0].y].tId != 0)
                    data.state = BtObjState.IDLE;
            }));
        }
    }
    IEnumerator PlayNormalAttack(GameObject myObj, BtObjType myType, int myId, int tgId)
    {
        var tgType = GetObjType(tgId);
        if (tgType == BtObjType.NONE) yield break;
        var myPos = myObj.transform.position;
        Vector3 tgPos = Vector3.zero;
        switch (tgType)
        {
            case BtObjType.PLAYER:
                tgPos = pObj.transform.position;
                break;
            case BtObjType.MONSTER:
                tgPos = mObj[tgId].transform.position;
                break;
        }

        Vector3 midPoint = GetMidPoint(myPos, tgPos);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(myObj.transform.DOMove(midPoint, 0.05f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                switch (myType)
                {
                    case BtObjType.PLAYER:
                        switch (tgType)
                        {
                            case BtObjType.MONSTER:
                                mData[tgId].OnDamaged(PlayerManager.I.pData.Att);
                                break;
                            case BtObjType.NPC:
                                break;
                        }
                        break;
                    case BtObjType.MONSTER:
                        switch (tgType)
                        {
                            case BtObjType.PLAYER:
                                pData.OnDamaged(mData[myId].att);
                                break;
                            case BtObjType.MONSTER:
                                break;
                            case BtObjType.NPC:
                                break;
                        }
                        break;
                    case BtObjType.NPC:
                        switch (tgType)
                        {
                            case BtObjType.PLAYER:
                                break;
                            case BtObjType.MONSTER:
                                break;
                            case BtObjType.NPC:
                                break;
                        }
                        break;
                }
            }));
        sequence.Append(myObj.transform.DOMove(myPos, 0.05f)
            .SetEase(Ease.InQuad));
        sequence.Play();

        yield return new WaitForSeconds(0.3f);
        TurnAction();
    }
    Vector3 GetMidPoint(Vector3 myPos, Vector3 tgPos)
    {
        Vector3 dir = (tgPos - myPos).normalized;
        float dist = Vector3.Distance(myPos, tgPos);
        return myPos + (dir * (dist * 0.1f));
    }
    int SearchNearbyAiObj(Vector2Int pos, BtObjType myType)
    {
        int oId = 0;
        float minDist = float.MaxValue;
        switch (myType)
        {
            case BtObjType.MONSTER:
                foreach (var t in objTurn)
                {
                    if (t.type == BtObjType.PLAYER || t.type == BtObjType.NPC)
                    {
                        float dist = Vector2.Distance(pos, t.pos);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            oId = t.objId;
                        }
                    }
                }
                break;
            case BtObjType.NPC:
                break;
        }
        return oId;
    }
    BtObjType GetObjType(int objId)
    {
        foreach (var t in objTurn)
        {
            if (t.state == BtObjState.DEAD) continue;
            if (t.objId == objId)
                return t.type;
        }
        return BtObjType.NONE;
    }

    public void DeathObj(int objId)
    {
        foreach (var t in objTurn)
        {
            if (t.objId == objId)
            {
                t.state = BtObjState.DEAD;
                break;
            }
        }
        RemoveGridId(objId);
        CheckGameClear();
    }
    void CheckGameClear()
    {
        int aliveCnt = 0;
        foreach (var t in objTurn)
        {
            if (t.objId == 1000) continue;
            if (t.state != BtObjState.DEAD) aliveCnt++;
        }
        if (aliveCnt == 0)
            Presenter.Send("BattleMainUI", "OnGameClear");
    }
    void UpdateGrid(int sx, int sy, int tx, int ty, int w, int h, int id)
    {
        if (w == 1 && h == 1)
        {
            gGrid[sx, sy].tId = 0;
            gGrid[tx, ty].tId = id;
        }
        else
        {
            for (int x = sx; x < sx + w; x++)
                for (int y = sy; y < sy + h; y++) gGrid[x, y].tId = 0;

            for (int x = tx; x < tx + w; x++)
                for (int y = ty; y < ty + h; y++) gGrid[x, y].tId = id;
        }
    }
    public void RemoveGridId(int objId)
    {
        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                if (gGrid[x, y].tId == objId)
                    gGrid[x, y].tId = 0;
            }
        }
    }
    #endregion
    #region ==== UI Action ====
    public void ShowDmgTxt(int dmg, Vector3 pos)
    {
        var txt = GetDmgTxt();
        if (txt == null)
        {
            txt = Instantiate(ResManager.GetGameObject("DmgTxt"), dmgTxtParent.transform).GetComponent<Text>();
            dmgTxtList.Add(txt);
        }
        float yyy = pos.y + 0.6f;
        txt.transform.position = new Vector3(pos.x, yyy, 0);
        txt.text = dmg.ToString();
        var damageSequence = DOTween.Sequence();
        damageSequence.SetAutoKill(true);
        damageSequence.Append(txt.transform.DOMoveY(yyy + 0.6f, 0.5f).SetEase(Ease.OutQuad))
                    .Join(txt.DOFade(0f, 1f))
                    .Join(txt.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack))
                    .Append(txt.transform.DOScale(1f, 0.2f))
                    .OnComplete(() =>
                    {
                        txt.gameObject.SetActive(false);
                        txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 1f);
                        txt.transform.localScale = Vector3.one;
                    });
    }
    Text GetDmgTxt()
    {
        foreach (var txt in dmgTxtList)
        {
            if (!txt.gameObject.activeSelf)
            {
                txt.gameObject.SetActive(true);
                return txt;
            }
        }
        return null;
    }
    #endregion
    void MoveCamera(bool isInit)
    {
        var targetPosition = new Vector3(pObj.transform.position.x, pObj.transform.position.y, -10f);
        if (isInit)
            cmr.transform.position = targetPosition;
        else
            cmr.transform.position = Vector3.SmoothDamp(cmr.transform.position, targetPosition, ref velocity, 0.1f);
    }
    void FadeIn()
    {
        // 전투 씬 시작시 페이드인용 암막 이미지
        var blackImg = GameObject.FindGameObjectWithTag("blackImg").GetComponent<Image>();
        if (!blackImg.gameObject.activeSelf)
            blackImg.gameObject.SetActive(true);
        blackImg.color = new Color(0, 0, 0, 1f);
        blackImg.DOFade(0f, 1f).OnComplete(() =>
        {
            blackImg.gameObject.SetActive(false);
        });
    }
    public void MoveToWorld()
    {
        SceneManager.LoadScene("World");
    }
    void CheckMainManager()
    {
        if (GameObject.Find("Manager") == null)
        {
            var obj = new GameObject("Manager");
            obj.AddComponent<PlayerManager>();
            obj.AddComponent<MonManager>();
            obj.AddComponent<ItemManager>();
            obj.AddComponent<SaveFileManager>();
            obj.AddComponent<BattlePathManager>();
            obj.AddComponent<CursorManager>();
            obj.AddComponent<WorldObjManager>();
        }
    }
}