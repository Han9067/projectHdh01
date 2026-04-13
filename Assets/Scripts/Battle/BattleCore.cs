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
public static class Directions
{
    public static readonly Vector2Int[] Dir8 = {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };
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
    public bool isAction = false;
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
    private Vector3 velocity = Vector3.zero; //카메라 속도
    [Header("====UI====")]
    public GameObject dmgTxtParent;
    // private List<TextMeshProUGUI> dmgTxtList = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> dmgTxtList = new List<TextMeshProUGUI>();
    public Image bloodScreen;
    [Header("====Map====")]
    private GameObject tileMapObj; // 맵 타일 오브젝트
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
    [SerializeField] private GameObject rngParent, propParent, prop2Parent; // 공격 범위 그리드 부모, 환경 프리팹 부모, 환경2 프리팹 부모
    [SerializeField] private List<PropObj> propObj = new List<PropObj>(); // 환경 프리팹 리스트
    [SerializeField] private List<PropObj> prop2Obj = new List<PropObj>(); // 환경 프리팹2 리스트
    private List<RngGrid> attRng = new List<RngGrid>();
    private Vector2Int attRngPos = new Vector2Int(-200, -200);
    private int lcx = 0, rcx = 0;//좌, 우 플레이어 또는 NPC, 몬스터의 기준 x좌표

    [Header("====Player====")]
    [SerializeField] private GameObject pObj;
    public GameObject focus; // 플레이어, 포커스, 환경
    private SpriteRenderer focusSrp;
    private bPlayer player; //플레이어
    private bool isActionable = false;// 플레이어 행동 가능 여부, 플레이어 이동 중인지 여부  
    private bool isMove = false;
    private static bool isSk = false; // 스킬 사용 중인지 여부
    private bool isSkAvailable = false; // 스킬 사용 가능 여부
    private int curUseSkId = 0; // 현재 사용중인 스킬 아이디
    private int pSkType = 0; // 플레이어 스킬 타입(1 : 이동형, 2 : 대상 공격형, 3 : 대상 버프형)
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
    private Vector2Int skPos = new Vector2Int(-1, -1);
    private List<TurnData> objTurn = new List<TurnData>();
    private int tIdx = 0; // 턴 인덱스
                          // float dTime = 0;

    #endregion
    string mapDefault = "Tile_101_1";
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
        bloodScreen.gameObject.SetActive(false);
        ItemManager.I.ClearDropItem(); // 전투 시작 전 드랍 아이템 초기화
    }
    void Start()
    {
        if (cmr == null) cmr = Camera.main;
        MoveCamera(true);
        // FadeIn(); // 페이드인 효과
        isActionable = true;
        CreateRngObj(8);
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
                            changeAlphaWithProps(i, 1f);
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
                        changeAlphaWithProps(idx[i], 0.5f);
                    }
                }
            }
        }
        if (isActionable)
        {
            Vector2Int t = FindTilePos(wPos);
            if (t.x == -1 && t.y == -1)
            {
                if (focus.activeSelf) focus.SetActive(false);
                return;
            }
            focus.transform.position = new Vector3(gGrid[t.x, t.y].x, gGrid[t.x, t.y].y, 0);
            string cName;
            if (gGrid[t.x, t.y].tId == 0)
            {
                //맨땅
                cName = "default";
                if (!GsManager.I.IsCursor(cName)) GsManager.I.SetCursor(cName);
                if (!focus.activeSelf && !isSk) focus.SetActive(true);
                if (focusSrp.color != Color.white) focusSrp.color = Color.white;
                HideAllOutline();
                if (attRng[0].gameObject.activeSelf && !isSk) HideAllRng();
            }
            else
            {
                if (gGrid[t.x, t.y].tId > 2000)
                {
                    cName = "attack";
                    if (focus.activeSelf) focus.SetActive(false);
                    HideAllOutline();
                    int mId = gGrid[t.x, t.y].tId;
                    ShowOutline(mId);
                    if (attRngPos != objTurn[0].pos)
                    {
                        ShowAttRng(objTurn[0].pos, 1, 1, player.pData.Rng);
                        attRngPos = objTurn[0].pos;
                    }
                }
                else if (gGrid[t.x, t.y].tId >= 1000)
                {
                    cName = "default";
                    if (focus.activeSelf) focus.SetActive(false);
                    if (gGrid[t.x, t.y].tId == 1000)
                    {
                        if (attRngPos != t)
                        {
                            ShowAttRng(t, 1, 1, player.pData.Rng);
                            attRngPos = t;
                        }
                    }
                }
                else
                {
                    cName = "notMove";
                    if (!focus.activeSelf) focus.SetActive(true);
                    if (focusSrp.color != Color.red) focusSrp.color = Color.red;
                }
                if (!GsManager.I.IsCursor(cName)) GsManager.I.SetCursor(cName);
                //공격 커서 상황일때도 가이드라인이 생성되어야할지는 추후 고민
            }

            if (isSk)
            {
                if (attRng[0].gameObject.activeSelf) SelRngGrid(t);
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (isSk)
                {
                    if (!IsUsingSk())
                        return;
                    else
                    {
                        objTurn[0].state = BtObjState.SKILL;
                        objTurn[0].tgId = gGrid[t.x, t.y].tId;
                        objTurn[0].skId = curUseSkId;
                        TurnAction();
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
                            OnMovePlayer(t, 1);
                        break;
                    case "default":
                        if (!focus.activeSelf) return;
                        OnMovePlayer(t);
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
        }
        // MoveCamera(false);
        if (isMove)
            MoveCamera(false);
    }
    #region ==== 🎨 LOAD BATTLE SCENE ====
    void LoadFieldMap()
    {
        if (gGrid != null)
            gGrid = null;
        //mapSeed -> 1~100 필드 101부터 던전 및 특수 맵
        mapSeed = 101; //던전 맵 시드
        tileMapObj = GameObject.Find(mapDefault);
        if (tileMapObj == null)
            tileMapObj = Instantiate(ResManager.GetGameObject(mapDefault), transform);
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
                    // TileBase gTile = gMap.GetTile(tilePos), pTile = pMap.GetTile(tilePos);
                    var pTile = pMap.GetTile(tilePos);
                    gGrid[x, y] = new BtGrid() { x = tilePos.x * tileItv + tileOffset, y = tilePos.y * tileItv + tileOffset, tId = 0 };
                    if (pTile != null)
                    {
                        string[] data = pTile.name.Split('_');
                        gGrid[x, y].tId = int.Parse(data[3]);
                        switch (data[2])
                        {
                            case "2":
                                string str = $"{pTile.name.Remove(pTile.name.Length - 2)}_{Random.Range(1, 9)}";
                                CreatePropObj("PropObj", $"{str}_1", mapH - y, propObj, gGrid[x, y].x, gGrid[x, y].y, propParent);
                                CreatePropObj("Prop2Obj", $"{str}_2", mapH - y, prop2Obj, gGrid[x, y].x, gGrid[x, y].y + 0.6f, prop2Parent);
                                break;
                            default:
                                CreatePropObj("PropObj", pTile.name, mapH - y, propObj, gGrid[x, y].x, gGrid[x, y].y, propParent);
                                break;
                        }
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
    private void CreateRngObj(int cnt)
    {
        int idx = attRng.Count;
        for (int i = 0; i < cnt; i++)
        {
            var obj = Instantiate(ResManager.GetGameObject("RngGrid"), rngParent.transform);
            var rng = obj.GetComponent<RngGrid>();
            obj.name = "Rng_" + (idx + i);
            obj.SetActive(false);
            attRng.Add(rng);
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
    #region ==== Field Action ====
    public void ShowAttRng(Vector2Int grid, int w, int h, int cnt)
    {
        foreach (var rng in attRng)
        {
            rng.gameObject.SetActive(false);
            rng.SetColor(Color.white);
        }

        int attMinX = grid.x, attMaxX = grid.x + w - 1, attMinY = grid.y, attMaxY = grid.y + h - 1;
        List<Vector2Int> rngPos = new List<Vector2Int>();
        for (int x = grid.x - cnt; x < grid.x + w + cnt; x++)
        {
            for (int y = grid.y - cnt; y < grid.y + h + cnt; y++)
            {
                if (x < 0 || x >= mapW || y < 0 || y >= mapH) continue;
                if (x >= grid.x && x < grid.x + w && y >= grid.y && y < grid.y + h) continue;

                // tId가 1~999 사이면 제외
                if (gGrid[x, y].tId > 0 && gGrid[x, y].tId < 1000) continue;

                int distX = x < attMinX ? attMinX - x : (x > attMaxX ? x - attMaxX : 0);
                int distY = y < attMinY ? attMinY - y : (y > attMaxY ? y - attMaxY : 0);
                if (Mathf.Max(distX, distY) <= cnt) rngPos.Add(new Vector2Int(x, y));
            }
        }
        if (rngPos.Count > attRng.Count) CreateRngObj(rngPos.Count - attRng.Count);
        for (int i = 0; i < rngPos.Count; i++)
        {
            Vector2Int p = rngPos[i];
            attRng[i].SetPos(gGrid[p.x, p.y].x, gGrid[p.x, p.y].y, p.x, p.y);
            attRng[i].gameObject.SetActive(true);
        }
        attRngPos = grid;
    }
    private void HideAllRng()
    {
        foreach (var rng in attRng)
            rng.gameObject.SetActive(false);
        attRngPos = new Vector2Int(-200, -200);
    }
    private void SelRngGrid(Vector2Int grid)
    {
        foreach (var rng in attRng)
            rng.SetColor(Color.white);
        if (grid.x == -1 && grid.y == -1)
        {
            skPos = new Vector2Int(-1, -1);
            return;
        }
        foreach (var rng in attRng)
        {
            if (rng.xx == grid.x && rng.yy == grid.y)
            {
                switch (pSkType)
                {
                    case 1:
                        if (gGrid[grid.x, grid.y].tId >= 1000)
                            rng.SetColor(Color.red);
                        else
                            rng.SetColor(Color.green);
                        break;
                    case 2:
                    case 3:
                        if (gGrid[grid.x, grid.y].tId < 1000)
                            rng.SetColor(Color.red);
                        else
                            rng.SetColor(Color.green);
                        break;
                }
                isSkAvailable = rng.IsSkAvailable();
                skPos = grid;
                return;
            }
        }
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
    private void changeAlphaWithProps(int idx, float val)
    {
        propObj[idx].SetAlpha(val);
        prop2Obj[idx].SetAlpha(val);
    }
    #endregion
    #region ==== Object Action ====
    void OnMovePlayer(Vector2Int t, int state = 0)
    {
        focus.SetActive(false);
        isActionable = false;
        isMove = true;
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
                        if (ot.mIdx >= ot.mPath.Length || GetNearbyEnemy(ot) || gGrid[ot.mPath[ot.mIdx].x, ot.mPath[ot.mIdx].y].tId != 0)
                        {
                            //ot.mIdx가 0이면 처음 이동하는거라 스킵시킨다.
                            if (ot.mIdx > 0)
                            {
                                ot.state = BtObjState.IDLE;
                                isMove = false;
                                isActionable = true;
                                return;
                            }
                        }
                        ot.isAction = true;
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
                            ot.isAction = false;
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
                        BattleSkManager.I.ActSkill(ot.skId, skPos);
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
    private float GetBodyObj(int objId)
    {
        if (objId == 1000)
            return player.GetObjDir();
        else
            return mData[objId].GetObjDir();
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
    private void SetObjDir(int objId, float cx, float lx)
    {
        //cx : 현재 위치의 x좌표, lx : 바라보는 위치의 x좌표
        float dir = cx == lx ? GetBodyObj(objId) : (cx > lx ? 1f : -1f);
        if (objId == 1000)
            player.SetObjDir(dir);
        else
            mData[objId].SetObjDir(dir);
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
    IEnumerator MoveObj(GameObject obj, int objId, Vector2Int cv, Vector2Int mv, float ct, Action callA = null, Action callB = null)
    {
        SetObjDir(objId, cv, mv);
        var pos = new Vector3(gGrid[mv.x, mv.y].x, gGrid[mv.x, mv.y].y, 0);
        obj.transform.DOJump(pos, jumpPower: 0.3f, numJumps: 1, duration: 0.3f).SetEase(Ease.OutQuad); //통통 튀며 이동
        callA?.Invoke();
        yield return new WaitForSeconds(ct);
        callB?.Invoke();
        TurnAction();
    }

    private void TrackMon(TurnData data, int mId, float ct)
    {
        data.isAction = true; //행동 시작
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
                    data.isAction = false; //행동 종료
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
                data.isAction = false;
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

        Sequence sequence = DOTween.Sequence();
        sequence.Append(myObj.transform.DOMove(midPoint, 0.1f)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                CompAttAct(myId, myType, tgId, tgType, myPos, tgPos, attId);
            }));
        sequence.Append(myObj.transform.DOMove(myPos, 0.05f)
            .SetEase(Ease.InQuad));
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
        SetObjDir(myId, myPos.x, tgPos.x); //현재 오브젝트가 타겟을 바라보도록
        SetObjDir(tgId, tgPos.x, myPos.x); //타겟 오브젝트가 현재 오브젝트를 바라보도록
        int att = 0, crt = 0, crtRate = 0, dmg = 0;
        switch (myType)
        {
            case BtObjType.PLAYER: //플레이어가 타겟에게 행동
                att = player.pData.Att;
                crt = player.pData.Crt;
                crtRate = player.pData.CrtRate;
                switch (tgType)
                {
                    case BtObjType.MONSTER:
                        switch (attId)
                        {
                            case 1003:
                                float val = (float)BattleSkManager.GetSkAttVal(player.pData.SkList[1003], 601) * 0.01f;
                                att = (int)(att * val);

                                List<int> dmgList = new List<int>();
                                for (int i = 0; i < 2; i++)
                                    dmgList.Add(GsManager.I.GetDamage(att, mData[tgId].def));
                                mData[tgId].OnDamaged(dmgList[0] + dmgList[1], BtFaction.ALLY, myPos);
                                ShowEff("N_DoubleAtt", tgPos, player.bodyObj.transform.localScale.x, () => { TurnAction(); });
                                StartCoroutine(ShowSqcDmgTxt(2, dmgList, 0.3f, tgPos));
                                break;
                            default:
                                dmg = GsManager.I.GetDamage(att, mData[tgId].def);
                                mData[tgId].OnDamaged(dmg, BtFaction.ALLY, myPos);
                                StartCoroutine(UseObjTurn(0.3f));
                                ShowDmgTxt(dmg, tgPos);
                                break;
                        }
                        break;
                    case BtObjType.NPC:
                        break;
                }
                break;
            case BtObjType.MONSTER: //몬스터가 타겟에게 행동
                att = mData[myId].att;
                crt = mData[myId].crt;
                crtRate = mData[myId].crtRate;
                switch (tgType)
                {
                    case BtObjType.PLAYER:
                        dmg = GsManager.I.GetDamage(att, player.pData.Def);
                        player.OnDamaged(dmg, myPos);
                        // ShowEff("N_Att", tgPos, mData[myId].bodyObj.transform.localScale.x);
                        StartCoroutine(UseObjTurn(0.3f));
                        ShowDmgTxt(dmg, tgPos);
                        break;
                    case BtObjType.MONSTER:
                        break;
                    case BtObjType.NPC:
                        break;
                }
                break;
            case BtObjType.NPC: //NPC가 타겟에게 행동
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
                    changeAlphaWithProps(i, 0.5f);
                }
            }
            else
            {
                if (prop2Obj[i].onObj)
                {
                    prop2Obj[i].onObj = false;
                    if (prop2Obj[i].curAlpha != 1f)
                        changeAlphaWithProps(i, 1f);
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
        skPos = new Vector2Int(-1, -1);
        isSk = false; curUseSkId = 0;
        HideAllRng();
    }
    public bool IsUsingSk() => BattleSkManager.I.IsUsingSk();
    // BattleSkManager 전용 (최소 API)
    public void GetSkillState(out bool isSk, out int curSkId, out Vector2Int skPos, out bool available)
    { isSk = BattleCore.isSk; curSkId = curUseSkId; skPos = this.skPos; available = isSkAvailable; }
    public void BeginSkill(int skId, int skType)
    { focus.SetActive(false); isSk = true; curUseSkId = skId; pSkType = skType; }
    public Vector2Int GetPlayerTilePos() => FindTilePos(player.transform.position);
    public void DashToTile(Vector2Int pos)
    {
        Vector3 wPos = pObj.transform.position;
        Vector3 tgPos = new Vector3(gGrid[pos.x, pos.y].x, gGrid[pos.x, pos.y].y, 0);
        float dur = Mathf.Clamp(Vector3.Distance(wPos, tgPos) * 0.1f, 0.1f, 1f);
        SetObjDir(1000, cpPos, pos);
        pObj.transform.DOMove(tgPos, dur).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            UpdateGrid(cpPos.x, cpPos.y, pos.x, pos.y, 1, 1, 1000);
            cpPos = pos;
            objTurn[0].pos = pos;
            player.SetObjLayer(mapH - pos.y);
            TurnAction();
        });
    }
    public void ActMeleeToTile(Vector2Int pos, int skId)
    {
        ActObjWithMeleeAtt(player.bodyObj, BtObjType.PLAYER, 1000, gGrid[pos.x, pos.y].tId, skId);
    }
    #endregion
    #region ==== 애니메이션 ====
    public void ShowEff(string effName, Vector3 pos, float dir, Action call = null)
    {
        var eff = GetEffIdx(effName);
        if (eff != null)
        {
            StartBtEff(eff, pos, dir, call);
        }
        else
        {
            var obj = Instantiate(ResManager.GetGameObject(effName), effParent.transform);
            eff = obj.GetComponent<SkEffObj>();
            if (!effList.ContainsKey(effName))
                effList[effName] = new List<SkEffObj>();
            effList[effName].Add(eff);
            StartBtEff(eff, pos, dir, call);
        }
    }
    private void StartBtEff(SkEffObj eff, Vector3 pos, float dir, Action call = null)
    {
        eff.transform.position = pos;
        eff.transform.localScale = new Vector3(dir, 1, 1);
        eff.anim.EndEvent.RemoveAllListeners();
        eff.anim.EndEvent.AddListener(() =>
        {
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
        HideAllRng();
    }
    public void ShowDmgTxt(int dmg, Vector3 pos)
    {
        var txt = GetDmgTxt();
        if (txt == null)
        {
            txt = Instantiate(ResManager.GetGameObject("DmgTxt"), dmgTxtParent.transform).GetComponent<TextMeshProUGUI>();
            dmgTxtList.Add(txt);
        }
        else
            txt.gameObject.SetActive(true);
        txt.transform.position = pos;
        txt.text = dmg.ToString();

        DOTween.Sequence().SetAutoKill(true).Append(txt.transform.DOMoveY(txt.transform.position.y + 0.6f, 0.5f).SetEase(Ease.OutQuad))
        .Join(txt.DOFade(0f, 1f))
        .Join(txt.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack))
        .Append(txt.transform.DOScale(1f, 0.2f))
        .OnComplete(() =>
        {
            txt.color = new Color(1f, 1f, 1f, 1f);
            txt.gameObject.SetActive(false);
        });
    }
    IEnumerator ShowSqcDmgTxt(int cnt, List<int> dmgList, float ct, Vector3 pos)
    {
        for (int i = 0; i < cnt; i++)
        {
            ShowDmgTxt(dmgList[i], pos);
            yield return new WaitForSeconds(ct);
        }
    }
    TextMeshProUGUI GetDmgTxt()
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
        player.StateOutline();
    }
}
[CustomEditor(typeof(BattleCore))]
public class BattleCoreEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BattleCore myScript = (BattleCore)target;
        if (GUILayout.Button("테스트"))
        {
            myScript.TestPlayer();
        }
        if (GUILayout.Button("테스트 오버레이 프로퍼"))
        {
            myScript.TestCheckOverlapProp();
        }
        // if (GUILayout.Button("공격 애니메이션 테스트"))
        // {
        //     myScript.ShowTestAnimation("N_Att");
        // }
    }
}