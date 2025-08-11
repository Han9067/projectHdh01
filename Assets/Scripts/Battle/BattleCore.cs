using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class tileGrid
{
    public float x, y;
    public int type;
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
    public Transform monsterParent;


    // float pathUpdateTimer = 0;
    void Awake()
    {
        CheckManager();
        // pDir = 3;//임시
        //맵 타일 로드
        LoadFieldMap(); // 맵 타일 로드
        LoadPlayer(); // 플레이어 배치
        // LoadMonster(); // 몬스터 배치
        //ps. 여기에서는 아니지만 나중에 맵이 변경 또는 이동되는 특수 지형 및 던전도 대응해야함....ㅠㅠ
    }
    void Start()
    {
        if (cmr == null) cmr = Camera.main;
        cmr.transform.position = new Vector3(pObj.transform.position.x, pObj.transform.position.y, -10f);
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
            switch (gGrid[t.x, t.y].type)
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
                Vector2Int[] path = BattlePathManager.I.GetPath(cpX, cpY, t.x, t.y, gGrid);
                if (path.Length > 0)
                {
                    StartCoroutine(MovePlayer(path));
                }
            }
        }
        if (isMove)
            MoveCamera();
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
                    TileBase gTile = gMap.GetTile(tilePos), pTile = pMap.GetTile(tilePos);
                    gGrid[x, y] = new tileGrid() { x = tilePos.x * tileItv + tileOffset, y = tilePos.y * tileItv + tileOffset, type = 0 };
                    //gGrid[x, y] = new tileGrid() { x = tilePos.x * tileItv + tileOffset, y = tilePos.y * tileItv + tileOffset, type = int.Parse(gTile.name.Split('_')[2]) };
                    if (pTile != null)
                    {
                        gGrid[x, y].type = int.Parse(pTile.name.Split('_')[2]);
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
    void LoadPlayer()
    {
        if (pObj == null)
            pObj = GameObject.FindGameObjectWithTag("Player");
        pObj.transform.position = new Vector3(-0.6f, -0.6f, 0);
        int[] arr = GetPlayerPos();
        cpX = arr[0]; cpY = arr[1];
        pObj.GetComponent<bPlayer>().SetObjLayer(arr[1]);
    }
    void LoadMonster()
    {
        MonManager.I.TestCreateMon();

        if (MonManager.I.BattleMonList.Count > 0)
        {
            foreach (int monId in MonManager.I.BattleMonList)
            {
                GameObject mon = Instantiate(monPrefab, monsterParent);
                mon.GetComponent<bMonster>().monsterId = monId;
                // mon.transform.position = new Vector3(gGrid[x, y].x, gGrid[x, y].y, 0);
                mObj.Add(mon);
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

            cpX = t.x; cpY = t.y; // 플레이어 위치 업데이트

            //추후에 몬스터 & NPC 이동 또는 행동 추가 예정
        }
        isActionable = true;
        isMove = false;
    }
    void MoveCamera()
    {
        Vector3 targetPosition = new Vector3(pObj.transform.position.x, pObj.transform.position.y, -10f);
        cmr.transform.position = Vector3.SmoothDamp(cmr.transform.position, targetPosition, ref velocity, 0.1f);
    }

    #region ==== 🎨 ORDERING IN LAYER ====
    void SetObjLayer()
    {

    }
    #endregion


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
