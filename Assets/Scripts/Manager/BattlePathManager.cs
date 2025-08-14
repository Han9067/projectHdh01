using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GB;

public class BattlePathManager : AutoSingleton<BattlePathManager>
{
    // 정적 방향 배열로 메모리 할당 최적화 (8방향)
    private static readonly Vector2Int[] DIRECTIONS = {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left,
        new Vector2Int(1, 1),   new Vector2Int(1, -1),  new Vector2Int(-1, 1),  new Vector2Int(-1, -1)
    };

    // 경로 구성용 리스트 재사용 (가비지 컬렉션 최적화)
    private List<Vector2Int> pathList = new List<Vector2Int>();

    public Vector2Int[] GetPath(int sx, int sy, int ex, int ey, tileGrid[,] gGrid)
    {
        Vector2Int sPos = new Vector2Int(sx, sy), ePos = new Vector2Int(ex, ey);
        // 빠른 조기 반환
        if (sPos == ePos) return new Vector2Int[] { sPos };
        if (!IsValidPosition(sPos, gGrid) || !IsValidPosition(ePos, gGrid)) return new Vector2Int[] { };
        // if (gGrid[ePos.x, ePos.y].tId >= 1) return new Vector2Int[] { };

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
            foreach (Vector2Int dir in DIRECTIONS)
            {
                Vector2Int next = current + dir;
                if (IsValidPosition(next, gGrid) && !cameFrom.ContainsKey(next) && (gGrid[next.x, next.y].tId == 0 || next == ePos)) // 목적지라면 어떤 타일이든 통과
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

    private bool IsValidPosition(Vector2Int pos, tileGrid[,] grid)
    {
        return pos.x >= 0 && pos.x < grid.GetLength(0) &&
               pos.y >= 0 && pos.y < grid.GetLength(1);
    }
}
