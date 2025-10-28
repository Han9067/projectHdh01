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
    public class wmGrid
    {
        public int x;
        public int y;
        public string tName;
        public Vector3 worldPos;
    }
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
    #region 월드맵 그리드, 타일 관련
    private bool isCreate = false;
    public Dictionary<int, List<Vector3>> wAreaPos = new Dictionary<int, List<Vector3>>(); //영역 내 타일들의 월드 좌표
    public Dictionary<(int x, int y), wmGrid> wGridDic = new Dictionary<(int, int), wmGrid>(); //월드맵 그리드
    public void CreateWorldMapGrid(Tilemap tilemap)
    {
        if (isCreate) return;
        isCreate = true;
        wGridDic.Clear();
        wAreaPos.Clear();

        BoundsInt bounds = tilemap.cellBounds;
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int tPos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(tPos);

                wmGrid grid = new wmGrid();
                grid.x = x;
                grid.y = y;
                grid.tName = tile != null ? tile.name : "wt_o";
                grid.worldPos = tilemap.CellToWorld(tPos) + tilemap.cellSize * 0.5f;  // 월드 좌표도 저장

                wGridDic[(x, y)] = grid;
            }
        }
        #region 월드맵 그리드 전체 체크
        Dictionary<int, Vector3Int> wRoadSpotPos = new Dictionary<int, Vector3Int>();
        foreach (var kvp in wGridDic)
        {
            int x = kvp.Key.Item1;
            int y = kvp.Key.Item2;
            wmGrid grid = kvp.Value;

            Vector3Int tPos = new Vector3Int(x, y, 0);

            if (!string.IsNullOrEmpty(grid.tName) && grid.tName != "wt_o" && grid.tName.Length > 5)
            {
                string name = grid.tName;
                string type = name.Substring(0, 5);
                int num = int.Parse(name.Substring(5));

                switch (type)
                {
                    case "wt_r_":
                        if (num > 0)
                            wRoadSpotPos[num] = tPos;
                        break;
                    default:
                        int id = type == "wt_n_" ? num : 100 + num;
                        if (!wAreaPos.ContainsKey(id))
                            wAreaPos[id] = new List<Vector3>();

                        wAreaPos[id].Add(grid.worldPos);  // 캐싱된 월드 좌표 사용
                        break;
                }
            }
        }
        CreateWorldAreaData(); //월드맵 지역 데이터 생성
        #endregion
        #region 월드맵 도로 생성
        int[,] root = { { 1, 2 }, { 1, 3 } };
        int cnt = root.GetLength(0);
        for (int i = 0; i < cnt; i++)
        {
            int s = root[i, 0], e = root[i, 1];
            List<Vector3Int> road = FindRoadPath(wRoadSpotPos[s], wRoadSpotPos[e]);  // tilemap 매개변수 제거
            List<Vector3> p1 = new List<Vector3>(), p2 = new List<Vector3>();
            for (int j = 0; j < road.Count; j++)
            {
                // wGridDic에서 월드 좌표 가져오기
                p1.Add(wGridDic[(road[j].x, road[j].y)].worldPos);
                p2.Add(wGridDic[(road[road.Count - j - 1].x, road[road.Count - j - 1].y)].worldPos);
            }
            PlaceManager.I.CityDic[s].Road.Add($"{s}_{e}", p1);
            PlaceManager.I.CityDic[e].Road.Add($"{e}_{s}", p2);
        }
        #endregion
    }

    public List<Vector3Int> FindRoadPath(Vector3Int sPos, Vector3Int ePos)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> parent = new Dictionary<Vector3Int, Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        queue.Enqueue(sPos);
        visited.Add(sPos);
        parent[sPos] = sPos;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            if (current == ePos)
            {
                List<Vector3Int> path = new List<Vector3Int>();
                Vector3Int pos = ePos;

                while (pos != sPos)
                {
                    path.Add(pos);
                    pos = parent[pos];
                }
                path.Add(sPos);
                path.Reverse();

                return path;
            }

            foreach (Vector3Int dir in v3Dir8)
            {
                Vector3Int next = current + dir;
                if (visited.Contains(next))
                    continue;
                // wGridDic에서 타일 정보 가져오기
                if (!wGridDic.TryGetValue((next.x, next.y), out wmGrid grid))
                    continue;
                // 끝점이거나 "wt_r_0" 타일인 경우만 경로로 인정
                bool isValid = (next == ePos) || (grid.tName == "wt_r_0");
                if (isValid)
                {
                    visited.Add(next);
                    parent[next] = current;
                    queue.Enqueue(next);
                }
            }
        }

        return new List<Vector3Int>();
    }

    #endregion
    #region A* 길찾기 + 경로 최적화
    public class PathNode
    {
        public Vector3Int position;
        public float gCost;
        public float hCost;
        public float fCost => gCost + hCost;
        public PathNode parent;

        public PathNode(Vector3Int pos, float g, float h, PathNode p = null)
        {
            position = pos;
            gCost = g;
            hCost = h;
            parent = p;
        }
    }

    // 메인 길찾기 함수 (A* + 경로 최적화)
    public List<Vector3> FindPathOptimized(Vector3Int startCell, Vector3Int endCell)
    {
        List<Vector3> rawPath = FindPathAStar(startCell, endCell);
        // 경로 최적화 (불필요한 웨이포인트 제거)
        List<Vector3> smoothPath = SmoothPath(rawPath);

        return smoothPath;
    }

    // A* 알고리즘
    private List<Vector3> FindPathAStar(Vector3Int startCell, Vector3Int endCell)
    {
        List<PathNode> openList = new List<PathNode>();
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>();

        PathNode startNode = new PathNode(startCell, 0, GetHeuristic(startCell, endCell));
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // fCost가 가장 낮은 노드 선택
            PathNode currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].fCost < currentNode.fCost ||
                    (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                {
                    currentNode = openList[i];
                }
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode.position);

            // 목적지 도착
            if (currentNode.position == endCell)
                return RetracePath(currentNode);

            // 인접한 8방향 타일 탐색
            foreach (Vector3Int dir in v3Dir8)
            {
                Vector3Int neighborPos = currentNode.position + dir;

                if (closedList.Contains(neighborPos))
                    continue;

                if (!wGridDic.TryGetValue((neighborPos.x, neighborPos.y), out var grid))
                    continue;

                if (!IsWalkable(grid.tName))
                    continue;

                // 대각선 이동 비용 계산
                float moveCost = (dir.x != 0 && dir.y != 0) ? 1.414f : 1f;
                float newGCost = currentNode.gCost + moveCost;

                PathNode existingNode = openList.Find(n => n.position == neighborPos);

                if (existingNode == null)
                {
                    PathNode newNode = new PathNode(
                        neighborPos,
                        newGCost,
                        GetHeuristic(neighborPos, endCell),
                        currentNode
                    );
                    openList.Add(newNode);
                }
                else if (newGCost < existingNode.gCost)
                {
                    existingNode.gCost = newGCost;
                    existingNode.parent = currentNode;
                }
            }
        }
        Debug.Log("경로를 찾을 수 없습니다!");
        return null;
    }

    // 경로 역추적
    private List<Vector3> RetracePath(PathNode endNode)
    {
        List<Vector3> path = new List<Vector3>();
        PathNode currentNode = endNode;

        while (currentNode != null)
        {
            if (wGridDic.TryGetValue((currentNode.position.x, currentNode.position.y), out var grid))
            {
                path.Add(grid.worldPos);
            }
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    // 경로 최적화: Line of Sight를 이용한 웨이포인트 제거
    private List<Vector3> SmoothPath(List<Vector3> path)
    {
        if (path == null || path.Count <= 2)
            return path;

        List<Vector3> smoothedPath = new List<Vector3>();
        smoothedPath.Add(path[0]); // 시작점

        int currentIndex = 0;

        while (currentIndex < path.Count - 1)
        {
            int farthestIndex = currentIndex + 1;

            // 현재 위치에서 가장 멀리 직선으로 갈 수 있는 지점 찾기
            for (int i = currentIndex + 2; i < path.Count; i++)
            {
                if (HasLineOfSight(path[currentIndex], path[i]))
                {
                    farthestIndex = i;
                }
                else
                {
                    break; // 장애물이 있으면 중단
                }
            }

            smoothedPath.Add(path[farthestIndex]);
            currentIndex = farthestIndex;
        }

        return smoothedPath;
    }

    // 두 점 사이에 장애물이 없는지 확인 (Bresenham's Line Algorithm)
    private bool HasLineOfSight(Vector3 fromWorld, Vector3 toWorld)
    {
        // 월드 좌표를 셀 좌표로 변환 (Tilemap 필요 - WorldCore에서 전달받거나 참조)
        // 임시로 wGridDic를 역으로 찾는 방식 사용
        Vector3Int fromCell = WorldToCell(fromWorld);
        Vector3Int toCell = WorldToCell(toWorld);

        // Bresenham's Line Algorithm
        int dx = Mathf.Abs(toCell.x - fromCell.x);
        int dy = Mathf.Abs(toCell.y - fromCell.y);
        int sx = fromCell.x < toCell.x ? 1 : -1;
        int sy = fromCell.y < toCell.y ? 1 : -1;
        int err = dx - dy;

        int x = fromCell.x;
        int y = fromCell.y;

        while (true)
        {
            // 현재 셀이 이동 가능한지 체크
            if (!wGridDic.TryGetValue((x, y), out var grid))
                return false;

            if (!IsWalkable(grid.tName))
                return false;

            // 목적지 도착
            if (x == toCell.x && y == toCell.y)
                break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y += sy;
            }
        }

        return true;
    }

    // 월드 좌표를 셀 좌표로 변환 (가장 가까운 그리드 찾기)
    private Vector3Int WorldToCell(Vector3 worldPos)
    {
        float minDistance = float.MaxValue;
        Vector3Int closestCell = Vector3Int.zero;

        foreach (var kvp in wGridDic)
        {
            float distance = Vector3.Distance(worldPos, kvp.Value.worldPos);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestCell = new Vector3Int(kvp.Value.x, kvp.Value.y, 0);
            }
        }

        return closestCell;
    }

    // 타일 이동 가능 여부 체크
    private bool IsWalkable(string tileName)
    {
        if (tileName == "wt_x") return false; // 이동 불가 타일
        return true;
    }

    // 휴리스틱 함수 (Octile Distance - 8방향 이동에 최적)
    private float GetHeuristic(Vector3Int from, Vector3Int to)
    {
        float dx = Mathf.Abs(from.x - to.x);
        float dy = Mathf.Abs(from.y - to.y);
        // Octile distance: D * (dx + dy) + (D2 - 2 * D) * min(dx, dy)
        // D = 1, D2 = 1.414
        return (dx + dy) + (1.414f - 2f) * Mathf.Min(dx, dy);
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
