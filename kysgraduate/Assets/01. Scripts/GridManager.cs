using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager I { get; private set; }
    private void Awake() => I = this;

    [Header("Board")]
    public int width = 16;        // X축 칸 수
    public int height = 16;       // Z축 칸 수(기존 y 의미였던 것을 Z로 사용)
    public int mineCount = 102;
    public float cellSize = 1f;

    [Tooltip("그리드 원점 (XZ 평면 기준). 예: (0,0,0)에서 시작")]
    public Vector3 origin = Vector3.zero;

    [Header("3D Placement")]
    [Tooltip("셀을 배치할 월드 Y 높이(바닥 높이).")]
    public float yLevel = 0f;
    public float wallYLevel = 0f;

    [Header("Refs")]
    public Cell cellPrefab;
    public GameObject wallPrefab;
    public GameObject playerPrefab;   // 🎮 플레이어 프리팹

    private Cell[,] _cells;
    private readonly HashSet<Cell> _openedCells = new(); // 시야 계산 최적화용
    public IEnumerable<Cell> OpenedCells => _openedCells;

    void Start()
    {
        Generate();
        BuildBorderWalls();
        SpawnPlayerAtCenter();
    }

    public void Generate()
    {
        _cells = new Cell[width, height];

        // 생성: XZ 격자에 셀 배치
        for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
            {
                var pos = GridToWorld(x, z);
                pos = pos - Vector3.up * 0.5f;
                var cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                cell.GridPos = new Vector2Int(x, z); // 인덱스는 (x, z)를 Vector2Int로 보관
                _cells[x, z] = cell;
            }

        // 지뢰 배치
        PlaceMines();

        // 숫자 계산 & 스프라이트 세팅
        for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
            {
                var c = _cells[x, z];
                //if (c.IsMine) continue;
                c.NeighborMines = CountNeighbors(x, z);
                c.SetNumberTexture();
            }
    }

    private void PlaceMines()
    {
        var all = new List<Vector2Int>();
        for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
                all.Add(new Vector2Int(x, z));

        // 단순 셔플
        for (int i = 0; i < all.Count; i++)
        {
            int j = Random.Range(i, all.Count);
            (all[i], all[j]) = (all[j], all[i]);
        }

        for (int i = 0; i < mineCount && i < all.Count; i++)
        {
            var p = all[i];
            _cells[p.x, p.y].IsMine = true; // p.y는 z 인덱스
        }
    }

    // 8방향 이웃 카운트(평면상 대각 포함)
    private int CountNeighbors(int x, int z)
    {
        int cnt = 0;
        for (int dz = -1; dz <= 1; dz++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dz == 0) continue;
                int nx = x + dx, nz = z + dz;
                if (InBounds(nx, nz) && _cells[nx, nz].IsMine) cnt++;
            }
        return cnt;
    }

    public bool InBounds(int x, int z) =>
        x >= 0 && x < width && z >= 0 && z < height;

    /// <summary>
    /// 격자 (x,z) 중심의 월드 좌표 반환 (XZ 평면, yLevel 높이).
    /// </summary>
    public Vector3 GridToWorld(int x, int z)
    {
        float wx = origin.x + (x + 0.5f) * cellSize;
        float wz = origin.z + (z + 0.5f) * cellSize;
        return new Vector3(wx, yLevel, wz);
    }

    /// <summary>
    /// 월드 좌표 → 격자 인덱스(x,z). 성공 시 true.
    /// </summary>
    public bool WorldToGrid(Vector3 world, out int x, out int z)
    {
        // XZ 평면에서 origin 기준 오프셋
        float px = world.x - origin.x;
        float pz = world.z - origin.z;

        x = Mathf.FloorToInt(px / cellSize);
        z = Mathf.FloorToInt(pz / cellSize);
        return InBounds(x, z);
    }

    /// <summary>
    /// 마우스 픽으로 얻은 히트 포인트에서 셀 찾고 싶을 때 사용:
    /// Physics.Raycast로 바닥 히트 지점(worldHit) 얻은 후 호출.
    /// </summary>
    public bool TryGetCellFromWorld(Vector3 worldHit, out Cell cell)
    {
        if (WorldToGrid(worldHit, out int x, out int z))
        {
            cell = _cells[x, z];
            return true;
        }
        cell = null;
        return false;
    }
    private void SpawnPlayerAtCenter()
    {
        if (playerPrefab == null) return;

        int centerX = width / 2;
        int centerZ = height / 2;

        Vector3 spawnPos = GridToWorld(centerX, centerZ);
        spawnPos.y = spawnPos.y + 0.5f;
        Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }
    private void BuildBorderWalls()
    {
        if (!wallPrefab) return;

        // 보드 경계(셀 중심이 아닌, 셀 외벽 라인)
        float minX = origin.x;
        float maxX = origin.x + width * cellSize;
        float minZ = origin.z;
        float maxZ = origin.z + height * cellSize;

        // 벽은 "한 칸" 단위로 배치 (셀 밖으로 0.5칸)
        float y = wallYLevel;

        Transform parent = new GameObject("Walls").transform;
        parent.SetParent(transform, false);

        // 상단/하단(가로 방향): x를 따라 반복, z는 maxZ+0.5, minZ-0.5
        for (int x = 0; x < width; x++)
        {
            float cx = origin.x + (x + 0.5f) * cellSize;

            // 상단
            Vector3 topPos = new Vector3(cx, y, maxZ + 0.5f * cellSize);
            Instantiate(wallPrefab, topPos, Quaternion.identity, parent);

            // 하단
            Vector3 botPos = new Vector3(cx, y, minZ - 0.5f * cellSize);
            Instantiate(wallPrefab, botPos, Quaternion.identity, parent);
        }

        // 좌/우(세로 방향): z를 따라 반복, x는 minX-0.5, maxX+0.5
        for (int z = 0; z < height; z++)
        {
            float cz = origin.z + (z + 0.5f) * cellSize;

            // 좌측
            Vector3 leftPos = new Vector3(minX - 0.5f * cellSize, y, cz);
            var left = Instantiate(wallPrefab, leftPos, Quaternion.identity, parent);

            // 우측
            Vector3 rightPos = new Vector3(maxX + 0.5f * cellSize, y, cz);
            var right = Instantiate(wallPrefab, rightPos, Quaternion.identity, parent);
        }
    }
}
