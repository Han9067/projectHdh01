using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using GB;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System;
using TMPro;
using UnityEditor;
using System.Linq;
using UnityEngine.Animations;
public class BtGrid
{
    public float x, y;
    public int tId;
}
public enum BtObjState
{
    IDLE, DEAD, READY, MOVE, ATTACK, SKILL, TRACK
}
public enum BtObjType
{
    NONE, PLAYER, MONSTER, NPC
}
public enum BtFaction
{
    ALLY, ENEMY
}
public class TurnData
{
    public int objId, mIdx = 0, tgId = 0, skId = 0; // 해당 턴 오브젝트 아이디, 이동 인덱스, 타깃 아이디, 스킬 아이디
    public int w, h;
    public Vector2Int pos; // 해당 턴 오브젝트 위치
    public BtObjState state;
    public BtObjType type;
    public BtFaction faction;
    public Vector2Int[] mPath;
    public bool isActObj = false; //행동 제어를 위한 변수
    public TurnData(int objId, BtObjState state, BtObjType type, BtFaction faction, Vector2Int pos, int w, int h)
    {
        this.objId = objId;
        this.state = state;
        this.type = type;
        this.faction = faction;
        this.pos = pos;
        this.w = w;
        this.h = h;
    }
}
public class BattleCore : AutoSingleton<BattleCore>
{
    #region ==== Global Variable ====
    [Header("====Camera====")]
    [SerializeField] private Camera cmr; // 전투씬 메인 카메라
    private bool isNotSmooth = false; //카메라의 기본 무빙이 스무스 모드인데 특정 상황에 따라 변동되도록 하기 위한 변수\
    private Vector3 velocity = Vector3.zero; //카메라 속도
    [Header("====UI====")]
    public GameObject dmgTxtParent;
    private List<DmgTxt> dmgTxtList = new List<DmgTxt>();
    public Image bloodScreen;
    [Header("====Map====")]
    private GameObject tileMapObj; // 맵 타일 오브젝트
    public int mapSeed, pDir; // [맵 시드], [플레이어 방향 상,하,좌,우] -> 초기의 플레이어 그룹의 방향을 지정하고 시작하고 나서는 플레이어의 방향으로만 체크
    public BtGrid[,] gGrid; // 땅타일 그리드
    // ========================================
    // 🎮 gGrid 내부 tId 관련 내용 => 0~99 -> 타일종류, 3자리 숫자 -> 환경 오브젝트, 4자리 숫자 -> 플레이어(1000 고정), NPC, 몬스터
    // ========================================
    private Tilemap gMap; // 땅 타일 맵
    private int mapW, mapH; // 맵 너비, 맵 높이, 플레이어 x,y좌표
    private Vector2Int cpPos = new Vector2Int(0, 0); //현재 플레이어 위치 좌표
    private float tileOffset = 0.6f, tileItv = 1.2f; // 타일 오프셋, 타일 간격
    float[] mapLimit = new float[4]; // 0 : 상, 1 : 하, 2 : 좌, 3 : 우 맵 타일 제한
    float[] camLimit = new float[4]; // 0 : 상, 1 : 하, 2 : 좌, 3 : 우 카메라 제한
    public List<GameObject> guideObj = new List<GameObject>(); // 길찾기 가이드 오브젝트
    [SerializeField] private GameObject guideObjParent; // 길찾기 가이드 오브젝트 부모
    [SerializeField] private GameObject rngParent, escParent, propParent, prop2Parent; // 공격 범위 그리드 부모, 탈출존 프리팹, 부모, 환경 프리팹 부모, 환경2 프리팹 부모
    [SerializeField] private List<PropObj> propObj = new List<PropObj>(); // 환경 프리팹 리스트
    [SerializeField] private List<PropObj> prop2Obj = new List<PropObj>(); // 환경 프리팹2 리스트
    [Header("====Rng====")]
    private Vector2Int attPos = new Vector2Int(-1, -1);
    private Vector2Int selSkPos = new Vector2Int(-1, -1);
    private List<RngGrid> attRng = new List<RngGrid>();
    private List<RngGrid> skRng = new List<RngGrid>();
    private int pSkRngType = 0; // 현재 스킬 타입 ->단일인지 다중인지 등
    private int pSkTgType = 0; //0 : 적, 1 : 자신, 2 : 아군 버프
    private int lcx = 0, rcx = 0;//좌, 우 플레이어 또는 NPC, 몬스터의 기준 x좌표

    [Header("====Player====")]
    [SerializeField] private GameObject pObj;
    public GameObject focus; // 플레이어, 포커스, 환경
    private SpriteRenderer focusSrp;
    private bPlayer player; //플레이어
    private bool isActionable = false;// 플레이어 행동 가능 여부
    private bool isMove = false;
    public bool isSk = false; // 스킬 사용 중인지 여부
    private int curUseSkId = 0; // 현재 사용중인 스킬 아이디
    private Vector2Int[] pPath; // 플레이어 이동 경로
    [Header("====Monster====")]
    private Dictionary<int, GameObject> mObj = new Dictionary<int, GameObject>();
    private Dictionary<int, bMonster> mData = new Dictionary<int, bMonster>();
    public Transform monsterParent;

    // [Header("====NPC====")]
    // public GameObject npcPrefab;
    // Dictionary<int, GameObject> nObj = new Dictionary<int, GameObject>();
    // Dictionary<int, bNPC> nData = new Dictionary<int, bNPC>();
    // public Transform npcParent;
    [Header("====Effect====")]
    public GameObject effParent;
    public Dictionary<string, List<SkEffObj>> effList = new Dictionary<string, List<SkEffObj>>();
    [Header("====Projectile====")]
    public GameObject projParent;
    public List<GameObject> projObj = new List<GameObject>();
    [Header("====Common====")]
    [SerializeField] private int objId;
    [SerializeField] private int curSelObjId = 0;
    private List<TurnData> objTurn = new List<TurnData>();
    private int tIdx = 0; // 턴 인덱스
                          // float dTime = 0;
    private static readonly float[] ang8 = new float[] { -45f, -135f, 135f, 45f, 0f, -90f, 180f, 90f };
    private Vector3 focusBackupPos;
    #endregion
    // string mapDefault = "Tile_101_1";
    void Awake()
    {
        if (Time.timeScale == 0) Time.timeScale = 1;
        CheckMainManager();
        //맵 타일 로드
        LoadFieldMap(); // 맵 타일 로드

        focusSrp = focus.GetComponent<SpriteRenderer>();

        objId = 1000;
        LoadPlayerGrp(); //1000:플레이어, 1001~1500 : 아군 npc, 1501~2000 : 아군 몬스터
        objId = 2000;
        LoadEnemyGrp(); // 2000~2999 : 적 몬스터, 3000~3999 : 적 NPC
        bloodScreen.gameObject.SetActive(false);
        ItemManager.I.ClearDropItem(); // 전투 시작 전 드랍 아이템 초기화
                                       // pDir = (int)player.GetObjDir(); // 전투 시작되면 플레이어의 방향으로 설정
    }
    void Start()
    {
        if (cmr == null) cmr = Camera.main;
        MoveCamera(true);
        // FadeIn(); // 페이드인 효과
        isActionable = true;
        CreateRngObj(8, attRng);
    }
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            // UI 오버레이 영역에서는 포커스 커서 비활성화
            if (!GsManager.I.IsCursor("default")) GsManager.I.SetCursor("default");
            if (focus.activeSelf) focus.SetActive(false);
            return;
        }
        Vector3 wPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        wPos.z = 0;
        if (prop2Obj.Count > 0)
        {
            List<int> idx = new List<int>();
            for (int i = 0; i < prop2Obj.Count; i++)
            {
                Bounds b = prop2Obj[i].bounds;
                bool contains = wPos.x > b.min.x && wPos.x < b.max.x && wPos.y > b.min.y && wPos.y < b.max.y;
                if (contains)
                    idx.Add(i);
                else
                {
                    if (prop2Obj[i].curAlpha != 1f)
                    {
                        prop2Obj[i].onMouse = false;
                        if (!prop2Obj[i].onObj)
                            ChangeAlphaWithProps(i, 1f);
                    }
                }
            }
            if (idx.Count > 0)
            {
                for (int i = 0; i < idx.Count; i++)
                {
                    if (prop2Obj[idx[i]].curAlpha != 0.5f)
                    {
                        prop2Obj[idx[i]].onMouse = true;
                        ChangeAlphaWithProps(idx[i], 0.5f);
                    }
                }
            }
        }
        if (isActionable)
        {
            Vector2Int t = GetTilePos(wPos);
            if (t.x == -1 && t.y == -1)
            {
                if (focus.activeSelf) focus.SetActive(false);
                return;
            }
            focus.transform.position = GetGridToWorldPos(t);
            string cName;
            if (gGrid[t.x, t.y].tId == 0)
            {
                //맨땅
                cName = "default";
                if (!GsManager.I.IsCursor(cName)) GsManager.I.SetCursor(cName);
                if (!focus.activeSelf && !isSk)
                {
                    focus.SetActive(true);
                }
                if (focusSrp.color != Color.white) focusSrp.color = Color.white;
                HideAllOutline();
                if (attRng[0].gameObject.activeSelf && !isSk) HideAllRng();
                ///가이드라인
                if (focusBackupPos != focus.transform.position && !isSk)
                {
                    focusBackupPos = focus.transform.position;
                    //
                    if (pPath != null) pPath = null;
                    pPath = BattlePathManager.I.GetPath(cpPos, t, gGrid);
                    ShowMoveGuide(pPath);
                }
            }
            else
            {
                HideMoveGuide();
                if (gGrid[t.x, t.y].tId > 2000)
                {
                    cName = "attack";
                    if (focus.activeSelf) focus.SetActive(false);
                    HideAllOutline();
                    int mId = gGrid[t.x, t.y].tId;
                    ShowOutline(mId);
                }
                else if (gGrid[t.x, t.y].tId >= 1000)
                {
                    cName = "default";
                    if (focus.activeSelf) focus.SetActive(false);
                    if (gGrid[t.x, t.y].tId == 1000)
                        CheckAttRng(t);
                }
                else
                {
                    cName = "notMove";
                    if (!focus.activeSelf)
                    {
                        focus.SetActive(true);
                        HideAllRng();
                    }
                    if (focusSrp.color != Color.red) focusSrp.color = Color.red;
                }
                if (!GsManager.I.IsCursor(cName)) GsManager.I.SetCursor(cName);
                //공격 커서 상황일때도 가이드라인이 생성되어야할지는 추후 고민
            }

            if (isSk)
            {
                if (skRng.Count > 0)
                    SelSKRngGrid(t);
            }
            #region Input Act
            if (Input.GetMouseButtonDown(0))
            {
                if (isSk)
                {
                    if (!IsUsingSk())
                        return;
                    else
                    {
                        UseSk(curUseSkId);
                        return;
                    }
                }
                switch (cName)
                {
                    case "attack":
                        if (GetAttackTarget(gGrid[t.x, t.y].tId, cpPos, player.pData.Rng, 1, 1))
                        {
                            objTurn[0].state = BtObjState.ATTACK;
                            objTurn[0].tgId = gGrid[t.x, t.y].tId;
                            TurnAction();
                        }
                        else
                            OnMovePlayer(1);
                        break;
                    case "default":
                        if (!focus.activeSelf) return;
                        OnMovePlayer();
                        break;
                }
                InitCursorUI();
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (curSelObjId != 0)
                {
                    UIManager.ShowPopup("SelectPop");
                    Presenter.Send("SelectPop", "SetList", 2);
                }
            }
            #endregion
        }
        if (isMove)
            MoveCamera(isNotSmooth);
    }
    #region ==== 🎨 LOAD BATTLE SCENE ====
    void LoadFieldMap()
    {
        if (gGrid != null)
            gGrid = null;
        //mapSeed -> 1~100 필드 101부터 던전 및 특수 맵
        mapSeed = 1; //던전 맵 시드
        tileMapObj = GameObject.Find($"Tile_{mapSeed}_1");
        if (tileMapObj == null)
            tileMapObj = Instantiate(ResManager.GetGameObject($"Tile_{mapSeed}_1"), transform);
        //lcx, rcx 설정
        lcx = mapSeed < 101 ? 13 : 10;
        rcx = mapSeed < 101 ? 24 : 17;

        gMap = tileMapObj.transform.Find("Bg")?.GetComponent<Tilemap>();
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
                    var pTile = pMap.GetTile(tilePos);
                    gGrid[x, y] = new BtGrid() { x = tilePos.x * tileItv + tileOffset, y = tilePos.y * tileItv + tileOffset, tId = 0 };
                    if (pTile != null)
                    {
                        // string[] data = pTile.name.Split('_');
                        // gGrid[x, y].tId = int.Parse(data[2]);
                        // switch (data[2])
                        // {
                        //     case "11":
                        //         string str = $"{pTile.name.Remove(pTile.name.Length - 2)}_{Random.Range(1, 9)}";
                        //         CreatePropObj("PropObj", $"{str}_1", mapH - y, propObj, gGrid[x, y].x, gGrid[x, y].y, propParent);
                        //         CreatePropObj("Prop2Obj", $"{str}_2", mapH - y, prop2Obj, gGrid[x, y].x, gGrid[x, y].y + 0.6f, prop2Parent);
                        //         break;
                        //     default:
                        //         CreatePropObj("PropObj", pTile.name, mapH - y, propObj, gGrid[x, y].x, gGrid[x, y].y, propParent);
                        //         break;
                        // }
                    }
                    else
                    {
                        //btBlk -> 블럭 타일로 어떠한 동적 오브젝트도 이동 할 수 없음.
                        //btEsc -> 탈출구 타일로 해당 타일에 도달하면 탈출이 된다.
                        var gTile = gMap.GetTile(tilePos);
                        switch (gTile.name)
                        {
                            case "btEsc":
                                gGrid[x, y].tId = 1;
                                //탈출용 오브젝트 생성
                                var obj = Instantiate(ResManager.GetGameObject("EcsObj"), escParent.transform);
                                obj.transform.position = new Vector3(gGrid[x, y].x, gGrid[x, y].y, 0);
                                break;
                            case "btBlk":
                                gGrid[x, y].tId = 2;
                                break;
                            default:
                                gGrid[x, y].tId = 0;
                                break;
                        }
                    }
                }
            }
            mapLimit[0] = (mapW / 2) * -1.2f; mapLimit[1] = (mapW / 2) * 1.2f;
            mapLimit[2] = (mapH / 2) * -1.2f; mapLimit[3] = (mapH / 2) * 1.2f;

            camLimit[0] = mapLimit[0] + 7f; camLimit[1] = mapLimit[1] - 7f;
            camLimit[2] = mapLimit[2] + 4f; camLimit[3] = mapLimit[3] - 4f;
        }
        pMap.gameObject.SetActive(false);
    }
    void LoadPlayerGrp()
    {
        if (pObj == null)
            pObj = GameObject.FindGameObjectWithTag("Player");
        player = pObj.GetComponent<bPlayer>();
        //추후 NPC 생성
        if (mapSeed < 1000) // 맵 시드가 1000 미만이면 일반 필드 1001부터는 특수 장소(던전이나 숲 등)
        {
            //추후엔 특정 이벤트(기습, 매복 등) 으로 배치 상황이 특수해질 경우도 대응해야함
            pDir = Random.Range(0, 2); // 0:좌, 1:우 -> 파티의 방향 설정
            int cx = pDir == 0 ? lcx : rcx, cy = mapSeed < 101 ? 13 : 11;
            player.SetObjDir(pDir == 0 ? -1 : 1);

            var pos = GetStartPos(cx, cy); //추후 문제가 생길수있음...
            pObj.transform.position = new Vector3(gGrid[pos.x, pos.y].x, gGrid[pos.x, pos.y].y, 0);
            cpPos = pos;
            gGrid[pos.x, pos.y].tId = 1000;
            player.SetObjLayer(mapH - cy);
            objTurn.Add(new TurnData(1000, BtObjState.READY, BtObjType.PLAYER, BtFaction.ALLY, cpPos, 1, 1));
        }
    }
    void LoadEnemyGrp()
    {
        WorldObjManager.I.TestCreateMon(); //테스트용

        if (WorldObjManager.I.btMonList.Count > 0)
        {
            int cx = pDir == 0 ? rcx : lcx, cy = mapSeed < 101 ? 13 : 11, idx = 0;
            //추후 핵심 시스템 끝나면 중심점과 rng 값을 조정할 생각 
            int mCnt = WorldObjManager.I.btMonList.Count, rx = (mCnt / 2) + 1, ry = (mCnt / 4) + 1;
            for (int i = 0; i < mCnt; i++)
            {
                int mx = cx + Random.Range(-rx, rx + 1), my = cy + Random.Range(-ry, ry + 1);
                var p = GetStartPos(mx, my);
                int mId = WorldObjManager.I.btMonList[idx];
                int w = MonManager.I.MonDataList[mId].W, h = MonManager.I.MonDataList[mId].H;
                var mon = Instantiate(ResManager.GetGameObject("MonObj"), monsterParent);
                var data = mon.GetComponent<bMonster>();
                data.SetObjDir(pDir == 0 ? 1 : -1);
                data.SetMonData(++objId, mId, gGrid[p.x, p.y].x, gGrid[p.x, p.y].y);
                data.SetObjLayer(mapH - p.y);
                mon.name = "Mon_" + objId;
                mObj.Add(objId, mon);
                mData.Add(objId, data);
                UpdateGrid(p.x, p.y, p.x, p.y, w, h, objId);
                objTurn.Add(new TurnData(objId, BtObjState.IDLE, BtObjType.MONSTER, BtFaction.ENEMY, p, w, h));
                idx++;
            }
        }
    }
    private Vector2Int GetStartPos(int x, int y)
    {
        if (x >= 0 && x < mapW && y >= 0 && y < mapH && gGrid[x, y].tId == 0)
            return new Vector2Int(x, y);
        int maxRadius = Mathf.Max(mapW, mapH);
        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    // 현재 radius의 “테두리”만 보려면: max(|dx|,|dy|) == radius
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != radius)
                        continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || nx >= mapW || ny < 0 || ny >= mapH)
                        continue;
                    if (gGrid[nx, ny].tId == 0)
                        return new Vector2Int(nx, ny);
                }
            }
        }
        return new Vector2Int(0, 0);
    }
    private void CreateRngObj(int cnt, List<RngGrid> list, int type = 0)
    {
        if (cnt == 0) return;
        int idx = list.Count;
        for (int i = 0; i < cnt; i++)
        {
            var obj = Instantiate(ResManager.GetGameObject("RngObj"), rngParent.transform);
            var rng = obj.GetComponent<RngGrid>();
            rng.SetData(type); //type -> 0 : 일반 그리드, 1 : 스킬 범위 그리드
            obj.name = $"Rng_{type}_{idx + i}";
            obj.SetActive(false);
            list.Add(rng);
        }
    }
    private void CreatePropObj(string prefabName, string name, int layer, List<PropObj> list, float x, float y, GameObject parent)
    {
        var obj = Instantiate(ResManager.GetGameObject(prefabName), parent.transform);
        obj.name = name;
        obj.transform.position = new Vector3(x, y, 0);
        PropObj prop = obj.GetComponent<PropObj>();
        prop.SetProp(name, layer);
        list.Add(prop);
    }
    #endregion
    #region ==== 🎨 GET ====
    private Vector2Int GetTilePos(Vector3 worldPos)
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
    public bool GetIsActPlayer() => isActionable;
    private float GetBodyObj(int objId)
    {
        if (objId == 1000)
            return player.GetObjDir();
        else
            return mData[objId].GetObjDir();
    }
    private float GetObjDir(int objId)
    {
        if (objId == 1000)
            return player.dir;
        else
            return mData[objId].dir;
    }
    bool GetAttackTarget(int tId, Vector2Int pos, int rng, int w, int h)
    {
        int attMinX = pos.x, attMaxX = pos.x + w - 1, attMinY = pos.y, attMaxY = pos.y + h - 1;
        for (int x = pos.x - rng; x < pos.x + w + rng; x++)
        {
            for (int y = pos.y - rng; y < pos.y + h + rng; y++)
            {
                if (x < 0 || x >= mapW || y < 0 || y >= mapH) continue;
                if (x >= pos.x && x < pos.x + w && y >= pos.y && y < pos.y + h) continue;
                int distX = x < attMinX ? attMinX - x : (x > attMaxX ? x - attMaxX : 0);
                int distY = y < attMinY ? attMinY - y : (y > attMaxY ? y - attMaxY : 0);
                if (Mathf.Max(distX, distY) <= rng && gGrid[x, y].tId == tId) return true;
            }
        }
        return false;
    }
    bool GetNearbyEnemy(TurnData data)
    {
        int x0 = data.pos.x, y0 = data.pos.y;
        int w = data.w, h = data.h;

        for (int x = x0 - 1; x <= x0 + w; x++)
        {
            for (int y = y0 - 1; y <= y0 + h; y++)
            {
                if (x >= x0 && x < x0 + w && y >= y0 && y < y0 + h)
                    continue;
                if (x < 0 || x >= mapW || y < 0 || y >= mapH)
                    continue;

                int tId = gGrid[x, y].tId;
                if (tId == 0) continue;

                BtFaction? cellFaction = GetFactionByObjId(tId);
                if (cellFaction.HasValue && cellFaction.Value != data.faction)
                    return true;  // 적 발견
            }
        }
        return false;
    }
    private BtFaction? GetFactionByObjId(int objId)
    {
        foreach (var t in objTurn)
            if (t.objId == objId) return t.faction;
        return null;
    }
    private Vector3 GetTgPos(BtObjType tgType, int tgId)
    {
        switch (tgType)
        {
            case BtObjType.PLAYER:
                return player.bodyObj.transform.position;
            case BtObjType.MONSTER:
                return mData[tgId].bodyObj.transform.position;
            default:
                return Vector3.zero;
        }
    }
    private Vector3 GetNearPos(int myId, int tgId)
    {
        List<Vector3> myPosList = new List<Vector3>();
        List<Vector3> tgPosList = new List<Vector3>();
        for (int a = 0; a < mapW; a++)
        {
            for (int b = 0; b < mapH; b++)
            {
                if (gGrid[a, b].tId == myId)
                    myPosList.Add(new Vector3(gGrid[a, b].x, gGrid[a, b].y, 0));
                if (gGrid[a, b].tId == tgId)
                    tgPosList.Add(new Vector3(gGrid[a, b].x, gGrid[a, b].y, 0));
            }
        }
        float minDist = float.MaxValue;
        Vector3 result = Vector3.zero;
        for (int a = 0; a < myPosList.Count; a++)
        {
            for (int b = 0; b < tgPosList.Count; b++)
            {
                float dist = Vector3.Distance(myPosList[a], tgPosList[b]);
                if (dist < minDist)
                {
                    minDist = dist;
                    result = myPosList[a];
                }
            }
        }
        return result;
    }
    private Vector3 GetEdgePos(int myId, int w, int h, float ang)
    {
        Vector3 pos = Vector3.zero;
        for (int a = 0; a < mapW; a++)
        {
            for (int b = 0; b < mapH; b++)
            {
                if (gGrid[a, b].tId == myId)
                {
                    pos = new Vector3(gGrid[a, b].x, gGrid[a, b].y, 0);
                    break;
                }
            }
        }
        if (w > 1 || h > 1)
            pos -= new Vector3((w - 1) * 0.3f, (h - 1) * 0.3f, 0);
        float add = 0.6f;
        float add2 = 0.3f;
        int val = (int)ang;
        switch (val)
        {
            case -45:
                return pos + new Vector3((-w * 0.3f) - add2, (h * 0.3f) + add2, 0);
            case -135:
                return pos + new Vector3((w * 0.3f) + add2, (h * 0.3f) + add2, 0);
            case 135:
                return pos + new Vector3((w * 0.3f) + add2, (-h * 0.3f) - add2, 0);
            case 45:
                return pos + new Vector3((-w * 0.3f) - add2, (-h * 0.3f) - add2, 0);
            case 0:
                return pos + new Vector3((-w * 0.3f) - add, 0, 0);
            case -90:
                return pos + new Vector3(0, (h * 0.3f) + add, 0);
            case 180:
                return pos + new Vector3((w * 0.3f) + add, 0, 0);
            case 90:
                return pos + new Vector3(0, (-h * 0.3f) - add, 0);
        }
        return Vector3.zero;
    }
    private float GetLookAngle(Vector3 myPos, Vector3 tgPos)
    {
        var ang = Mathf.Atan2(myPos.y - tgPos.y, myPos.x - tgPos.x) * Mathf.Rad2Deg;
        for (int a = 0; a < ang8.Length; a++)
        {
            if (Mathf.Abs(ang - ang8[a]) < 2f)
                return ang8[a];
        }
        return ang;
    }
    private Vector3 GetGridPos(Vector3 attackerWorldPos, int tgId)
    {
        int closestX = -1, closestY = -1;
        float minDistSq = float.MaxValue;

        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                if (gGrid[x, y].tId != tgId) continue;

                Vector3 cellWorld = new Vector3(gGrid[x, y].x, gGrid[x, y].y, 0f);
                float distSq = (attackerWorldPos - cellWorld).sqrMagnitude;
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closestX = x;
                    closestY = y;
                }
            }
        }

        if (closestX < 0) return Vector3.zero;
        return new Vector3(gGrid[closestX, closestY].x, gGrid[closestX, closestY].y, 0f);
    }
    private GameObject GetObj(int oId)
    {
        if (oId == 1000)
            return pObj;
        else
            return mData[oId].gameObject;
    }
    private GameObject GetProjObj()
    {
        foreach (var t in projObj)
        {
            if (!t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(true);
                return t;
            }
        }
        var obj = Instantiate(ResManager.GetGameObject("ProjObj"), projParent.transform);
        projObj.Add(obj);
        return obj;
    }
    private float GetDmgPosY(int objId)
    {
        if (objId == 1000)
            return 1f;
        else
            return mData[objId].dmgPosY;
    }
    private Vector3 GetGridToWorldPos(Vector2Int pos)
    {
        return new Vector3(gGrid[pos.x, pos.y].x, gGrid[pos.x, pos.y].y, 0);
    }
    public Vector2Int GetObjGridPos(int objId)
    {
        return objTurn.Find(obj => obj.objId == objId).pos;
    }
    public int GetDir8Idx(Vector2Int cen, Vector2Int pos)
    {
        var dif = cen - pos;
        var dir8 = DirData.dir8_2;
        for (int a = 0; a < 8; a++)
        {
            if (dif == dir8[a])
                return a;
        }
        return 0;
    }
    public Dictionary<(int x, int y), int> Get3x1RngPos(Vector2Int pos, int idx)
    {
        var arr = new Dictionary<(int x, int y), int>();
        int x = pos.x, y = pos.y;
        switch (idx)
        {
            case 0:
                arr[(x, y)] = 28;
                arr[(x, y - 1)] = 13;
                arr[(x + 1, y)] = 12;
                break;
            case 1:
            case 6:
                arr[(x, y)] = 9;
                arr[(x - 1, y)] = 14;
                arr[(x + 1, y)] = 12;
                break;
            case 2:
                arr[(x, y)] = 25;
                arr[(x - 1, y)] = 14;
                arr[(x, y - 1)] = 13;
                break;
            case 3:
            case 4:
                arr[(x, y)] = 10;
                arr[(x, y + 1)] = 11;
                arr[(x, y - 1)] = 13;
                break;
            case 5:
                arr[(x, y)] = 27;
                arr[(x, y + 1)] = 11;
                arr[(x + 1, y)] = 12;
                break;
            case 7:
                arr[(x, y)] = 26;
                arr[(x, y + 1)] = 11;
                arr[(x - 1, y)] = 14;
                break;
        }
        return arr;
    }
    #endregion
    #region ==== Field Action ====
    private int GetRngType(Dictionary<(int x, int y), int> grid, int x, int y)
    {
        //총 0~14까지 존재하며 그룹은 1~4, 5~10, 11~14 4개 그룹으로 나누어짐
        // 1~4 까지는 1개 선으로 상, 우, 하, 좌 순
        // 5~10 까지는 2개 선으로 상&우, 우&하, 하&좌, 좌&상, 상&하, 좌&우 순
        // 11~14 까지는 3개 선으로 상&좌&우, 우&상&하, 하&좌&우, 좌&상&하 순
        //주어진 x,y을 기준으로 상하좌우 그리드가 존재하는지 체크 
        int idx = 0;
        int up = 0, down = 0, left = 0, right = 0;
        if (grid.ContainsKey((x, y + 1)))
        {
            idx++;
            up = 1;
        }
        if (grid.ContainsKey((x, y - 1)))
        {
            idx++;
            down = 1;
        }
        if (grid.ContainsKey((x - 1, y)))
        {
            idx++;
            left = 1;
        }
        if (grid.ContainsKey((x + 1, y)))
        {
            idx++;
            right = 1;
        }
        switch (idx)
        {
            case 4:
                if (!grid.ContainsKey((x - 1, y + 1)))
                    return 21;
                else if (!grid.ContainsKey((x + 1, y + 1)))
                    return 22;
                else if (!grid.ContainsKey((x + 1, y - 1)))
                    return 23;
                else if (!grid.ContainsKey((x - 1, y - 1)))
                    return 24;
                else
                    return 15;
            case 3:
                return down == 1 && left == 1 && right == 1 ? 1 : up == 1 && left == 1 && down == 1 ? 2 : up == 1 && left == 1 && right == 1 ? 3 : 4;
            case 2:
                var v2 = down == 1 && left == 1 ? 5 : left == 1 && up == 1 ? 6 : up == 1 && right == 1 ? 7 : right == 1 && down == 1 ? 8 :
                    left == 1 && right == 1 ? 9 : 10;
                //5,6,7,8일때, 상반되는 대각선의 그리드가 있는지 체크해줘야함
                switch (v2)
                {
                    case 5:
                        return grid.ContainsKey((x - 1, y - 1)) ? 5 : 25;
                    case 6:
                        return grid.ContainsKey((x - 1, y + 1)) ? 6 : 26;
                    case 7:
                        return grid.ContainsKey((x + 1, y + 1)) ? 7 : 27;
                    case 8:
                        return grid.ContainsKey((x + 1, y - 1)) ? 8 : 28;
                    default:
                        return v2;
                }
            case 1:
                return down == 1 ? 11 : left == 1 ? 12 : up == 1 ? 13 : 14;
            default:
                return 0;
        }
    }
    private void CheckAttRng(Vector2Int t)
    {
        if (!attRng[0].gameObject.activeSelf)
            ShowAttRng(t, 1);
        else
        {
            if (attPos != t)
                ShowAttRng(t, 1);
        }
    }
    private void ShowAttRng(Vector2Int pos, int cnt)
    {
        attPos = pos;
        //해당 함수는 사각형 형태의 공격 범위 그리드를 화면에 보여주는 함수
        //그리드 공식 -> 지정된 홀수 값의 제곱===== cnt 1의 값은 3이고 3부터 1씩 늘어날수록 2씩 증가 (예 : 3,5,7,9...)
        //계산된 그리드의 총 길이에서 정 가운데에 있는 숫자(예: cnt가 1일땐 총 그리드 수는 9이지만 9에서 중앙 숫자인 5일땐 예외처리로 제외시킴)
        //시작 위치는 cen에서 cnt 값만큼 왼쪽 상단(타입맵 좌표는 y의 경우 아래에서 위로 증가함으로 x는 -1, y는 +1를 적용해야함)으로 가서 처음 지정된 홀수값을 두번 반복문 돌려서 생성 -> idx 필참
        HideAllAttRng();
        int sx = pos.x - cnt, sy = pos.y + cnt, val = 3 + ((cnt - 1) * 2), num = val * val;

        var grid = new Dictionary<(int x, int y), int>();
        for (int y = sy; y > sy - val; y--)
        {
            for (int x = sx; x < sx + val; x++)
            {
                if (x < 0 || x >= mapW || y < 0 || y >= mapH) continue;
                grid[(x, y)] = 0; //여기에는 타입을 넣을생각
            }
        }
        grid.Remove((pos.x, pos.y));
        if (grid.Count > attRng.Count) CreateRngObj(grid.Count - attRng.Count, attRng);
        int a = 0;
        foreach (var g in grid)
        {
            int x = g.Key.x, y = g.Key.y;
            attRng[a].SetPos(gGrid[x, y].x, gGrid[x, y].y, x, y);
            attRng[a].SetSpr($"rng_{GetRngType(grid, x, y)}");
            attRng[a].gameObject.SetActive(true);
            a++;
        }
    }
    private void SetSkRng(int sk)
    {
        // 1 : 단일_근접1칸_일반 근접공격, 2 : 단일_근접 2칸_창 일반 공격, 3 : 단일_원거리1칸
        // 11 : 다중_근접 3칸_횡베기 등, 12 : 다중_근접 둥글게 8칸_회전베기 등, 13 : 다중_근접 십자 4칸_좌1우1상1하1칸, 
        // 14 : 다중_근접 십자 8칸_좌2우2상2하2칸, 15 : 다중_근접 직선 3칸_관통 찌르기_창
        if (skRng.Count > 0)
            HideAllSkRng();
        int cnt = 1;
        switch (sk)
        {
            case 11:
                cnt = 3;
                break;
            case 12:
            case 14:
                cnt = 8;
                break;
            case 13:
                cnt = 4;
                break;
            case 15:
                cnt = 3;
                break;
        }
        CreateRngObj(cnt - skRng.Count <= 0 ? 0 : cnt - skRng.Count, skRng, 1);
    }
    private void HideAllAttRng()
    {
        foreach (var rng in attRng)
            rng.gameObject.SetActive(false);
    }
    public void ShowSkRng(int rng, Vector2Int pos, int sk, int tg)
    {
        //rng : 범위, pos : 스킬 사용자 위치, sk : 스킬 타입(단일, 다중 등), tg : 타겟 타입(빈땅, 적, 아군 버프 등)
        ShowAttRng(pos, rng);
        SetSkRng(sk);
        pSkRngType = sk;
        pSkTgType = tg;
    }
    private void SelSKRngGrid(Vector2Int pos)
    {
        //pos가 범위 그리드 내부에 있는지 체크
        selSkPos = new Vector2Int(-1, -1);
        foreach (var rng in attRng)
        {
            if (rng.xx == pos.x && rng.yy == pos.y)
            {
                selSkPos = pos;
                break;
            }
        }
        if (selSkPos.x == -1 && selSkPos.y == -1)
        {
            if (skRng.Count > 0)
                HideAllSkRng();
            return;
        }
        var p = GetGridToWorldPos(pos);
        if (pSkRngType < 4)
        {
            if (!skRng[0].gameObject.activeSelf)
            {
                skRng[0].gameObject.SetActive(true);
                skRng[0].SetSpr($"rng_{0}");
                skRng[0].SetColor(pSkTgType);
            }
            skRng[0].SetPos(p.x, p.y, pos.x, pos.y);
        }
        else
        {
            switch (pSkRngType)
            {
                case 11:
                    //횡베기 등 0번 배열을 중심으로 1번은 좌, 2번은 우로 배치
                    //Get3x1RngPos를 호출할때, pos와 함께, 현재 위치에 대한 인덱스를 보냄
                    // 인덱스의 기준은 왼쪽 상단부터 0,1,2 왼쪽 중앙부터 3,4 왼쪽 하단부터 5,6,7
                    var arr = Get3x1RngPos(pos, GetDir8Idx(pos, cpPos));
                    if (skRng.Count < 3)
                        CreateRngObj(3 - skRng.Count, skRng);
                    int a = 0;
                    if (skRng[0].gameObject.activeSelf && skRng[0].xx == pos.x && skRng[0].yy == pos.y)
                        return;
                    // Debug.Log($"arr: {arr.Count}");
                    foreach (var g in arr)
                    {
                        if (!skRng[a].gameObject.activeSelf)
                        {
                            skRng[a].gameObject.SetActive(true);
                            skRng[a].SetColor(pSkTgType);
                        }
                        int x = g.Key.x, y = g.Key.y;
                        skRng[a].SetSpr($"rng_{g.Value}");
                        skRng[a].SetPos(gGrid[x, y].x, gGrid[x, y].y, x, y);
                        a++;
                    }
                    break;
                case 12:
                    break;
            }
        }
        //방향
        var dir = pos.x - cpPos.x;
        if (dir == 0) return;
        player.SetObjDir(dir > 0 ? -1 : 1);
    }
    private void HideAllSkRng()
    {
        foreach (var rng in skRng)
            rng.gameObject.SetActive(false);
    }
    private void HideAllRng()
    {
        HideAllAttRng();
        HideAllSkRng();
    }
    public bool GetActiveCurPosWithRngGrid(Vector2Int grid)
    {
        foreach (var rng in attRng)
        {
            if (rng.xx == grid.x && rng.yy == grid.y)
                return true;
        }
        return false;
    }
    private void ChangeAlphaWithProps(int idx, float val)
    {
        propObj[idx].SetAlpha(val);
        prop2Obj[idx].SetAlpha(val);
    }
    private void ShowMoveGuide(Vector2Int[] path)
    {
        List<Vector2> pPos = new List<Vector2>();
        pPos.Add(new Vector2(gGrid[cpPos.x, cpPos.y].x, gGrid[cpPos.x, cpPos.y].y));
        for (int i = 0; i < path.Length; i++)
            pPos.Add(new Vector2(gGrid[path[i].x, path[i].y].x, gGrid[path[i].x, path[i].y].y));
        List<Vector2> guidePos = new List<Vector2>();

        for (int i = 1; i < pPos.Count; i++)
        {
            Vector3 prev = pPos[i - 1];
            Vector3 curr = pPos[i];
            // 두 점 사이 중간 좌표
            Vector3 mid = (prev + curr) * 0.5f;
            // 또는: Vector3 mid = Vector3.Lerp(prev, curr, 0.5f);
            guidePos.Add(mid);   // i-1 ~ i 사이 좌표 1개
            guidePos.Add(curr);  // i 좌표
        }

        if (guideObj.Count > 0)
            HideMoveGuide();
        for (int i = 0; i < guidePos.Count; i++)
        {
            if (guideObj.Count > i)
            {
                guideObj[i].SetActive(true);
                guideObj[i].transform.position = new Vector3(guidePos[i].x, guidePos[i].y, 0);
                continue;
            }
            var obj = Instantiate(ResManager.GetGameObject("GuideObj"), guideObjParent.transform);
            obj.transform.position = new Vector3(guidePos[i].x, guidePos[i].y, 0);
            guideObj.Add(obj);
        }
    }
    private void HideMoveGuide()
    {
        if (guideObj.Count > 0)
        {
            if (!guideObj[0].activeSelf) return;
            foreach (var obj in guideObj)
                obj.SetActive(false);
        }
    }
    #endregion
    #region ==== Object Action ====
    void OnMovePlayer(int state = 0)
    {
        focus.SetActive(false);
        HideMoveGuide();
        isActionable = false;
        isMove = true;
        if (state == 1)
            pPath = new Vector2Int[] { pPath[0] };
        TurnData pTurn = objTurn[0];
        pTurn.state = BtObjState.MOVE;
        pTurn.mPath = pPath; pTurn.mIdx = 0;
        TurnAction();
    }
    private void PlayerReady()
    {
        // Debug.Log("READY");
        isSk = false;
        isMove = false;
        isActionable = true;
        Presenter.Send("BattleMainUI", "ReduceSkCt");
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
                        PlayerReady();
                        return;
                    case BtObjState.IDLE:
                        ot.state = BtObjState.READY;
                        //휴식
                        break;
                    case BtObjState.MOVE:
                        if (ot.mIdx >= ot.mPath.Length || GetNearbyEnemy(ot) || gGrid[ot.mPath[ot.mIdx].x, ot.mPath[ot.mIdx].y].tId != 0)
                        {
                            //ot.mIdx가 0이면 처음 이동하는거라 스킵시킨다.
                            if (ot.mIdx > 0)
                            {
                                PlayerReady();
                                return;
                            }
                        }
                        ot.isActObj = true;
                        StartCoroutine(MoveObj(pObj, 1000, cpPos, ot.mPath[ot.mIdx], 0.3f, () =>
                        {
                            Vector2Int nPos = ot.mPath[ot.mIdx];
                            UpdateGrid(cpPos.x, cpPos.y, nPos.x, nPos.y, 1, 1, 1000);
                            cpPos = nPos;
                            ot.pos = nPos;
                            player.SetObjLayer(mapH - nPos.y);
                            ot.mIdx++;
                        }, () =>
                        {
                            ot.isActObj = false;
                        }));
                        break;
                    case BtObjState.ATTACK:
                        switch (player.pData.AtkType)
                        {
                            case 0:
                                ActObjWithMeleeAtt(pObj, BtObjType.PLAYER, 1000, ot.tgId);
                                break;
                            case 1:
                                if (player.pData.EqSlot["Hand2"] == null)
                                {
                                    GsManager.I.ShowTstMsg("Tst_NotAmmo");
                                    return;
                                }
                                AttObjWithRanged(player.bodyObj, BtObjType.PLAYER, 1000, ot.tgId,
                                    player.pData.EqSlot["Hand2"] == null ? 0 : player.pData.EqSlot["Hand2"].ItemId);
                                player.pData.EqSlot["Hand2"].Dur -= 1;
                                if (player.pData.EqSlot["Hand2"].Dur <= 0)
                                {
                                    PlayerManager.I.pData.Inven.Remove(player.pData.EqSlot["Hand2"]);
                                    player.pData.EqSlot["Hand2"] = null;
                                }
                                break;
                        }
                        isActionable = false;
                        ot.state = BtObjState.READY;
                        break;
                    case BtObjState.SKILL:
                        BattleSkManager.I.ActSkill(1000, ot.skId, selSkPos);
                        isActionable = false;
                        ot.state = BtObjState.READY;
                        break;
                }
                break;
            case BtObjType.MONSTER:
                int mId = ot.objId;
                if (ot.tgId == 0)
                {
                    tgId = SearchNearbyAiObj(ot.pos, ot.faction);
                    if (tgId != 0)
                        ot.tgId = tgId;
                    //추후에 버그 발생을 대응하기 위해 타깃이 없으면 해당 타입을 제외한 다른 타입들을 검색하여 씬을 종료시키든 마무리해야함.
                }
                switch (ot.state)
                {
                    case BtObjState.IDLE:
                        var monData = mData[mId];
                        if (GetAttackTarget(ot.tgId, ot.pos, monData.Rng, monData.w, monData.h))
                        {
                            ActObjWithMeleeAtt(mObj[mId], BtObjType.MONSTER, mId, ot.tgId);
                        }
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
    private void SetObjDir(int objId, Vector2Int cv, Vector2Int lv)
    {
        //cv : 현재 위치의 벡터, lv : 바라보는 위치의 벡터
        float dir = cv.x == lv.x ? GetBodyObj(objId) : (cv.x > lv.x ? 1f : -1f);
        if (objId == 1000)
            player.SetObjDir(dir);
        else
            mData[objId].SetObjDir(dir);
    }
    private void SetObjDir(int objId, Vector3 cv, Vector3 lv)
    {
        float ang = Mathf.Atan2(lv.y - cv.y, lv.x - cv.x) * Mathf.Rad2Deg;
        float dir = ang > -90 && ang < 90 ? -1f : 1f;
        if (objId == 1000)
            player.SetObjDir(dir);
        else
            mData[objId].SetObjDir(dir);
    }
    private void IdleObj(int oId)
    {
        //휴식 및 대기
    }
    IEnumerator MoveObj(GameObject obj, int objId, Vector2Int cv, Vector2Int mv, float ct, Action callA = null, Action callB = null)
    {
        SetObjDir(objId, cv, mv);
        // bool isDiagonal = Mathf.Abs(cv.x - mv.x) > 0 && Mathf.Abs(cv.y - mv.y) > 0;
        float dur = 0.2f;
        var pos = new Vector3(gGrid[mv.x, mv.y].x, gGrid[mv.x, mv.y].y, 0);
        obj.transform.DOMove(pos, dur).SetEase(Ease.Linear);
        OnJumpMove(objId, dur);
        callA?.Invoke();
        yield return new WaitForSeconds(ct);
        callB?.Invoke();
        TurnAction();
    }
    public void OnJumpMove(int oid, float dur)
    {
        if (oid == 1000)
            player.OnJump(dur);
        else
            mData[oid].OnJump(dur);
    }
    private void TrackMon(TurnData data, int mId, float ct)
    {
        data.isActObj = true; //행동 시작
        if (data.tgId == 1000)
        {
            // Dictionary 중복 접근 최적화
            var mon = mData[mId];
            // 자기 자신의 ID(mId)를 전달하여 자신이 차지한 공간은 빈 공간으로 취급
            Vector2Int[] path = BattlePathManager.I.GetPath(data.pos, cpPos, gGrid, mon.w, mon.h, mId);
            if (path.Length > 0)
            {
                StartCoroutine(MoveObj(mObj[mId], mId, data.pos, path[0], ct, () =>
                {
                    data.isActObj = false; //행동 종료
                    UpdateGrid(data.pos.x, data.pos.y, path[0].x, path[0].y, mon.w, mon.h, mId);
                    data.pos = path[0]; //몬스터 위치 업데이트
                    mon.SetObjLayer(mapH - path[0].y); //몬스터 레이어 업데이트
                }, () =>
                {
                    if (GetAttackTarget(data.tgId, data.pos, mon.Rng, mon.w, mon.h) || gGrid[path[0].x, path[0].y].tId != 0)
                        data.state = BtObjState.IDLE;
                }));
            }
            else
            {
                // 경로를 찾지 못한 경우
                data.isActObj = false;
                data.state = BtObjState.IDLE;
            }
        }
    }
    private void ActObjWithMeleeAtt(GameObject myObj, BtObjType myType, int myId, int tgId, int attId = 0)
    {
        var tgType = GetObjType(tgId);
        var myPos = myObj.transform.position;
        Vector3 tgPos = GetTgPos(tgType, tgId);
        Vector3 gPos = GetGridPos(myPos, tgId);
        Vector3 midPoint = Vector3.Lerp(myPos, gPos, 0.4f);
        midPoint.z = 0f;

        CompAttAct(myId, myType, tgId, tgType, myPos, tgPos, attId); //공격 연출
        Sequence sequence = DOTween.Sequence();
        sequence.Append(myObj.transform.DOMove(midPoint, 0.1f).SetEase(Ease.InSine)
            // .OnComplete(() =>{
            //     CompAttAct(myId, myType, tgId, tgType, myPos, tgPos, attId);
            // })
            );
        sequence.Append(myObj.transform.DOMove(myPos, 0.05f).SetEase(Ease.InQuad));
        sequence.Play();
    }
    private void AttObjWithRanged(GameObject bodyObj, BtObjType myType, int myId, int tgId, int projId)
    {
        var tgType = GetObjType(tgId);
        if (tgType == BtObjType.NONE) return;
        Vector3 tgPos = GetTgPos(tgType, tgId);
        if (tgPos == Vector3.zero) return;
        tgPos += new Vector3(0, 0.5f, 0);
        var sPos = bodyObj.transform.position +
        (bodyObj.transform.localScale.x > 0 ? new Vector3(-0.5f, 0.5f, 0) : new Vector3(0.5f, 0.5f, 0)); // 시작 위치
        var proj = GetProjObj();
        proj.transform.position = sPos;

        float dist = Vector3.Distance(sPos, tgPos);
        float dur = Mathf.Clamp(dist / 20f, 0.05f, 0.4f);
        float minDistForArc = 1.5f;
        float h = (dist > minDistForArc)
            ? Mathf.Clamp((dist - minDistForArc) * 0.3f, 0f, 2.5f)
            : 0f;

        Vector3 start = sPos;
        Vector3 end = tgPos;
        // ---- 수식 포물선: 위치 = 직선 보간 + 4*h*t*(1-t), 접선 = (end-start) + (0, 4h(1-2t), 0) ----
        DOTween.To(() => 0f, t =>
        {
            // 위치
            Vector3 pos = Vector3.Lerp(start, end, t) + new Vector3(0, 4f * h * t * (1f - t), 0);
            proj.transform.position = pos;

            // 진행 방향(접선) → 스프라이트가 위를 향함: Atan2 - 90°
            Vector3 tangent = (end - start) + new Vector3(0, 4f * h * (1f - 2f * t), 0);
            if (tangent.sqrMagnitude > 0.0001f)
            {
                tangent.Normalize();
                float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - 90f;
                proj.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }, 1f, dur)
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            proj.SetActive(false);
            CompAttAct(myId, myType, tgId, tgType, sPos, tgPos, projId);
        });
    }
    private int SearchNearbyAiObj(Vector2Int pos, BtFaction faction)
    {
        int oId = 0;
        float minDist = float.MaxValue;
        BtFaction tgFaction = faction == BtFaction.ENEMY ? BtFaction.ALLY : BtFaction.ENEMY;
        foreach (var t in objTurn)
        {
            if (t.faction == tgFaction)
            {
                float dist = Vector2.Distance(pos, t.pos);
                if (dist < minDist)
                {
                    minDist = dist;
                    oId = t.objId;
                }
            }
        }
        return oId;
    }
    private BtObjType GetObjType(int objId)
    {
        foreach (var t in objTurn)
        {
            if (t.state == BtObjState.DEAD) continue;
            if (t.objId == objId)
                return t.type;
        }
        return BtObjType.NONE;
    }
    private void CompAttAct(int myId, BtObjType myType, int tgId,
    BtObjType tgType, Vector3 myPos, Vector3 tgPos, int attId)
    {
        SetObjDir(myId, myPos, tgPos); //현재 오브젝트가 타겟을 바라보도록
        // SetObjDir(tgId, tgPos, myPos); //타겟 오브젝트가 현재 오브젝트를 바라보도록
        int att = 0, crt = 0, crtRate = 0; float ct = 0.3f;
        string aniKey = "";
        List<int> dmgList = new List<int>(); //데미지 리스트
        List<bool> crtList = new List<bool>(); //크리티컬 리스트
        List<Vector3> dmgPosList = new List<Vector3>(); //데미지 받는 대상 위치 리스트
        //추후에 데미지 텍스트도 리스트로 관리하여 다중으로 나오는 텍스트에 대한 발생 시간을 제어해야함
        // bool crtOn = false;
        Vector3 myNearPos = GetNearPos(myId, tgId);
        Vector3 tgNearPos = GetNearPos(tgId, myId);
        float ang = GetLookAngle(myNearPos, tgNearPos);
        //공격 대상자
        switch (myType)
        {
            case BtObjType.MONSTER:
                att = mData[myId].att;
                crt = mData[myId].crt;
                crtRate = mData[myId].crtRate;
                break;
            case BtObjType.NPC:
                break;
            default:
                //Player
                att = player.pData.Att;
                crt = player.pData.Crt;
                crtRate = player.pData.CrtRate;
                // att = (int)(att * BattleSkManager.GetSkAttVal(player.pData.SkList[1101], 601) * 0.01f);
                att = attId == 0 ? att : (int)(att * BattleSkManager.GetSkAttVal(player.pData.SkList[attId], 601) * 0.01f);
                aniKey = attId == 0 ? GetMeleeAniKey() : GetSkAniKey(attId);
                break;
        }
        //타겟
        switch (tgType)
        {
            case BtObjType.MONSTER:
                switch (attId)
                {
                    case 1101:
                        for (int i = 0; i < 2; i++)
                        {
                            crtList.Add(GsManager.I.IsCrt(crtRate));
                            dmgList.Add(crtList[i] ? GsManager.I.GetCrtDamage(att, mData[tgId].def, crt) : GsManager.I.GetDamage(att, mData[tgId].def));
                            dmgPosList.Add(new Vector3(tgPos.x, tgPos.y + GetDmgPosY(myId), 0f)); //데미지 텍스트 위치 설정
                        }
                        mData[tgId].OnDamaged(dmgList[0] + dmgList[1], BtFaction.ALLY, myPos);
                        ShowEff(aniKey, GetGridToWorldPos(selSkPos), 0f, () => { TurnAction(); });
                        break;
                    case 1102:
                        var tgPosArr = Get3x1RngPos(selSkPos, GetDir8Idx(selSkPos, cpPos));
                        foreach (var t in tgPosArr)
                        {
                            int id = gGrid[t.Key.x, t.Key.y].tId;
                            if (id == 0) continue;
                            crtList.Add(GsManager.I.IsCrt(crtRate));
                            var dmg = crtList[crtList.Count - 1] ? GsManager.I.GetCrtDamage(att, mData[id].def, crt) : GsManager.I.GetDamage(att, mData[id].def);
                            dmgList.Add(dmg);
                            var oPos = GetObj(id).transform.position;
                            dmgPosList.Add(new Vector3(oPos.x, oPos.y + GetDmgPosY(id), 0f));  //데미지 텍스트 위치 설정
                            mData[id].OnDamaged(dmg, BtFaction.ALLY, myPos);
                        }
                        ShowEff(aniKey, GetEdgePos(myId, 1, 1, ang), ang, () => { TurnAction(); });
                        ct = 0.02f;
                        break;
                    default:
                        var ePos = GetEdgePos(myId, 1, 1, ang);
                        switch (attId)
                        {
                            case 1201:
                            case 1301:
                                ang = GetObjDir(myId) == 1f ? 0f : 180f;
                                ePos = tgPos;
                                ePos.y += 1f;
                                ePos.x += ang == 0f ? 0.5f : -0.5f;
                                break;
                        }
                        crtList.Add(GsManager.I.IsCrt(crtRate));
                        dmgList.Add(crtList[0] ? GsManager.I.GetCrtDamage(att, mData[tgId].def, crt) : GsManager.I.GetDamage(att, mData[tgId].def));
                        dmgPosList.Add(new Vector3(tgPos.x, tgPos.y + GetDmgPosY(myId), 0f));
                        mData[tgId].OnDamaged(dmgList[0], BtFaction.ALLY, myPos);
                        ShowEff(aniKey, ePos, ang, () => { TurnAction(); });
                        break;
                }
                StartCoroutine(SetSqcDmgTxt(dmgList.Count, dmgList, crtList, ct, dmgPosList));
                break;
            case BtObjType.NPC:
                break;
            case BtObjType.PLAYER:
                if (isMove)
                {
                    //이동중인 플레이어가 타겟이면 카메라를 고정시키게함
                    isMove = false;
                    MoveCamera(true);
                }
                crtList.Add(GsManager.I.IsCrt(crtRate));
                dmgList.Add(crtList[0] ? GsManager.I.GetCrtDamage(att, player.pData.Def, crt) : GsManager.I.GetDamage(att, player.pData.Def));
                dmgPosList.Add(new Vector3(tgPos.x, tgPos.y + GetDmgPosY(myId), 0f));
                player.OnDamaged(dmgList[0], myPos);
                ShowEff("N_Att1", GetEdgePos(myId, mData[myId].w, mData[myId].h, ang), ang, () => { TurnAction(); });
                StartCoroutine(SetSqcDmgTxt(1, dmgList, crtList, 0.3f, dmgPosList));
                break;
        }
    }
    private string GetMeleeAniKey()
    {
        bool wpOn;
        int wpType;
        if (player.pData.EqSlot["Hand1"] != null)
        {
            if (player.pData.EqSlot["Hand2"] != null)
                return "N_Att1";
            else
            {
                wpOn = true;
                wpType = player.pData.EqSlot["Hand1"].Type;
            }
        }
        else if (player.pData.EqSlot["Hand2"] != null)
        {
            wpOn = true;
            wpType = player.pData.EqSlot["Hand2"].Type;
        }
        else
            return "N_Att1";
        if (wpOn)
        {
            switch (wpType)
            {
                case 11:
                case 12:
                    return "N_Att2";
                case 13:
                case 14:
                    return "N_Att3";
                case 15:
                case 16:
                    return "N_Att4";
                case 17:
                case 18:
                    return "N_Att1";
                case 19:
                    return "N_Att5";
            }
            return "N_Att1";
        }
        else
            return "N_Att1";
    }
    private string GetSkAniKey(int skId)
    {
        switch (skId)
        {
            case 1101:
                return "W_DoubleSlash";
            case 1102:
                return "W_WideSlash";
            case 1201:
                return "W_HeadSplitter";
            case 1301:
                return "W_Smash";
            default:
                return "N_Att1";
        }
    }
    IEnumerator UseObjTurn(float ct)
    {
        yield return new WaitForSeconds(ct);
        TurnAction();
    }
    public void DeathObj(int objId, BtFaction attacker)
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
        if (attacker == BtFaction.ALLY)
        {
            GetExp(mData[objId].gainExp, attacker);//경험치 획득
            List<MonData.DropData> dropList = mData[objId].monData.DropList;
            for (int i = 0; i < dropList.Count; i++)
            {
                ItemManager.I.SaveDropItem(dropList[i].ItemId, dropList[i].Rate);
            }
        }
        CheckGameClear();
    }
    void CheckGameClear()
    {
        int aCnt = 0, eCnt = 0;
        foreach (var t in objTurn)
        {
            if (t.state == BtObjState.DEAD) continue;
            if (t.faction == BtFaction.ALLY) aCnt++;
            else eCnt++;
        }
        if (eCnt == 0)
            Presenter.Send("BattleMainUI", "OnGameClear");
        else if (aCnt == 0)
            Presenter.Send("BattleMainUI", "OnGameOver");
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
    private void CheckPlayerOverlapProp()
    {
        Bounds b = player.bColl.bounds;
        for (int i = 0; i < prop2Obj.Count; i++)
        {
            if (!prop2Obj[i].gameObject.activeInHierarchy) continue;
            if (b.Intersects(prop2Obj[i].bounds))
            {
                if (!prop2Obj[i].onObj)
                {
                    prop2Obj[i].onObj = true;
                    ChangeAlphaWithProps(i, 0.5f);
                }
            }
            else
            {
                if (prop2Obj[i].onObj)
                {
                    prop2Obj[i].onObj = false;
                    if (prop2Obj[i].curAlpha != 1f)
                        ChangeAlphaWithProps(i, 1f);
                }
            }
        }
    }
    public void TestCheckOverlapProp()
    {
        CheckPlayerOverlapProp();
    }
    #endregion
    #region ==== 스킬 ====
    public void InitBtSk()
    {
        selSkPos = new Vector2Int(-1, -1);
        isSk = false; curUseSkId = 0;
        HideAllRng();
    }
    public Vector2Int GetSelSkPos() => selSkPos;
    public void UseSk(int skId)
    {
        //pSkTgType -> 1 : 빈 땅, 2 : 적, 3 : 버프
        bool onSk;
        var p = selSkPos;
        string str = "";
        switch (pSkTgType)
        {
            case 1:
                onSk = gGrid[p.x, p.y].tId == 0;
                str = "Tst_NotSk";
                break;
            case 2:
                onSk = gGrid[p.x, p.y].tId >= 2000;
                str = "Tst_NotSkTg";
                break;
            case 3:
                onSk = gGrid[p.x, p.y].tId < 2000;
                str = "Tst_NotSkTg";
                break;
            default:
                onSk = true;
                break;
        }
        if (!onSk)
        {
            GsManager.I.ShowTstMsg(str);
            return;
        }
        objTurn[0].state = BtObjState.SKILL;
        objTurn[0].skId = skId;
        TurnAction();
    }
    public bool IsUsingSk() => BattleSkManager.I.IsUsingSk();
    public void BeginSkill(int skId)
    { focus.SetActive(false); isSk = true; curUseSkId = skId; HideMoveGuide(); HideAllAttRng(); }
    public Vector2Int GetPlayerTilePos() => GetTilePos(player.transform.position);
    public void BuffSk(int skId)
    {
        string skName = "";
        Vector3 pos = Vector3.zero;
        switch (skId)
        {
            case 1001:
                pos = player.transform.position;
                skName = "N_Meditation";
                break;
        }
        isActionable = false;
        ShowEff(skName, pos, 0f, () => { objTurn[0].state = BtObjState.READY; TurnAction(); });
    }
    public void DashToTile(Vector2Int tgPos, int oId, Action call = null)
    {
        Vector3 oPos = pObj.transform.position, tPos = new Vector3(gGrid[tgPos.x, tgPos.y].x, gGrid[tgPos.x, tgPos.y].y, 0);

        Vector2Int myPos = GetTilePos(oPos);
        var path = BattlePathManager.I.GetPath(myPos, tgPos, gGrid);
        int len = path.Length;

        float dur = Mathf.Clamp(Vector3.Distance(oPos, tPos) * 0.05f, 0.1f, 1f);
        SetObjDir(oId, myPos, tgPos);
        isMove = true;
        isNotSmooth = true;
        var obj = GetObj(oId);

        var ang = Mathf.Atan2(oPos.y - tPos.y, oPos.x - tPos.x) * Mathf.Rad2Deg;
        if (ang == -90f && ang == 90f)
            ang = GetObjDir(oId) == 1f ? 0f : 180f;
        obj.transform.DOMove(tPos, dur).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (oId == 1000)
            {
                isMove = false;
                isNotSmooth = false;
                UpdateGrid(cpPos.x, cpPos.y, tgPos.x, tgPos.y, 1, 1, 1000);
                cpPos = tgPos;
                objTurn[0].pos = tgPos;
                player.SetObjLayer(mapH - tgPos.y);
            }
            else { }
            call?.Invoke();
        });

        if (oId == 1000)
            player.OnJump(dur);
        else
            mData[oId].OnJump(dur);
        StartCoroutine(ShowLoopEff("N_Dash", len, dur / len, oId, ang));
    }
    IEnumerator ShowLoopEff(string eff, int cnt, float ct, int oId, float ang)
    {
        for (int i = 0; i < cnt; i++)
        {
            ShowEff(eff, GetObj(oId).transform.position, ang);
            yield return new WaitForSeconds(ct);
        }
    }
    public void ActMeleeToTile(Vector2Int pos, int skId)
    {
        ActObjWithMeleeAtt(player.bodyObj, BtObjType.PLAYER, 1000, gGrid[pos.x, pos.y].tId, skId);
    }
    public void Dash(int oId, Vector2Int tgPos)
    {
        DashToTile(tgPos, oId, () => { TurnAction(); });
    }
    public void DashAttack(int oId, Vector2Int tgPos)
    {
        var myPos = GetObjGridPos(oId);
        Vector2Int[] path = BattlePathManager.I.GetPath(myPos, tgPos, gGrid);
        Vector2Int last = path[path.Length - 2];
        DashToTile(last, oId, () =>
        {
            var obj = GetObj(oId);
            ActObjWithMeleeAtt(obj, GetObjType(oId), oId, gGrid[tgPos.x, tgPos.y].tId);
        });
    }
    #endregion
    #region ==== 애니메이션 ====
    public void ShowEff(string effName, Vector3 pos, float angle, Action call = null)
    {
        var eff = GetEffIdx(effName);
        if (eff != null)
        {
            StartBtEff(eff, pos, angle, call);
        }
        else
        {
            var obj = Instantiate(ResManager.GetGameObject(effName), effParent.transform);
            eff = obj.GetComponent<SkEffObj>();
            if (!effList.ContainsKey(effName))
                effList[effName] = new List<SkEffObj>();
            effList[effName].Add(eff);
            StartBtEff(eff, pos, angle, call);
        }
    }
    private void StartBtEff(SkEffObj eff, Vector3 pos, float angle, Action call = null)
    {
        var dir = angle > -90 && angle < 90 ? 1f : -1f;
        var ang = dir == 1f ? angle : angle - 180f;
        eff.transform.position = pos;
        eff.transform.rotation = Quaternion.Euler(0f, 0f, ang);
        eff.transform.localScale = new Vector3(dir, 1, 1);

        if (eff.anim.IsPlaying)
            eff.anim.Stop();
        eff.anim.EndEvent.RemoveAllListeners();
        eff.anim.EndEvent.AddListener(() =>
        {
            eff.gameObject.SetActive(false);
            if (call != null) call();
        });
        eff.anim.Play();
    }
    SkEffObj GetEffIdx(string effName)
    {
        if (!effList.ContainsKey(effName) || effList[effName].Count == 0)
            return null;
        foreach (var t in effList[effName])
        {
            if (!t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(true);
                return t;
            }
        }
        return null;
    }
    #endregion
    #region ==== UI Action ====
    private void InitCursorUI()
    {
        GsManager.I.SetCursor("default");
        focus.SetActive(false);
        HideAllOutline();
        HideAllAttRng();
    }
    public void ShowDmgTxt(int dmg, bool crt, Vector3 pos)
    {
        var txt = GetDmgTxt();
        if (txt == null)
        {
            txt = Instantiate(ResManager.GetGameObject("DmgTxt"), dmgTxtParent.transform).GetComponent<DmgTxt>();
            dmgTxtList.Add(txt);
        }
        else
            txt.gameObject.SetActive(true);
        txt.ShowDmgTxt(dmg, crt, pos);
        // if (crt)
        // {
        //     // StartCoroutine(CoCameraShake(0.22f, 0.18f));
        // }
    }
    private IEnumerator CoCameraShake(float duration, float magnitude)
    {
        // 기준 위치 저장 (로컬/월드 중 현재 카메라 구조에 맞게 선택)
        Vector3 origin = cmr.transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 랜덤 오프셋
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            cmr.transform.localPosition = origin + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // 원위치 복귀
        cmr.transform.localPosition = origin;
    }
    IEnumerator SetSqcDmgTxt(int cnt, List<int> dmgList, List<bool> crtList, float ct, List<Vector3> dmgPosList)
    {
        for (int i = 0; i < cnt; i++)
        {
            ShowDmgTxt(dmgList[i], crtList[i], dmgPosList[i]);
            yield return new WaitForSeconds(ct);
        }
    }

    DmgTxt GetDmgTxt()
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
    public void ShowBloodScreen()
    {
        bloodScreen.DOKill();

        bloodScreen.gameObject.SetActive(true);
        bloodScreen.color = new Color(1f, 1f, 1f, 1f);
        bloodScreen.DOFade(0f, 1f).OnComplete(() =>
        {
            bloodScreen.gameObject.SetActive(false);
        });
    }
    public void ShowOutline(int objId)
    {
        if (!mData[objId].isOutline)
            mData[objId].StateOutline(true);
        curSelObjId = objId;
    }
    public void HideAllOutline()
    {
        foreach (var t in mData)
        {
            if (t.Value.isOutline)
                t.Value.StateOutline(false);
        }
        curSelObjId = 0;
    }

    public void ShowObjInfo()
    {
        UIManager.ShowPopup("ObjInfoPop");
        string data = "";
        data += mData[curSelObjId].mName + "_" + mData[curSelObjId].lv + "_" + mData[curSelObjId].hp + " / " + mData[curSelObjId].maxHp + "_"
         + mData[curSelObjId].att + "_" + mData[curSelObjId].def;
        Presenter.Send("ObjInfoPop", "ObjInfoData", data);
    }
    #endregion
    #region ==== Data Action ====
    public void GetExp(int exp, BtFaction attacker)
    {
        if (attacker == BtFaction.ALLY)
        {
            player.pData.Exp += exp; //일단은 플레이어만 넣어두고 추후에 아군 NPC, 몬스터 경험치 획득 추가
        }
    }
    #endregion
    void MoveCamera(bool isInit)
    {
        var targetPosition = new Vector3(pObj.transform.position.x, pObj.transform.position.y, -10f);
        //카메라가 x,y축에서 제한된 위치를 벗어나면 벗어나지못하게 제한
        targetPosition.x = Mathf.Clamp(targetPosition.x, camLimit[0], camLimit[1]);
        targetPosition.y = Mathf.Clamp(targetPosition.y, camLimit[2], camLimit[3]);
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
        DOTween.KillAll();
        GsManager.gameState = GameState.World; //스테이터스 변경
        UIManager.ChangeScene("World");
    }
    void CheckMainManager()
    {
        if (GameObject.Find("Manager") == null)
        {
            GameObject managerPrefab = ResManager.GetGameObject("Manager");
            if (managerPrefab != null)
            {
                GameObject battleParent = GameObject.Find("Battle");
                if (battleParent != null)
                    Instantiate(managerPrefab, battleParent.transform);
            }
        }
    }
    public void TestPlayer()
    {
        // float ang = player.GetObjDir() == 1f ? 0f : 180f;
        // Vector3 pos = player.transform.position;
        // pos.x += ang == 0f ? 1f : -1f;
        // ShowEff("N_Dash", pos, ang);
        ShowDmgTxt(100, true, player.transform.position);
    }
}
[CustomEditor(typeof(BattleCore))]
public class BattleCoreEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BattleCore myScript = (BattleCore)target;
        if (GUILayout.Button("테스또"))
        {
            myScript.TestPlayer();
        }
        if (GUILayout.Button("테스트 오버레이 프로퍼"))
        {
            myScript.TestCheckOverlapProp();
        }
    }
}