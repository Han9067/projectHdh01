using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class BattleCore : AutoSingleton<BattleCore>
{
    public class tileGrid
    {
        public float x, y;
        public int type;
    }
    public int mapSeed, pDir; // [맵 시드], [플레이어 방향 상,하,좌,우]
    [SerializeField] private GameObject tileMapObj; // 맵 타일 오브젝트
    public GameObject player, focus; // 플레이어, 포커스 오브젝트
    public GameObject propParent, propPrefab; // 환경, 물건 프리팹 부모, 프리팹
    public tileGrid[,] gGrid; // 땅타일 그리드
    public int[] mapLimit = new int[4]; // 0 : 상, 1 : 하, 2 : 좌, 3 : 우 맵 타일 제한
    private Tilemap gMap; // 땅 타일 맵
    private int mapW, mapH, cpX, cpY;
    private float tileOffset = 0.6f, tileItv = 1.2f; // 타일 오프셋, 타일 간격
    void Awake()
    {
        // pDir = 3;//임시
        //맵 타일 로드
        LoadFieldMap(); // 맵 타일 로드
        LoadPlayer(); // 플레이어 배치
        LoadMonster(); // 몬스터 배치
        LoadNPC(); // NPC 배치
        //ps. 여기에서는 아니지만 나중에 맵이 변경 또는 이동되는 특수 지형 및 던전도 대응해야함....ㅠㅠ
    }
    void Start()
    {
        // FadeIn(); // 페이드인 효과
    }
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
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        player.transform.position = new Vector3(-0.6f, -0.6f, 0);
        int[] arr = GetPlayerPos();
        cpX = arr[0]; cpY = arr[1];
        Debug.Log("cpX: " + cpX + ", cpY: " + cpY);
    }
    void LoadMonster()
    {

    }
    void LoadNPC()
    {

    }
    int[] GetPlayerPos()
    {
        float px = player.transform.position.x, py = player.transform.position.y;
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
    Vector3 FindNearestTilePosition(Vector3 worldPos)
    {
        float minDistance = float.MaxValue;
        Vector3 nearestPos = Vector3.zero;

        for (int x = 0; x < mapW; x++)
        {
            for (int y = 0; y < mapH; y++)
            {
                Vector3 tilePos = new Vector3(gGrid[x, y].x, gGrid[x, y].y, 0);
                float distance = Vector3.Distance(worldPos, tilePos);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPos = tilePos;
                }
            }
        }

        return nearestPos;
    }
    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;

        // 가장 가까운 타일 좌표 찾기
        Vector3 nearestTilePos = FindNearestTilePosition(mouseWorldPos);

        // focus를 가장 가까운 타일 좌표로 이동
        focus.transform.position = nearestTilePos;
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
}
