using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathManager : MonoBehaviour
{
    public static PathManager I;

    private void Awake()
    {
        if (I == null)
            I = this;
        else
            Destroy(gameObject);
    }

    public class Node
    {
        public Vector3Int position;
        public Node parent;
        public int gCost;  // 시작 노드부터 현재 노드까지의 비용
        public int hCost;  // 현재 노드에서 목표까지의 예상 비용 (휴리스틱)
        public int fCost => gCost + hCost;  // 총 비용

        public Node(Vector3Int pos)
        {
            position = pos;
        }
    }

    // 유클리드 거리 휴리스틱 (대각선 이동 허용 시 사용)
    private int Heuristic(Vector3Int a, Vector3Int b)
    {
        return Mathf.RoundToInt(Vector3Int.Distance(a, b));
    }

    // 실제 경로 찾기 메서드 (타일맵을 외부에서 전달받도록 변경)
    public List<Vector3> FindPath(Vector3 startWorldPos, Vector3 targetWorldPos, Tilemap tilemap)
    {
        Vector3Int start = tilemap.WorldToCell(startWorldPos);
        Vector3Int target = tilemap.WorldToCell(targetWorldPos);

        Dictionary<Vector3Int, Node> openSet = new Dictionary<Vector3Int, Node>();
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();

        Node startNode = new Node(start);
        startNode.gCost = 0;
        startNode.hCost = Heuristic(start, target);

        openSet.Add(start, startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = GetLowestFCostNode(openSet);

            if (currentNode.position == target)
            {
                return RetracePath(currentNode, tilemap);
            }

            openSet.Remove(currentNode.position);
            closedSet.Add(currentNode.position);

            foreach (Vector3Int neighborPos in GetNeighbors(currentNode.position))
            {
                if (closedSet.Contains(neighborPos) || !IsWalkable(neighborPos, tilemap))
                    continue;

                int tentativeGCost = currentNode.gCost + GetDistance(currentNode.position, neighborPos);

                if (!openSet.ContainsKey(neighborPos))
                {
                    Node neighborNode = new Node(neighborPos);
                    neighborNode.gCost = tentativeGCost;
                    neighborNode.hCost = Heuristic(neighborPos, target);
                    neighborNode.parent = currentNode;
                    openSet.Add(neighborPos, neighborNode);
                }
                else if (tentativeGCost < openSet[neighborPos].gCost)
                {
                    openSet[neighborPos].gCost = tentativeGCost;
                    openSet[neighborPos].parent = currentNode;
                }
            }
        }

        return null; // 경로를 찾을 수 없을 때
    }

    // 경로를 역추적하여 반환
    private List<Vector3> RetracePath(Node endNode, Tilemap tilemap)
    {
        List<Vector3> path = new List<Vector3>();
        Node currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(tilemap.GetCellCenterWorld(currentNode.position));
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }

    // 가장 낮은 F 비용을 가진 노드 반환
    private Node GetLowestFCostNode(Dictionary<Vector3Int, Node> openSet)
    {
        Node lowestFCostNode = null;
        foreach (Node node in openSet.Values)
        {
            if (lowestFCostNode == null || node.fCost < lowestFCostNode.fCost ||
               (node.fCost == lowestFCostNode.fCost && node.hCost < lowestFCostNode.hCost))
            {
                lowestFCostNode = node;
            }
        }
        return lowestFCostNode;
    }

    // 인접한 8방향 좌표 반환 (상, 하, 좌, 우 + 대각선)
    private List<Vector3Int> GetNeighbors(Vector3Int position)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>
        {
            new Vector3Int(position.x + 1, position.y, 0),     // 오른쪽
            new Vector3Int(position.x - 1, position.y, 0),     // 왼쪽
            new Vector3Int(position.x, position.y + 1, 0),     // 위
            new Vector3Int(position.x, position.y - 1, 0),     // 아래
            new Vector3Int(position.x + 1, position.y + 1, 0), // 오른쪽 위 대각선
            new Vector3Int(position.x - 1, position.y + 1, 0), // 왼쪽 위 대각선
            new Vector3Int(position.x + 1, position.y - 1, 0), // 오른쪽 아래 대각선
            new Vector3Int(position.x - 1, position.y - 1, 0)  // 왼쪽 아래 대각선
        };

        return neighbors;
    }

    // 두 노드 간 거리 반환 (직선: 10, 대각선: 14)
    private int GetDistance(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        if (dx > dy)
            return 14 * dy + 10 * (dx - dy);
        return 14 * dx + 10 * (dy - dx);
    }

    // 특정 좌표가 이동 가능한지 여부 확인 (타일맵을 외부에서 전달받도록 변경)
    private bool IsWalkable(Vector3Int position, Tilemap tilemap)
    {
        TileBase tile = tilemap.GetTile(position);
        return tile != null; // 타일이 존재하면 이동 가능, 없으면 불가
    }
}
