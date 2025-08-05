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
    public int mapSeed; // 맵 시드
    [SerializeField] private GameObject tileMapObj; // 맵 타일 오브젝트
    public Image blackImg; // 전투 씬 시작시 페이드인용 암막 이미지
    public int[,] gGrid; // 땅타일 그리드
    public int[,] pGrid; // 환경, 물건 타일 그리드 -> 해당 그리드로 길찾기
    public int[] mapLimit = new int[4]; // 0 : 상, 1 : 하, 2 : 좌, 3 : 우 맵 타일 제한
    void Awake()
    {
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
        {
            gGrid = null;
            pGrid = null;
        }
        if (mapSeed != 0)
        {
            //추후에 맵 시드에 맞춰서 맵 생성
        }
        else
        {
            if (tileMapObj == null)
                tileMapObj = GameObject.FindGameObjectWithTag("tileMapObj");
        }

        Tilemap gMap = tileMapObj.transform.Find("Ground")?.GetComponent<Tilemap>();
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
            int w = maxX - minX + 1, h = maxY - minY + 1;
            gGrid = new int[h, w];
            pGrid = new int[h, w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector3Int tilePos = new Vector3Int(minX + x, minY + y, 0);
                    TileBase gTile = gMap.GetTile(tilePos), pTile = pMap.GetTile(tilePos);
                    gGrid[y, x] = int.Parse(gTile.name.Split('_')[2]);
                    pGrid[y, x] = pTile == null ? 0 : int.Parse(pTile.name.Split('_')[2]);
                }
            }
        }
    }
    void LoadPlayer()
    {

    }
    void LoadMonster()
    {

    }
    void LoadNPC()
    {

    }

    void FadeIn()
    {
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
