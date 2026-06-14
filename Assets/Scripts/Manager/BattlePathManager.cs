using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class BattlePathManager : AutoSingleton<BattlePathManager>
{
    // 경로 구성용 리스트 재사용 (가비지 컬렉션 최적화)
    private List<Vector2Int> pathList = new List<Vector2Int>();
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
    private enum ActDir { Up, Down, Left, Right }

    public bool IsValidActPos(Vector2Int sPos, Vector2Int tPos, BtGrid[,] grid, int sId, int tId)
    {
        Vector2Int dif = tPos - sPos;
        // 같은 칸
        if (dif == Vector2Int.zero)
            return false;
        Vector2Int step;
        ActDir dir;
        if (dif.x == 0 && dif.y != 0)
        {
            // 수직 직선 (상/하)
            step = new Vector2Int(0, dif.y > 0 ? 1 : -1);
            dir = dif.y > 0 ? ActDir.Up : ActDir.Down;
        }
        else if (dif.y == 0 && dif.x != 0)
        {
            // 수평 직선 (좌/우)
            step = new Vector2Int(dif.x > 0 ? 1 : -1, 0);
            dir = dif.x > 0 ? ActDir.Right : ActDir.Left;
        }
        else
        {
            // 직선이 아님 (대각선 포함) → 현재는 미지원
            // TODO: 추후 대각선 시야 필요 시 여기서 처리
            return false;
        }
        return IsLineClear(sPos, tPos, step, grid);
    }
    private bool IsLineClear(Vector2Int sPos, Vector2Int tPos, Vector2Int step, BtGrid[,] grid)
    {
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
    #endregion
}
