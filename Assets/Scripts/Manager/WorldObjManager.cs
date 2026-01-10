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
    public Vector3 pos, tgPos;
    public List<Vector3> path = new List<Vector3>();
}
public class WorldMarkerData
{
    public int type;
    public Vector3 pos;
}
public class WorldObjManager : AutoSingleton<WorldObjManager>
{
    public class wmGrid
    {
        public int x, y;
        public string tName, tType;
        public Vector3 worldPos;
        public float tCost; //타일 비용
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
    Dictionary<int, Vector3Int> wRoadSpotPos = new Dictionary<int, Vector3Int>();
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
                grid.tType = grid.tName.Substring(3, 1);
                grid.worldPos = tilemap.CellToWorld(tPos) + tilemap.cellSize * 0.5f;  // 월드 좌표도 저장
                grid.tCost = GetCost(grid.tName);
                wGridDic[(x, y)] = grid;
            }
        }
        #region 월드맵 그리드 전체 체크
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
        int[,] root = { { 1, 2 } };
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
    private float GetCost(string tileName)
    {
        switch (tileName.Substring(0, 4))
        {
            case "wt_r":
                return 0.6f;
            case "wt_f":
                return 1.2f;
            default:
                return 1f;
        }
    }
    #endregion
    #region A* 일반 필드 길찾기
    public class PathNode
    {
        public Vector3Int pos;
        public Vector3 worldPos; // 월드 좌표 캐싱
        public float gCost, hCost;
        public float fCost => gCost + hCost;
        public PathNode parent;

        public PathNode(Vector3Int p, Vector3 wPos, float g, float h, PathNode n = null)
        {
            pos = p;
            worldPos = wPos;
            gCost = g;
            hCost = h;
            parent = n;
        }
    }

    // 메인 길찾기 함수 (A* + 경로 최적화)
    public List<Vector3> FindPathOptimized(Vector3Int startCell, Vector3Int endCell)
    {
        wmPath rawPath = FindPathAStar(startCell, endCell);
        if (rawPath == null || rawPath.list == null || rawPath.list.Count == 0)
            return null;

        // 경로 최적화 (불필요한 웨이포인트 제거)
        wmPath result = SmoothPath(rawPath);

        return result.list;
    }

    // A* 알고리즘 (최적화 버전)
    private wmPath FindPathAStar(Vector3Int startCell, Vector3Int endCell)
    {
        // openList를 Dictionary로 관리하여 O(1) 조회
        Dictionary<Vector3Int, PathNode> openDict = new Dictionary<Vector3Int, PathNode>();
        List<PathNode> openList = new List<PathNode>(); // 정렬된 리스트 유지
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>();

        // 시작 노드 생성
        if (!wGridDic.TryGetValue((startCell.x, startCell.y), out var startGrid))
            return null;

        PathNode startNode = new PathNode(startCell, startGrid.worldPos, 0, GetHeuristic(startCell, endCell));
        openList.Add(startNode);
        openDict[startCell] = startNode;

        while (openList.Count > 0)
        {
            // fCost가 가장 낮은 노드 선택 (정렬된 리스트의 마지막 요소 사용)
            int bIdx = 0; //bestIndex
            float bfc = openList[0].fCost; //bestFCost
            float bhc = openList[0].hCost; //bestHCost

            for (int i = 1; i < openList.Count; i++)
            {
                PathNode node = openList[i];
                float nfc = node.fCost; //nodeFCost
                float nhc = node.hCost; //nodeHCost

                if (nfc < bfc ||
                    (nfc == bfc && nhc < bhc))
                {
                    bIdx = i;
                    bfc = nfc;
                    bhc = nhc;
                }
            }

            PathNode currentNode = openList[bIdx];
            // 마지막 요소와 교체 후 제거 (O(1))
            openList[bIdx] = openList[openList.Count - 1];
            openList.RemoveAt(openList.Count - 1);
            openDict.Remove(currentNode.pos);
            closedList.Add(currentNode.pos);

            // 목적지 도착
            if (currentNode.pos == endCell)
                return RetracePathWithCost(currentNode);

            // 인접한 8방향 타일 탐색
            foreach (Vector3Int dir in v3Dir8)
            {
                Vector3Int neighborPos = currentNode.pos + dir;

                if (closedList.Contains(neighborPos))
                    continue;

                if (!wGridDic.TryGetValue((neighborPos.x, neighborPos.y), out var grid))
                    continue;

                if (!IsWalkable(grid.tName))
                    continue;

                // 대각선 이동 비용 계산 + 타일 cost 적용
                float baseMoveCost = (dir.x != 0 && dir.y != 0) ? 1.414f : 1f;
                float moveCost = baseMoveCost * grid.tCost;
                float newGCost = currentNode.gCost + moveCost;

                // Dictionary로 O(1) 조회
                if (openDict.TryGetValue(neighborPos, out PathNode existingNode))
                {
                    if (newGCost < existingNode.gCost)
                    {
                        existingNode.gCost = newGCost;
                        existingNode.parent = currentNode;
                    }
                }
                else
                {
                    PathNode newNode = new PathNode(
                        neighborPos,
                        grid.worldPos,
                        newGCost,
                        GetHeuristic(neighborPos, endCell),
                        currentNode
                    );
                    openList.Add(newNode);
                    openDict[neighborPos] = newNode;
                }
            }
        }
        return null;
    }
    // 경로 역추적 + 가중치 계산
    private wmPath RetracePathWithCost(PathNode endNode)
    {
        List<Vector3> path = new List<Vector3>();
        List<PathNode> nodePath = new List<PathNode>();
        PathNode currentNode = endNode;
        float totalCost = 0f;

        // 경로 역추적
        while (currentNode != null)
        {
            path.Add(currentNode.worldPos);
            nodePath.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        nodePath.Reverse();

        // 가중치 계산 (각 노드 간 이동 비용 합산)
        for (int i = 0; i < nodePath.Count - 1; i++)
        {
            PathNode next = nodePath[i + 1];

            // 다음 타일의 비용 가져오기
            if (wGridDic.TryGetValue((next.pos.x, next.pos.y), out var nextGrid))
                totalCost += nextGrid.tCost;
        }

        wmPath result = new wmPath();
        result.list = path;
        result.cost = totalCost;

        return result;
    }
    private wmPath SmoothPath(wmPath path)
    {
        if (path == null || path.list == null || path.list.Count <= 2)
            return path;

        List<Vector3> smoothedPath = new List<Vector3>();
        smoothedPath.Add(path.list[0]); // 시작점

        int currentIndex = 0;
        float totalCost = 0f;

        while (currentIndex < path.list.Count - 1)
        {
            int farthestIndex = currentIndex + 1;

            // 현재 위치에서 가장 멀리 직선으로 갈 수 있는 지점 찾기
            for (int i = currentIndex + 2; i < path.list.Count; i++)
            {
                if (HasLineOfSight(path.list[currentIndex], path.list[i]))
                    farthestIndex = i;
                else
                    break; // 장애물이 있으면 중단
            }

            smoothedPath.Add(path.list[farthestIndex]);

            // 스무딩된 경로의 가중치 계산 (직선 거리 기반)
            Vector3Int toCell = WorldToCellOptimized(path.list[farthestIndex]);

            if (wGridDic.TryGetValue((toCell.x, toCell.y), out var grid))
            {
                float distance = Vector3.Distance(path.list[currentIndex], path.list[farthestIndex]);
                totalCost += distance * grid.tCost;
            }

            currentIndex = farthestIndex;
        }

        wmPath result = new wmPath();
        result.list = smoothedPath;
        result.cost = totalCost;

        return result;
    }

    // 두 점 사이에 장애물이 없는지 확인 (Bresenham's Line Algorithm)
    private bool HasLineOfSight(Vector3 fromWorld, Vector3 toWorld)
    {
        // 월드 좌표를 셀 좌표로 변환 (최적화: 직접 계산)
        Vector3Int fromCell = WorldToCellOptimized(fromWorld);
        Vector3Int toCell = WorldToCellOptimized(toWorld);

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

    // 최적화된 월드 좌표 -> 셀 좌표 변환 (캐싱 활용)
    private Dictionary<Vector3, Vector3Int> worldToCellCache = new Dictionary<Vector3, Vector3Int>();
    private Vector3Int WorldToCellOptimized(Vector3 worldPos)
    {
        // 정밀도를 낮춰서 캐싱 효율 향상 (0.1 단위로 반올림)
        Vector3 roundedPos = new Vector3(
            Mathf.Round(worldPos.x * 10f) * 0.1f,
            Mathf.Round(worldPos.y * 10f) * 0.1f, 0f
        );

        if (worldToCellCache.TryGetValue(roundedPos, out Vector3Int cached))
            return cached;

        // 캐시에 없으면 계산
        float minDistance = float.MaxValue;
        Vector3Int closestCell = Vector3Int.zero;

        foreach (var kvp in wGridDic)
        {
            float sqrDistance = (worldPos - kvp.Value.worldPos).sqrMagnitude; // SqrMagnitude 사용 (제곱근 계산 제거)
            if (sqrDistance < minDistance)
            {
                minDistance = sqrDistance;
                closestCell = new Vector3Int(kvp.Value.x, kvp.Value.y, 0);
            }
        }

        // 캐시에 저장 (캐시 크기 제한)
        if (worldToCellCache.Count > 1000)
            worldToCellCache.Clear(); // 캐시가 너무 커지면 초기화
        worldToCellCache[roundedPos] = closestCell;

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
        return (dx + dy) + (-0.586f) * Mathf.Min(dx, dy); //-0.586f = 1.414 - 2
    }
    #endregion
    #region 마을 이동 길찾기
    // public wmPath FindPathToCity(int tgCity)
    // {
    //     Vector3 pPos = WorldCore.I.GetPlayerPos();
    //     Vector3Int sRoadPos = WorldToCell(FindAroundRoad(pPos)); //타일 위치
    //     wmPath total = new wmPath();
    //     total.list = new List<Vector3>();
    //     total.cost = 0f;
    //     total.list.Add(pPos);

    //     wmPath path1 = FindPathOptimized(WorldToCell(pPos), sRoadPos);
    //     // path1.pos.RemoveAt(0);
    //     path1.list.RemoveAt(path1.list.Count - 1);
    //     foreach (var v in path1.list)
    //     {
    //         total.list.Add(v);
    //         total.cost += wGridDic[(WorldToCell(v).x, WorldToCell(v).y)].tCost;
    //     }
    //     List<Vector3Int> roadPath = FindRoadPath(sRoadPos, wRoadSpotPos[tgCity]);
    //     foreach (var v in roadPath)
    //     {
    //         total.list.Add(wGridDic[(v.x, v.y)].worldPos);
    //         total.cost += wGridDic[(v.x, v.y)].tCost;
    //     }
    //     return total;
    // }
    // private Vector3 FindAroundRoad(Vector3 pPos)
    // {
    //     Vector3Int playerCell = WorldToCell(pPos);

    //     // BFS를 위한 큐와 방문 체크
    //     Queue<Vector3Int> queue = new Queue<Vector3Int>();
    //     HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

    //     queue.Enqueue(playerCell);
    //     visited.Add(playerCell);

    //     // 최대 검색 범위 제한 (성능 최적화)
    //     int maxSearchRadius = 50; // 필요에 따라 조정 가능
    //     int currentRadius = 0;

    //     while (queue.Count > 0 && currentRadius < maxSearchRadius)
    //     {
    //         int levelSize = queue.Count; // 현재 레벨의 노드 개수
    //         currentRadius++;

    //         // 현재 레벨의 모든 노드 처리
    //         for (int i = 0; i < levelSize; i++)
    //         {
    //             Vector3Int current = queue.Dequeue();

    //             // 도로 타일인지 체크 (wt_r로 시작하는 타일)
    //             if (wGridDic.TryGetValue((current.x, current.y), out wmGrid grid))
    //             {
    //                 if (grid.tName.StartsWith("wt_r"))
    //                 {
    //                     // 도로를 찾았으면 월드 좌표 반환
    //                     return grid.worldPos;
    //                 }
    //             }

    //             // 8방향 인접 타일 탐색
    //             foreach (Vector3Int dir in v3Dir8)
    //             {
    //                 Vector3Int next = current + dir;

    //                 if (visited.Contains(next))
    //                     continue;

    //                 // 그리드에 존재하는지 체크
    //                 if (!wGridDic.TryGetValue((next.x, next.y), out wmGrid nextGrid))
    //                     continue;

    //                 // 이동 가능한 타일인지 체크 (도로 또는 이동 가능한 타일)
    //                 if (IsWalkable(nextGrid.tName) || nextGrid.tName.StartsWith("wt_r"))
    //                 {
    //                     visited.Add(next);
    //                     queue.Enqueue(next);
    //                 }
    //             }
    //         }
    //     }
    //     return Vector3.zero;
    // }
    #endregion

    #region 월드맵 오브젝트 스폰 관련
    private SpawnMonTable _spawnMonTable;
    public SpawnMonTable SpawnMonTable => _spawnMonTable ?? (_spawnMonTable = GameDataManager.GetTable<SpawnMonTable>());
    private MonGrpTable _monGrpTable;
    public MonGrpTable MonGrpTable => _monGrpTable ?? (_monGrpTable = GameDataManager.GetTable<MonGrpTable>());
    public Dictionary<int, MonGrpData> monGrpData = new Dictionary<int, MonGrpData>();
    public Dictionary<int, WorldAreaData> areaDataList = new Dictionary<int, WorldAreaData>(); //월드맵 구역 데이터
    public Dictionary<int, WorldMonData> worldMonDataList = new Dictionary<int, WorldMonData>();
    public Dictionary<int, WorldMarkerData> worldMarkerDataList = new Dictionary<int, WorldMarkerData>();
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
            areaDataList[id].grpByGrade.Add(0, spawnMon.MG1.Split('_').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(1, spawnMon.MG1.Split('_').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(2, spawnMon.MG2.Split('_').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(3, spawnMon.MG3.Split('_').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(4, spawnMon.MG4.Split('_').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(5, spawnMon.MG5.Split('_').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(6, spawnMon.MG6.Split('_').Select(int.Parse).ToList());
            areaDataList[id].grpByGrade.Add(7, spawnMon.MG7.Split('_').Select(int.Parse).ToList());
        }
        //추후에 curCnt는 세이브 데이터를 통해 갱신해야 함
        //curCnt는 maxCnt만큼만 적용되는데 적용되는 시점은 스폰타임(현재 1주일간격)에 1번 적용
    }
    public void AddWorldMonData(int uId, int areaID, int leaderID, List<int> monList, Vector3 pos, Vector3 tgPos, List<Vector3> path)
    {
        worldMonDataList[uId] = new WorldMonData
        {
            areaID = areaID,
            ldID = leaderID,
            monList = monList,
            pos = pos,
            tgPos = tgPos,
            path = path,
        };
    }
    public void RemoveWorldMonData(int uId)
    {
        worldMonDataList.Remove(uId);
    }
    public void UpdateWorldMonData(Dictionary<int, wMon> wMonObj)
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
    public void AddWorldMarkerData(int uId, int type, Vector3 pos)
    {
        worldMarkerDataList[uId] = new WorldMarkerData
        {
            type = type,
            pos = pos,
        };
    }
    public void UpdateWorldMarkerData(Dictionary<int, wMarker> wMarkerObj)
    {
        foreach (var wMarker in wMarkerObj)
        {
            worldMarkerDataList[wMarker.Key].pos = wMarker.Value.transform.position;
        }
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
    public void TutoMon()
    {
        btMonList.Clear();
        btMonList.Add(1);
    }
    public void TestCreateMon()
    {
        btMonList.Clear();
        btMonList.Add(2);
        // btMonList.Add(2);
        // btMonList.Add(1);
        // btMonList.Add(1);
    }
    #endregion

}
