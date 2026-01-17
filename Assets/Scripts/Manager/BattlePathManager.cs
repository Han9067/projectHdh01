using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class BattlePathManager : AutoSingleton<BattlePathManager>
{
    // 경로 구성용 리스트 재사용 (가비지 컬렉션 최적화)
    private List<Vector2Int> pathList = new List<Vector2Int>();

    // 기존 메서드 (1x1 전용 - 하위 호환성)
    public Vector2Int[] GetPath(Vector2Int sPos, Vector2Int ePos, BtGrid[,] gGrid)
    {
        return GetPath(sPos, ePos, gGrid, 1, 1);
    }

    // 크기 고려 경로 탐색 (왼쪽 상단 기준)
    public Vector2Int[] GetPath(Vector2Int sPos, Vector2Int ePos, BtGrid[,] gGrid, int width, int height)
    {
        return GetPath(sPos, ePos, gGrid, width, height, 0); // 기본값 0 (자기 자신 ID 없음)
    }

    // 자기 자신 ID를 고려한 오버로드
    public Vector2Int[] GetPath(Vector2Int sPos, Vector2Int ePos, BtGrid[,] gGrid, int width, int height, int selfId)
    {
        // 빠른 조기 반환
        if (sPos == ePos) return new Vector2Int[] { sPos };
        if (!IsValidPosition(sPos, gGrid) || !IsValidPosition(ePos, gGrid)) return new Vector2Int[] { };

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
            foreach (Vector2Int dir in Directions.Dir8)
            {
                Vector2Int next = current + dir;
                if (IsValidPosition(next, gGrid) && !cameFrom.ContainsKey(next) &&
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

    private bool IsValidPosition(Vector2Int pos, BtGrid[,] grid)
    {
        return pos.x >= 0 && pos.x < grid.GetLength(0) &&
               pos.y >= 0 && pos.y < grid.GetLength(1);
    }
}
