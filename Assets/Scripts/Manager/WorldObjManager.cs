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
    public int ldID; //leaderID
    public List<int> monList = new List<int>();
    public Vector3 pos;
}
public class WorldObjManager : AutoSingleton<WorldObjManager>
{
    private static readonly Vector3Int[] v3Dir8 = new Vector3Int[]
    {
        new Vector3Int(0, 1, 0),    // 위
        new Vector3Int(0, -1, 0),   // 아래
        new Vector3Int(-1, 0, 0),   // 왼쪽
        new Vector3Int(1, 0, 0),    // 오른쪽
        new Vector3Int(-1, 1, 0),   // 왼쪽 위 대각선
        new Vector3Int(1, 1, 0),    // 오른쪽 위 대각선
        new Vector3Int(-1, -1, 0),  // 왼쪽 아래 대각선
        new Vector3Int(1, -1, 0)    // 오른쪽 아래 대각선
    };
    #region 월드맵 초기 생성 관련
    private bool isCreate = false;
    public Dictionary<int, List<Vector3>> wAreaPos = new Dictionary<int, List<Vector3>>(); //영역 내 타일들의 월드 좌표
    public void CreateWorldMap(Tilemap tilemap)
    {
        if (isCreate) return;
        isCreate = true;
        wAreaPos.Clear();
        Dictionary<int, Vector3> wRoadSpotPos = new Dictionary<int, Vector3>();
        // 타일맵의 모든 타일을 검색
        BoundsInt bounds = tilemap.cellBounds;
        #region 월드맵 구역 생성
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
                    string type = name.Substring(0, 5);
                    int num = int.Parse(name.Substring(5));
                    switch (type)
                    {
                        case "wt_r_":
                            if (num != 0)
                                wRoadSpotPos[num] = tilemap.CellToWorld(tPos) + tilemap.cellSize * 0.5f;
                            break;
                        default:
                            int id = type == "wt_n_" ? num : 100 + num;
                            if (!wAreaPos.ContainsKey(id))
                                wAreaPos[id] = new List<Vector3>();
                            // 타일맵 좌표를 게임 좌표로 변환
                            Vector3 worldPos = tilemap.CellToWorld(tPos) + tilemap.cellSize * 0.5f;
                            wAreaPos[id].Add(worldPos);
                            break;
                    }
                }
            }
        }
        int[,] root = { { 1, 2 }, { 1, 3 } };
        int cnt = root.GetLength(0);
        List<List<int>> path = new List<List<int>>();
        Debug.Log($"cnt: {cnt}");
        for (int i = 0; i < cnt; i++)
        {

        }

        // foreach (var spot in wRoadSpotPos)
        // {
        //     Debug.Log($"wRoadSpotPos: {spot.Key} -> {spot.Value}");
        // }
        #endregion
        CreateWorldAreaData(); //구역별 몬스터 스폰 데이터 생성
        #region 월드맵 도로 생성

        #endregion
        //CityDic
    }
    public static List<Vector3Int> FindRoadPath(Tilemap tilemap, Vector3Int sPos, Vector3Int ePos, string eName)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        queue.Enqueue(sPos);
        visited.Add(sPos);
        cameFrom[sPos] = sPos;
        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();
            if (current == ePos) break;
            foreach (Vector3Int dir in v3Dir8)
            {
                Vector3Int neighbor = current + dir;

                if (visited.Contains(neighbor))
                    continue;

                TileBase tile = tilemap.GetTile(neighbor);
                if (tile == null)
                    continue;

                string tileName = tile.name;

                if (tileName == "wt_r_0" || tileName == eName)
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }
        return null;
    }
    private static List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom,
                                                     Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int current = end;

        // 도착점에서 시작점까지 역순으로 추적
        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Add(start);

        // 시작점 -> 도착점 순서로 뒤집기
        path.Reverse();

        return path;
    }
    private static Vector3Int FindTilePosition(Tilemap tilemap, string tileName)
    {
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);

                if (tile != null && tile.name == tileName)
                {
                    return pos;
                }
            }
        }

        return Vector3Int.zero;
    }
    #endregion
    #region 월드맵 몬스터 스폰 관련
    private SpawnMonTable _spawnMonTable;
    public SpawnMonTable SpawnMonTable => _spawnMonTable ?? (_spawnMonTable = GameDataManager.GetTable<SpawnMonTable>());
    private MonGrpTable _monGrpTable;
    public MonGrpTable MonGrpTable => _monGrpTable ?? (_monGrpTable = GameDataManager.GetTable<MonGrpTable>());
    public Dictionary<int, MonGrpData> monGrpData = new Dictionary<int, MonGrpData>();
    public Dictionary<int, WorldAreaData> areaDataList = new Dictionary<int, WorldAreaData>(); //월드맵 구역 데이터
    public Dictionary<int, WorldMonData> worldMonDataList = new Dictionary<int, WorldMonData>();
    public void CreateWorldAreaData()
    {
        foreach (var monGrp in MonGrpTable.Datas)
            monGrpData[monGrp.GrpID] = new MonGrpData(monGrp.GrpID, monGrp.Grade, monGrp.Type, monGrp.Min, monGrp.Max, monGrp.LeaderID, monGrp.List);

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
            areaDataList[id].grpByGrade.Add(0, spawnMon.MG1.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(1, spawnMon.MG1.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(2, spawnMon.MG2.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(3, spawnMon.MG3.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(4, spawnMon.MG4.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(5, spawnMon.MG5.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(6, spawnMon.MG6.Split(',').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(7, spawnMon.MG7.Split(',').Select(int.Parse).ToList());
        }
        //추후에 curCnt는 세이브 데이터를 통해 갱신해야 함
        //curCnt는 maxCnt만큼만 적용되는데 적용되는 시점은 스폰타임(현재 1주일간격)에 1번 적용
    }
    public void AddWorldMonData(int uId, int areaID, int leaderID, List<int> monList, Vector3 pos)
    {
        worldMonDataList[uId] = new WorldMonData
        {
            areaID = areaID,
            ldID = leaderID,
            monList = monList,
            pos = pos
        };
    }
    public void RemoveWorldMonData(int uId)
    {
        worldMonDataList.Remove(uId);
    }
    public void UpdateWorldMonData(Dictionary<int, GameObject> wMonObj)
    {
        foreach (var wMon in wMonObj)
        {
            worldMonDataList[wMon.Key].pos = wMon.Value.transform.position;
        }
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
    public void RemoveWorldMonGrp()
    {
        List<int> list = btMonGrpUid;
        foreach (var t in list)
            worldMonDataList.Remove(t);
        btMonGrpUid.Clear();
    }
    #endregion

    #region 전투 참여 몬스터 관리
    public List<int> btMonList = new List<int>();
    public List<int> btMonGrpUid = new List<int>();
    public string GetAroundMon(List<int> grp, int uid, float x, float y, int n)
    {
        btMonList.Clear(); //전투에 참여하는 몬스터 ID
        btMonGrpUid.Clear(); //전투에 참여하는 몬스터의 파티 or 그룹 UID
        string str = "";
        foreach (var m in grp)
        {
            btMonList.Add(m);
            str += m + "_";
        }
        btMonGrpUid.Add(uid);
        // GameObject[] allMon = GameObject.FindGameObjectsWithTag("Monster");
        // for (int i = 0; i < allMon.Length; i++)
        // {
        //     if (n == i) continue;
        //     if (Vector2.Distance(allMon[i].transform.position, new Vector2(x, y)) < 10f)
        //     {
        //         wMon mon = allMon[i].GetComponent<wMon>();
        //         foreach (var m in mon.monGrp)
        //         {
        //             BattleMonList.Add(m);
        //             str += m + "_";
        //         }
        //     }
        // }
        str = str.TrimEnd('_');
        return str;
    }
    public void TestCreateMon()
    {
        btMonList.Clear();
        btMonList.Add(1);
        btMonList.Add(1);
        // btMonList.Add(1);
        // btMonList.Add(1);
    }
    #endregion
}
