using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class BattleCore : AutoSingleton<BattleCore>
{
    public Image blackImg;
    public GameObject tileParent;
    public GameObject tilePrefab;
    void Awake()
    {
        // if (!blackImg.gameObject.activeSelf)
        //     blackImg.gameObject.SetActive(true);
        // blackImg.color = new Color(0, 0, 0, 1f);
        // blackImg.DOFade(0f, 1f).OnComplete(() =>
        // {
        //     Debug.Log("BattleCore Start");
        //     blackImg.gameObject.SetActive(false);
        // });
        //그리드 생성
        CreateGrid();
    }
    void Start()
    {
    }
    void CreateGrid()
    {
        // 타일 크기 설정
        float tileSize = 120f / 100f;

        // 그리드 크기 설정
        int gridWidth = 40;
        int gridHeight = 30;

        // 전체 그리드 크기 계산
        float totalWidth = gridWidth * tileSize;
        float totalHeight = gridHeight * tileSize;

        // 시작 위치 계산
        Vector3 startPos = new Vector3(-totalWidth / 2f + tileSize / 2f, totalHeight / 2f - tileSize / 2f, 0f);

        // 타일 생성
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3 tilePosition = startPos + new Vector3(x * tileSize, -y * tileSize, 0f);
                GameObject tile = Instantiate(tilePrefab, tilePosition, Quaternion.identity, tileParent.transform);
                tile.name = $"Tile_{x}_{y}";
                tile.transform.localScale = new Vector3(tileSize, tileSize, 1f);
            }
        }
    }
}
