using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.Tilemaps;

public class WorldObjManager : AutoSingleton<WorldObjManager>
{
    #region 월드맵 영역 관련
    private bool isCreate = false;
    public Dictionary<int, List<Vector3Int>> nArea = new Dictionary<int, List<Vector3Int>>();
    public Dictionary<int, List<Vector3Int>> fArea = new Dictionary<int, List<Vector3Int>>();
    public void CreateWorldArea(Tilemap tilemap)
    {
        if (isCreate) return;
        isCreate = true;
        nArea.Clear();
        fArea.Clear();

        // 타일맵의 모든 타일을 검색
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(tilePosition);

                if (tile != null)
                {
                    string tileName = tile.name;

                    // 일반 구역 처리 (wt_n_숫자)
                    if (tileName.StartsWith("wt_n_"))
                    {
                        // 숫자 부분 추출
                        string numberPart = tileName.Substring(5); // "wt_n_" 제거
                        if (int.TryParse(numberPart, out int areaNumber))
                        {
                            if (!nArea.ContainsKey(areaNumber))
                            {
                                nArea[areaNumber] = new List<Vector3Int>();
                            }
                            nArea[areaNumber].Add(tilePosition);
                        }
                    }
                    // 숲 구역 처리 (wt_f_숫자)
                    else if (tileName.StartsWith("wt_f_"))
                    {
                        // 숫자 부분 추출
                        string numberPart = tileName.Substring(5); // "wt_f_" 제거
                        if (int.TryParse(numberPart, out int areaNumber))
                        {
                            if (!fArea.ContainsKey(areaNumber))
                            {
                                fArea[areaNumber] = new List<Vector3Int>();
                            }
                            fArea[areaNumber].Add(tilePosition);
                        }
                    }
                }
            }
        }
    }
    // 특정 구역의 타일 위치들을 가져오는 함수들
    public List<Vector3Int> GetNormalAreaTiles(int areaNumber)
    {
        return nArea.ContainsKey(areaNumber) ? nArea[areaNumber] : new List<Vector3Int>();
    }
    public List<Vector3Int> GetForestAreaTiles(int areaNumber)
    {
        return fArea.ContainsKey(areaNumber) ? fArea[areaNumber] : new List<Vector3Int>();
    }

    // 특정 구역의 타일 개수를 반환하는 함수들
    public int GetNormalAreaTileCount(int areaNumber)
    {
        return nArea.ContainsKey(areaNumber) ? nArea[areaNumber].Count : 0;
    }
    public int GetForestAreaTileCount(int areaNumber)
    {
        return fArea.ContainsKey(areaNumber) ? fArea[areaNumber].Count : 0;
    }
    #endregion
    #region 월드맵 몬스터 스폰 관련
    private SpawnMonTable _spawnMonTable;
    public SpawnMonTable SpawnMonTable => _spawnMonTable ?? (_spawnMonTable = GameDataManager.GetTable<SpawnMonTable>());
    // Dictionary<int, GameObject>  = new Dictionary<int, GameObject>();
    public void CheckWorldObj()
    {
        foreach (var spawnMon in SpawnMonTable.Datas)
        {

            // int areaNumber = spawnMon.AreaID;
            // int cnt = spawnMon.Cnt;
            // string monList = spawnMon.MonList;
        }
    }

    #endregion
}
