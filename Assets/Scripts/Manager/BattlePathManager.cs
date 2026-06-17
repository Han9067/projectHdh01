using System.Collections.Generic;
using UnityEngine;
using GB;

public class BattlePathManager : AutoSingleton<BattlePathManager>
{
    // 경로 구성용 리스트 재사용 (가비지 컬렉션 최적화)
    private List<Vector2Int> pathList = new List<Vector2Int>();
    private List<Vector2Int> lineList = new List<Vector2Int>();
    #region move
    // 기존 메서드 (1x1 전용 - 하위 호환성)
    public Vector2Int[] GetMovePath(Vector2Int sPos, Vector2Int ePos, BtGrid[,] gGrid)
    {
        return GetMovePath(sPos, ePos, gGrid, 1, 1);
    }

    // 크기 고려 경로 탐색 (왼쪽 상단 기준)
    public Vector2Int[] GetMovePath(Vector2Int sPos, Vector2Int ePos, BtGrid[,] gGrid, int width, int height)
    {
        return GetMovePath(sPos, ePos, gGrid, width, height, 0); // 기본값 0 (자기 자신 ID 없음)
    }

    // 자기 자신 ID를 고려한 오버로드
    public Vector2Int[] GetMovePath(Vector2Int sPos, Vector2Int ePos, BtGrid[,] gGrid, int width, int height, int selfId)
    {
        // 빠른 조기 반환
        if (sPos == ePos) return new Vector2Int[] { sPos };
        if (!IsValidPos(sPos, gGrid) || !IsValidPos(ePos, gGrid)) return new Vector2Int[] { };

        // BFS용 큐와 방문 체크
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(sPos);
        cameFrom[sPos] = sPos;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == ePos) return BuildPath(sPos, ePos, cameFrom);
            // 8방향 탐색 (정적 배열 사용)
            foreach (Vector2Int dir in DirData.dir8_1)
            {
                Vector2Int next = current + dir;
                if (IsValidPos(next, gGrid) && !cameFrom.ContainsKey(next) &&
                    IsAreaClear(next, ePos, gGrid, width, height, selfId))
                {
                    queue.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }
        // 경로를 찾지 못한 경우
        return new Vector2Int[] { };
        //추후에는 최소 값을 가진 경로가 여러개면 그 값들을 저장하고 그 중 하나를 랜덤으로 반환하도록 해야함
    }

    // 경로 구성 함수 분리 (재사용 가능)
    private Vector2Int[] BuildPath(Vector2Int _sPos, Vector2Int _ePos, Dictionary<Vector2Int, Vector2Int> _cameFrom)
    {
        pathList.Clear(); // 기존 리스트 재사용
        Vector2Int pos = _ePos;
        while (pos != _sPos)
        {
            pathList.Add(pos);
            pos = _cameFrom[pos];
        }

        // 역순으로 배열 생성
        Vector2Int[] path = new Vector2Int[pathList.Count];
        for (int i = 0; i < pathList.Count; i++)
            path[i] = pathList[pathList.Count - 1 - i];

        return path;
    }

    // 영역이 비어있는지 확인 (왼쪽 상단 기준)
    private bool IsAreaClear(Vector2Int topLeft, Vector2Int target, BtGrid[,] grid, int w, int h, int selfId)
    {
        for (int x = topLeft.x; x < topLeft.x + w; x++)
        {
            for (int y = topLeft.y; y < topLeft.y + h; y++)
            {
                if (x < 0 || x >= grid.GetLength(0) || y < 0 || y >= grid.GetLength(1))
                    return false;
                // 목적지가 아니고, 비어있지 않으며, 자기 자신의 ID도 아니면 이동 불가
                if (topLeft != target && grid[x, y].tId != 0 && grid[x, y].tId != selfId)
                    return false;
            }
        }
        return true;
    }

    private bool IsValidPos(Vector2Int pos, BtGrid[,] grid)
    {
        return pos.x >= 0 && pos.x < grid.GetLength(0) &&
               pos.y >= 0 && pos.y < grid.GetLength(1);
    }
    #endregion
    #region act & detect

    private enum RelQuad { UpLeft, UpRight, DownLeft, DownRight }

    public bool IsValidActPos(Vector2Int sPos, Vector2Int tPos, BtGrid[,] grid, int sId, int tId)
    {
        Vector2Int dif = tPos - sPos;
        // 같은 칸
        if (dif == Vector2Int.zero) return false;
        Vector2Int step;
        // ActDir dir;
        if (dif.x == 0 && dif.y != 0)
            step = new Vector2Int(0, dif.y > 0 ? 1 : -1);
        else if (dif.y == 0 && dif.x != 0)
            step = new Vector2Int(dif.x > 0 ? 1 : -1, 0);
        else
        {
            int dx = dif.x, dy = dif.y;
            RelQuad quad = dy > 0 ? (dx < 0 ? RelQuad.UpLeft : RelQuad.UpRight)
            : (dx < 0 ? RelQuad.DownLeft : RelQuad.DownRight);
            List<Vector2Int> list = new List<Vector2Int>();
            switch (quad)
            {
                case RelQuad.UpLeft:
                    if (dx >= -2)
                    {
                        list.Add(sPos + new Vector2Int(0, 1));
                        list.Add(sPos + new Vector2Int(-1, 1));
                        list.Add(tPos + new Vector2Int(0, -1));
                        list.Add(tPos + new Vector2Int(1, -1));
                    }
                    else
                    {
                        list.Add(sPos + new Vector2Int(0, 1));
                        list.Add(tPos + new Vector2Int(0, -1));
                    }
                    break;
                case RelQuad.UpRight:
                    if (dx <= 2)
                    {
                        list.Add(sPos + new Vector2Int(0, 1));
                        list.Add(sPos + new Vector2Int(1, 1));
                        list.Add(tPos + new Vector2Int(0, -1));
                        list.Add(tPos + new Vector2Int(-1, -1));
                    }
                    else
                    {
                        list.Add(sPos + new Vector2Int(0, 1));
                        list.Add(tPos + new Vector2Int(0, -1));
                    }
                    break;
                case RelQuad.DownLeft:
                    if (dx >= -2)
                    {
                        list.Add(sPos + new Vector2Int(0, -1));
                        list.Add(sPos + new Vector2Int(-1, -1));
                        list.Add(tPos + new Vector2Int(0, 1));
                        list.Add(tPos + new Vector2Int(1, 1));
                    }
                    else
                    {
                        list.Add(sPos + new Vector2Int(0, -1));
                        list.Add(tPos + new Vector2Int(0, 1));
                    }
                    break;
                case RelQuad.DownRight:
                    if (dx <= 2)
                    {
                        list.Add(sPos + new Vector2Int(0, -1));
                        list.Add(sPos + new Vector2Int(1, -1));
                        list.Add(tPos + new Vector2Int(0, 1));
                        list.Add(tPos + new Vector2Int(-1, 1));
                    }
                    else
                    {
                        list.Add(sPos + new Vector2Int(0, -1));
                        list.Add(tPos + new Vector2Int(0, 1));
                    }
                    break;
            }
            foreach (var t in list)
            {
                if (!IsValidPos(t, grid))
                    return false;
                if (grid[t.x, t.y].tId != 0 && grid[t.x, t.y].tId != sId && grid[t.x, t.y].tId != tId)
                    return false;
            }
            return true;
        }
        // sPos, tPos 사이 칸만 검사 (양 끝점 제외)
        Vector2Int cur = sPos + step;
        while (cur != tPos)
        {
            if (!IsValidPos(cur, grid))
                return false;
            if (grid[cur.x, cur.y].tId != 0)
                return false;
            cur += step;
        }
        return true;
    }

    // Bresenham 선상 칸 나열 (시작·끝 포함). 반환 리스트는 내부 버퍼이므로 보관 시 복사 필요.
    public List<Vector2Int> GetBresenhamLine(Vector2Int from, Vector2Int to)
    {
        BuildBresenhamLine(from, to, lineList);
        return lineList;
    }

    // Bresenham 직선 시야(느슨): 선상 중간 칸만 검사. 코너 옆 벽은 막지 않음. to가 벽(tId==2)이면 차단.
    public bool HasBresenhamLineOfSight(Vector2Int from, Vector2Int to, BtGrid[,] grid)
    {
        if (from == to) return false;
        if (!IsValidPos(from, grid) || !IsValidPos(to, grid)) return false;
        if (grid[to.x, to.y].tId == 2) return false;

        var line = GetBresenhamLine(from, to);
        for (int i = 1; i < line.Count - 1; i++)
        {
            if (!IsBresenhamCellClear(line[i], grid))
                return false;
        }
        return true;
    }

    private void BuildBresenhamLine(Vector2Int from, Vector2Int to, List<Vector2Int> buffer)
    {
        buffer.Clear();

        int x = from.x, y = from.y;
        int endX = to.x, endY = to.y;
        int dx = Mathf.Abs(endX - x);
        int dy = Mathf.Abs(endY - y);
        int sx = x < endX ? 1 : -1;
        int sy = y < endY ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            buffer.Add(new Vector2Int(x, y));
            if (x == endX && y == endY)
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
    }

    // 선상 장애물: 빈 칸(tId == 0), 탈출 타일(tId == 1) 통과
    private bool IsBresenhamCellClear(Vector2Int cell, BtGrid[,] grid)
    {
        if (!IsValidPos(cell, grid))
            return false;
        int tId = grid[cell.x, cell.y].tId;
        return tId == 0 || tId == 1;
    }
    #endregion
}
