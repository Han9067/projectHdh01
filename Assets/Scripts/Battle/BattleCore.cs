using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;

public class tileGrid
{
    public float x, y;
    public int tId;
}
public class BattleCore : AutoSingleton<BattleCore>
{
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
    public int[] mapLimit = new int[4]; // 0 : 상, 1 : 하, 2 : 좌, 3 : 우 맵 타일 제한
    private Tilemap gMap; // 땅 타일 맵
    private int mapW, mapH, cpX, cpY; // 맵 너비, 맵 높이, 플레이어 x,y좌표
    private float tileOffset = 0.6f, tileItv = 1.2f; // 타일 오프셋, 타일 간격

    [Header("====Player====")]
    [SerializeField] private Sprite focusSprite; // 포커스 스프라이트
    public GameObject pObj, focus, propParent, propPrefab; // 플레이어, 포커스, 환경, 물건 프리팹 부모, 프리팹
    private bool isActionable = true, isMove = false; //플레이어 행동 가능 여부

    [Header("====Monster====")]
    public GameObject monPrefab;
    List<GameObject> mObj = new List<GameObject>();
    List<bMonster> mData = new List<bMonster>();
    public Transform monsterParent;

    [Header("====Common====")]
    public int objId;

    // float pathUpdateTimer = 0;
    void Awake()
    {
        CheckManager();
        //맵 타일 로드
        LoadFieldMap(); // 맵 타일 로드

        objId = 1000;
        LoadPlayerGrp(); // 플레이어 및 플레이어편의 NPC 생성 후 전원 배치
        LoadEnemyGrp(); // 몬스터, 적 NPC 생성 후 전원 배치


        //ps. 여기에서는 아니지만 나중에 맵이 변경 또는 이동되는 특수 지형 및 던전도 대응해야함....ㅠㅠ
    }
    void Start()
    {
        if (cmr == null) cmr = Camera.main;
        MoveCamera(true);
        focusSprite = focus.GetComponent<SpriteRenderer>().sprite;
        // FadeIn(); // 페이드인 효과
    }
    void Update()
    {
        if (isActionable)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector2Int t = FindTilePos(mouseWorldPos);
            focus.transform.position = new Vector3(gGrid[t.x, t.y].x, gGrid[t.x, t.y].y, 0);
            string fName = "";
            //
            switch (gGrid[t.x, t.y].tId)
            {
                case 0: fName = "focus"; break;
                default: fName = "notMove"; break;
            }
            if (focusSprite.name != fName)
            {
                focusSprite = ResManager.GetSprite(fName);
                focus.GetComponent<SpriteRenderer>().sprite = focusSprite;
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;
                Vector2Int[] path = BattlePathManager.I.GetPath(cpX, cpY, t.x, t.y, gGrid);
                if (path.Length > 0)
                    StartCoroutine(MovePlayer(path));
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
            mapLimit[0] = bounds.yMax; mapLimit[1] = bounds.yMin;
            mapLimit[2] = bounds.xMin; mapLimit[3] = bounds.xMax;
            Debug.Log(mapLimit[0] + " " + mapLimit[1] + " " + mapLimit[2] + " " + mapLimit[3]);
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
        }
        pMap.gameObject.SetActive(false);
    }
    void LoadPlayerGrp()
    {
        if (pObj == null)
            pObj = GameObject.FindGameObjectWithTag("Player");
        //추후 NPC 생성
        if (mapSeed < 1000) // 맵 시드가 1000 미만이면 일반 필드 1001부터는 특수 장소(던전이나 숲 등)
        {
            //추후엔 특정 이벤트(기습, 매복 등) 으로 배치 상황이 특수해질 경우도 대응해야함
            pDir = Random.Range(0, 4); // 0:상, 1:하, 2:좌, 3:우 -> 파티의 방향 설정
            int cx = 0, cy = 0;
            switch (pDir)
            {
                case 0: cx = 15; cy = 6; break;
                case 1: cx = 15; cy = 18; break;
                case 2: cx = 8; cy = 12; break;
                case 3: cx = 22; cy = 12; break;
            }
            pObj.transform.position = new Vector3(gGrid[cx, cy].x, gGrid[cx, cy].y, 0);
            cpX = cx; cpY = cy;
            gGrid[cx, cy].tId = 1000;
            pObj.GetComponent<bPlayer>().SetObjLayer(cy);
        }
    }
    void LoadEnemyGrp()
    {
        MonManager.I.TestCreateMon(); //테스트용

        if (MonManager.I.BattleMonList.Count > 0)
        {
            int cx = 0, cy = 0;
            //플레이어와 반대 방향에 배치
            switch (pDir)
            {
                case 0: cx = 15; cy = 18; break;
                case 1: cx = 15; cy = 6; break;
                case 2: cx = 22; cy = 12; break;
                case 3: cx = 8; cy = 12; break;
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
                data.SetMonData(++objId, MonManager.I.BattleMonList[0], p.x, p.y, gGrid[p.x, p.y].x, gGrid[p.x, p.y].y);
                mObj.Add(mon);
                mData.Add(data);
                gGrid[p.x, p.y].tId = objId;
                //나중에 몬스터가 2x2 또는 3x3 타일 형태로 생성되는데 그때는 왼쪽 상단을 기준으로 좌표가 갱신되도록 함
            }
        }
    }
    int[] GetPlayerPos()
    {
        float px = pObj.transform.position.x, py = pObj.transform.position.y;
        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                if (gGrid[x, y].x == px && gGrid[x, y].y == py)
                {
                    return new int[] { x, y };
                }
            }
        }
        return new int[] { 0, 0 };
    }
    #endregion
    Vector2Int FindTilePos(Vector3 worldPos)
    {
        float minDistance = float.MaxValue;
        Vector2Int result = new Vector2Int(0, 0);

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
    private IEnumerator MovePlayer(Vector2Int[] path)
    {
        isActionable = false;
        isMove = true;
        for (int i = 0; i < path.Length; i++)
        {
            Vector2Int t = path[i]; //target pos
            Vector3 pos = new Vector3(gGrid[t.x, t.y].x, gGrid[t.x, t.y].y, 0);
            float dir = cpX == t.x ? pObj.transform.localScale.x : (cpX > t.x ? 1f : -1f); //캐릭터 방향 설정
            pObj.transform.localScale = new Vector3(dir, 1, 1);
            pObj.transform.DOMove(pos, 0.3f); //트윈으로 이동

            yield return new WaitForSeconds(0.3f); // 이동 완료까지 대기
            pObj.GetComponent<bPlayer>().SetObjLayer(t.y);
            cpX = t.x; cpY = t.y; // 플레이어 위치 업데이트

            //추후에 몬스터 & NPC 이동 또는 행동 추가 예정
        }
        isActionable = true;
        isMove = false;
    }
    void MoveCamera(bool isInit)
    {
        //mapLimit
        //맵 끝자리를 체크하여 카메라 이동 범위 제한

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

    void CheckManager()
    {
        if (GameObject.Find("Manager") == null)
        {
            GameObject obj = new GameObject("Manager");
            obj.AddComponent<PlayerManager>();
            obj.AddComponent<MonManager>();
            obj.AddComponent<ItemManager>();
            obj.AddComponent<SaveFileManager>();
            obj.AddComponent<BattlePathManager>();
        }
    }
}

// pathUpdateTimer += Time.deltaTime;
// if (pathUpdateTimer >= 0.2f)
// {
//     pathUpdateTimer = 0;
//     Vector2Int[] path = BattlePathManager.I.GetPath(cpX, cpY, t.x, t.y, gGrid);
//     
// }
