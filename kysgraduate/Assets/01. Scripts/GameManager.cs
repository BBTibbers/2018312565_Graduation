using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake() => Instance = this;

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
    public NavMeshSurface surface;
    public event Action OnLose;
    public event Action OnWin;

    public int CountFlag = 0;
    public int CorrectMineCount;

    void Start()
    {
        Generate();
        BuildBorderWalls();
        BakeNavMesh();      // ← 여기서 굽기

        SpawnPlayer();
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
                cell.IsCovered = true;
                _cells[x, z] = cell;
            }

        // 지뢰 배치
        PlaceMines();

        // 숫자 계산 & 스프라이트 세팅
        for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
            {
                var c = _cells[x, z];
                c.NeighborMines = CountNeighbors(x, z);
                c.RefreshTexture();
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
            int j = UnityEngine.Random.Range(i, all.Count);
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
    private void SpawnPlayer()
    {
        if (playerPrefab == null || _cells == null) return;

        int x, z;

        // 0칸을 우선 탐색
        if (TryGetRandomZeroCell(out x, out z))
        {
            // 0칸이면 연결 오픈
            RevealFlood(x, z);                 // ← 이전에 만들어둔 플러드필 함수
        }

        // 그 위치에 플레이어 생성
        Vector3 spawnPos = GridToWorld(x, z);
        spawnPos.y += 0.5f;
        Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }
    private bool TryGetRandomZeroCell(out int rx, out int rz, int maxTries = 200)
    {
        // 빠른 시도: 완전 랜덤으로 몇 번 찍어보기
        for (int i = 0; i < maxTries; i++)
        {
            int x = UnityEngine.Random.Range(0, width);
            int z = UnityEngine.Random.Range(0, height);
            var c = _cells[x, z];
            if (!c.IsMine && c.NeighborMines == 0)
            {
                rx = x; rz = z;
                return true;
            }
        }

        // 백업: 보드 전체에서 0칸 수집 후 랜덤
        var zeros = new List<Vector2Int>();
        for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
                if (!_cells[x, z].IsMine && _cells[x, z].NeighborMines == 0)
                    zeros.Add(new Vector2Int(x, z));

        if (zeros.Count > 0)
        {
            var p = zeros[UnityEngine.Random.Range(0, zeros.Count)];
            rx = p.x; rz = p.y;
            return true;
        }

        rx = 0; rz = 0;
        return false;
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
    public void BakeNavMesh()
    {
        if (surface == null)
        {
            surface = GetComponent<NavMeshSurface>();
            if (surface == null) surface = gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            // 필요 시: surface.layerMask = LayerMask.GetMask("Ground");
        }

        surface.BuildNavMesh(); // 런타임 베이크
    }
    // 연결된 0영역과 그 경계의 숫자칸까지 모두 오픈
    public void RevealFlood(int sx, int sz)
    {
        if (!InBounds(sx, sz)) return;

        var visited = new bool[width, height];
        var q = new Queue<Vector2Int>();

        void EnqueueIfValid(int x, int z)
        {
            if (!InBounds(x, z)) return;
            if (visited[x, z]) return;
            var c = _cells[x, z];
            if (c.IsMine || c.IsFlagged) return;
            visited[x, z] = true;
            q.Enqueue(new Vector2Int(x, z));
        }

        EnqueueIfValid(sx, sz);

        while (q.Count > 0)
        {
            var p = q.Dequeue();
            var cur = _cells[p.x, p.y];

            cur.IsCovered = false;
            cur.RefreshTexture();

            // 0칸이면 8방향을 큐에 추가 (숫자칸이면 거기서 확장은 멈춤)
            if (cur.NeighborMines == 0)
            {
                for (int dz = -1; dz <= 1; dz++)
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dz == 0) continue;
                        EnqueueIfValid(p.x + dx, p.y + dz);
                    }
            }
        }
    }

    public void ShowAllMine()
    {
        for(int x =0; x<width; x++)
            for(int z = 0; z < height; z++)
            {
                if (_cells[x, z].IsMine) 
                {
                    _cells[x, z].IsCovered = false;
                    _cells[x, z].RefreshTexture();
                }
            }
        OnLose.Invoke();
    }

    // 필요시: 외부에서 특정 셀 얻기
    public Cell GetCell(int x, int z) => InBounds(x, z) ? _cells[x, z] : null;

    public void CheckWin()
    {
        if(CorrectMineCount == mineCount)
        {
            OnWin.Invoke();
        }
    }

}
