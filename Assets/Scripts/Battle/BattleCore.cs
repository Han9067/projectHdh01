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

public class tileGrid
{
    public float x, y;
    public int tId;
}
public class BattleCore : AutoSingleton<BattleCore>
{
    private static readonly Vector2Int[] DIRECTIONS = {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left,
        new Vector2Int(1, 1),   new Vector2Int(1, -1),  new Vector2Int(-1, 1),  new Vector2Int(-1, -1)
    };
    [Header("====Camera====")]
    public Camera cmr; // 전투씬 메인 카메라
    private Vector3 velocity = Vector3.zero; //카메라 속도
    [Header("====Map====")]
    [SerializeField] private GameObject tileMapObj; // 맵 타일 오브젝트
    public int mapSeed, pDir; // [맵 시드], [플레이어 방향 상,하,좌,우]
    public tileGrid[,] gGrid; // 땅타일 그리드
    // ========================================
    // 🎮 gGrid 내부 tId 관련 내용 => 0~99 -> 타일종류, 3자리 숫자 -> 환경 오브젝트, 4자리 숫자 -> 플레이어(1000 고정), NPC, 몬스터
    // ========================================
    private Tilemap gMap; // 땅 타일 맵
    private int mapW, mapH, cpX, cpY; // 맵 너비, 맵 높이, 플레이어 x,y좌표
    private float tileOffset = 0.6f, tileItv = 1.2f; // 타일 오프셋, 타일 간격
    float[] mapLimit = new float[4]; // 0 : 상, 1 : 하, 2 : 좌, 3 : 우 맵 타일 제한
    public GameObject[,] guide; // 길찾기 가이드 오브젝트

    [Header("====Player====")]
    // [SerializeField] private Sprite focusSprite; // 포커스 스프라이트
    public GameObject pObj, focus, propParent, propPrefab; // 플레이어, 포커스, 환경, 물건 프리팹 부모, 프리팹
    private SpriteRenderer focusSrp;
    bPlayer pData;
    private bool isActionable = true, isMove = false; // 플레이어 행동 가능 여부, 플레이어 이동 중인지 여부

    [Header("====Monster====")]
    public GameObject monPrefab;
    List<GameObject> mObj = new List<GameObject>();
    List<bMonster> mData = new List<bMonster>();
    public Transform monsterParent;

    [Header("====Common====")]
    public int objId;
    // private List<int> 
    // float dTime = 0;

    void Awake()
    {
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
    }
    void Update()
    {
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
                if (EventSystem.current.IsPointerOverGameObject()) return;
                switch (cName)
                {
                    case "attack":
                        if (GetAttackTarget(gGrid[t.x, t.y].tId))
                        {
                            AttackPlayer(GetTargetMonster(t.x, t.y));
                        }
                        else
                            StartCoroutine(AutoMovePlayer(GetTargetMonster(t.x, t.y)));
                        break;
                    case "default":
                        if (!focus.activeSelf) return;
                        Vector2Int[] allPath = BattlePathManager.I.GetPath(cpX, cpY, t.x, t.y, gGrid);
                        if (allPath.Length > 0)
                            StartCoroutine(MovePlayer(allPath, () => { isActionable = false; isMove = true; },
                            () => { isActionable = true; isMove = false; MoveCamera(false); }));
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
        Tilemap pMap = tileMapObj.transform.Find("Prop")?.GetComponent<Tilemap>();

        if (gMap != null)
        {
            BoundsInt bounds = gMap.cellBounds;
            // 실제 타일이 배치된 최소/최대 좌표 찾기
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if (gMap.HasTile(pos))
                    {
                        minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                        minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                    }
                }
            }
            // 실제 크기 계산
            mapW = maxX - minX + 1; mapH = maxY - minY + 1;
            gGrid = new tileGrid[mapW, mapH];
            for (int x = 0; x < mapW; x++)
            {
                for (int y = 0; y < mapH; y++)
                {
                    Vector3Int tilePos = new Vector3Int(minX + x, minY + y, 0);
                    // TileBase gTile = gMap.GetTile(tilePos), pTile = pMap.GetTile(tilePos);
                    TileBase pTile = pMap.GetTile(tilePos);
                    gGrid[x, y] = new tileGrid() { x = tilePos.x * tileItv + tileOffset, y = tilePos.y * tileItv + tileOffset, tId = 0 };
                    if (pTile != null)
                    {
                        gGrid[x, y].tId = int.Parse(pTile.name.Split('_')[2]);
                        GameObject prop = Instantiate(propPrefab, propParent.transform);
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
            cpX = cx; cpY = cy;
            gGrid[cx, cy].tId = 1000;
            pData.SetObjLayer(cy);
        }
    }
    void LoadEnemyGrp()
    {
        MonManager.I.TestCreateMon(); //테스트용

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
            List<Vector2Int> mPos = new List<Vector2Int>();
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
            foreach (Vector2Int p in mPos)
            {
                GameObject mon = Instantiate(monPrefab, monsterParent);
                bMonster data = mon.GetComponent<bMonster>();
                data.SetDirObj(pDir == 0 ? 1 : -1);
                data.SetMonData(++objId, MonManager.I.BattleMonList[0], p.x, p.y, gGrid[p.x, p.y].x, gGrid[p.x, p.y].y);
                mon.name = "Mon_" + objId;
                mObj.Add(mon);
                mData.Add(data);
                gGrid[p.x, p.y].tId = objId;
                //나중에 몬스터가 2x2 또는 3x3 타일 형태로 생성되는데 그때는 왼쪽 상단을 기준으로 좌표가 갱신되도록 함
            }
        }
    }
    #endregion
    Vector2Int FindTilePos(Vector3 worldPos)
    {
        float minDistance = float.MaxValue;
        Vector2Int result = new Vector2Int(0, 0);

        if (worldPos.x < mapLimit[0] || worldPos.x > mapLimit[1] || worldPos.y < mapLimit[2] || worldPos.y > mapLimit[3])
            return new Vector2Int(-1, -1);

        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                Vector3 tilePos = new Vector3(gGrid[x, y].x, gGrid[x, y].y, 0);
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
    bool GetAttackTarget(int tId)
    {
        for (int i = 0; i < DIRECTIONS.Length; i++)
        {
            Vector2Int t = new Vector2Int(cpX, cpY) + DIRECTIONS[i];
            if (t.x < 0 || t.x >= mapW || t.y < 0 || t.y >= mapH)
                continue;
            if (gGrid[t.x, t.y].tId == tId)
                return true;
        }
        return false;
    }
    IEnumerator MovePlayer(Vector2Int[] path, Action callA = null, Action callB = null)
    {
        callA?.Invoke();
        for (int i = 0; i < path.Length; i++)
        {
            Vector2Int t = path[i]; //target pos
            SetMoveObj(pObj, new Vector2Int(cpX, cpY), t);
            yield return new WaitForSeconds(0.3f); // 이동 완료까지 대기
            pData.SetObjLayer(t.y);
            UpdateGrid(cpX, cpY, t.x, t.y, 1, 1, 1000);
            cpX = t.x; cpY = t.y; // 플레이어 위치 업데이트
            //추후에 몬스터 & NPC 이동 또는 행동 추가 예정
        }
        callB?.Invoke();
    }
    IEnumerator AutoMovePlayer(bMonster tg)
    {
        isActionable = false;
        isMove = true;
        while (true)
        {
            Vector2Int[] path = BattlePathManager.I.GetPath(cpX, cpY, tg.x, tg.y, gGrid);
            Vector2Int[] onePath = new Vector2Int[] { new Vector2Int(path[0].x, path[0].y) };
            StartCoroutine(MovePlayer(onePath)); //한 칸 이동
            yield return new WaitForSeconds(0.3f);

            if (GetAttackTarget(tg.objId) || tg == null)
                break;
        }
        isActionable = true;
        isMove = false;
        MoveCamera(false);
    }

    void AttackPlayer(bMonster tg)
    {
        Debug.Log("공격 : " + tg.objId);
        //추후에 공격력과 방어력을 계산해서 데미지가 입도록 하고 또한 더 나중에는 회피,치명타 관련도 대응
        tg.OnDamaged(PlayerManager.I.pData.Att);
    }

    void SetMoveObj(GameObject obj, Vector2Int pv, Vector2Int mv)
    {
        Vector3 pos = new Vector3(gGrid[mv.x, mv.y].x, gGrid[mv.x, mv.y].y, 0);
        float dir = pv.x == mv.x ? obj.transform.localScale.x : (pv.x > mv.x ? 1f : -1f); //캐릭터 방향 설정
        obj.transform.localScale = new Vector3(dir, 1, 1);
        obj.transform.DOMove(pos, 0.3f); //트윈으로 이동
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
    bMonster GetTargetMonster(int x, int y)
    {
        for (int i = 0; i < mData.Count; i++)
        {
            if (mData[i].x == x && mData[i].y == y)
                return mData[i];
        }
        return null;
    }
    #endregion
    void MoveCamera(bool isInit)
    {
        Vector3 targetPosition = new Vector3(pObj.transform.position.x, pObj.transform.position.y, -10f);
        if (isInit)
            cmr.transform.position = targetPosition;
        else
            cmr.transform.position = Vector3.SmoothDamp(cmr.transform.position, targetPosition, ref velocity, 0.1f);
    }
    void FadeIn()
    {
        // 전투 씬 시작시 페이드인용 암막 이미지
        Image blackImg = GameObject.FindGameObjectWithTag("blackImg").GetComponent<Image>();
        if (!blackImg.gameObject.activeSelf)
            blackImg.gameObject.SetActive(true);
        blackImg.color = new Color(0, 0, 0, 1f);
        blackImg.DOFade(0f, 1f).OnComplete(() =>
        {
            Debug.Log("BattleCore Start");
            blackImg.gameObject.SetActive(false);
        });
    }

    void CheckMainManager()
    {
        if (GameObject.Find("Manager") == null)
        {
            GameObject obj = new GameObject("Manager");
            obj.AddComponent<PlayerManager>();
            obj.AddComponent<MonManager>();
            obj.AddComponent<ItemManager>();
            obj.AddComponent<SaveFileManager>();
            obj.AddComponent<BattlePathManager>();
            obj.AddComponent<CursorManager>();
        }
    }
}