using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;
using UnityEngine.Tilemaps;
using System.Linq;

public class WorldAreaData
{
    public int areaID;
    public int areaType;
    public int curCnt;
    public int maxCnt;
    public Dictionary<int, List<int>> grpByGrade = new Dictionary<int, List<int>>();
}
public class WorldMonData
{
    public int areaID;
    public int mGrpID;
    public List<int> monList = new List<int>();
    public Vector3 pos;
}
public class WorldObjManager : AutoSingleton<WorldObjManager>
{
    #region 월드맵 영역 관련
    private bool isCreate = false;
    public Dictionary<int, List<Vector3>> wAreaPos = new Dictionary<int, List<Vector3>>(); //영역 내 타일들의 월드 좌표
    public void CreateWorldArea(Tilemap tilemap)
    {
        if (isCreate) return;
        isCreate = true;
        wAreaPos.Clear();

        // 타일맵의 모든 타일을 검색
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tPos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(tPos);

                // tile이 null이 아닌지 확인 추가
                if (tile != null && tile.name != null && tile.name.Length > 5)
                {
                    string name = tile.name;
                    int num = int.Parse(name.Substring(5));
                    int id = 0;
                    switch (name.Substring(0, 5))
                    {
                        case "wt_n_": id = num; break;
                        case "wt_f_": id = 100 + num; break;
                    }
                    if (!wAreaPos.ContainsKey(id))
                        wAreaPos[id] = new List<Vector3>();
                    // 타일맵 좌표를 게임 좌표로 변환
                    Vector3 worldPos = tilemap.CellToWorld(tPos) + tilemap.cellSize * 0.5f;
                    wAreaPos[id].Add(worldPos);
                }
            }
        }

        CreateWorldAreaData(); //구역별 몬스터 스폰 데이터 생성
    }
    #endregion
    #region 월드맵 몬스터 스폰 관련
    private SpawnMonTable _spawnMonTable;
    private MonGrpTable _monGrpTable;
    public SpawnMonTable SpawnMonTable => _spawnMonTable ?? (_spawnMonTable = GameDataManager.GetTable<SpawnMonTable>());
    public MonGrpTable MonGrpTable => _monGrpTable ?? (_monGrpTable = GameDataManager.GetTable<MonGrpTable>());
    public Dictionary<int, WorldAreaData> areaDataList = new Dictionary<int, WorldAreaData>(); //월드맵 구역 데이터
    public Dictionary<int, WorldMonData> worldMonDataList = new Dictionary<int, WorldMonData>();
    public void CreateWorldAreaData()
    {
        foreach (var spawnMon in SpawnMonTable.Datas)
        {
            int id = spawnMon.AreaID;
            areaDataList[id] = new WorldAreaData
            {
                areaID = id,
                areaType = id / 100,
                curCnt = 0,
                maxCnt = spawnMon.Cnt
            };
            areaDataList[id].grpByGrade.Add(10, spawnMon.MG10.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(9, spawnMon.MG9.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(8, spawnMon.MG8.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(7, spawnMon.MG7.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(6, spawnMon.MG6.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(5, spawnMon.MG5.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(4, spawnMon.MG4.Split(',').Select(int.Parse).ToList());
        }
        //추후에 curCnt는 세이브 데이터를 통해 갱신해야 함
        //curCnt는 maxCnt만큼만 적용되는데 적용되는 시점은 스폰타임(현재 1주일간격)에 1번 적용
    }
    public void AddWorldMonData(int uId, int areaID, int mGrpID, List<int> monList, Vector3 pos)
    {
        worldMonDataList[uId] = new WorldMonData
        {
            areaID = areaID,
            mGrpID = mGrpID,
            monList = monList,
            pos = pos
        };
    }
    public Vector3 GetSpawnPos(int areaID, float minDistance = 2.0f, int maxAttempts = 20)
    {
        if (!wAreaPos.ContainsKey(areaID)) return Vector3.zero;

        float sqrMinDistance = minDistance * minDistance;  // 제곱근 계산 제거
        List<Vector3> allPositions = wAreaPos[areaID];

        // 최대 시도 횟수만큼 랜덤 위치 테스트
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector3 randomPos = allPositions[Random.Range(0, allPositions.Count)];
            bool isValid = true;
            // 해당 구역의 몬스터들만 체크
            foreach (var monData in worldMonDataList.Values)
            {
                if (monData.areaID == areaID && Vector3.SqrMagnitude(randomPos - monData.pos) < sqrMinDistance)
                {
                    isValid = false;
                    break;
                }
            }
            if (isValid) return randomPos;
        }

        // 모든 시도가 실패하면 첫 번째 위치 반환
        return allPositions[0];
    }

    #endregion
}
