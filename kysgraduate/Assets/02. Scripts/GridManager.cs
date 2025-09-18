using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager I { get; private set; }
    private void Awake() => I = this;

    [Header("Board")]
    public int width = 16;
    public int height = 16;
    public int mineCount = 35;
    public float cellSize = 1f;
    public Vector2 origin = Vector2.zero;

    [Header("Refs")]
    public Cell cellPrefab;
    public Sprite[] numberSprites; // 0~8

    private Cell[,] _cells;
    private readonly HashSet<Cell> _openedCells = new(); // 시야 계산 최적화용

    public IEnumerable<Cell> OpenedCells => _openedCells;

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        _cells = new Cell[width, height];
        // 생성
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var pos = GridToWorld(x, y);
                var cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                cell.GridPos = new Vector2Int(x, y);
                _cells[x, y] = cell;
            }

        // 지뢰 배치
        PlaceMines();

        // 숫자 계산 & 숫자 스프라이트 세팅
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var c = _cells[x, y];
                if (c.IsMine) continue;
                c.Number = CountNeighbors(x, y);
                c.SetNumberSprite(numberSprites);
            }
    }

    private void PlaceMines()
    {
        var all = new List<Vector2Int>();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                all.Add(new Vector2Int(x, y));

        // 단순 셔플
        for (int i = 0; i < all.Count; i++)
        {
            int j = Random.Range(i, all.Count);
            (all[i], all[j]) = (all[j], all[i]);
        }

        for (int i = 0; i < mineCount && i < all.Count; i++)
        {
            var p = all[i];
            _cells[p.x, p.y].IsMine = true;
        }
    }

    private int CountNeighbors(int x, int y)
    {
        int cnt = 0;
        for (int dy = -1; dy <= 1; dy++)
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                int nx = x + dx, ny = y + dy;
                if (InBounds(nx, ny) && _cells[nx, ny].IsMine) cnt++;
            }
        return cnt;
    }

    public bool InBounds(int x, int y) =>
        x >= 0 && x < width && y >= 0 && y < height;

    public Vector3 GridToWorld(int x, int y) =>
        (Vector3)(origin + new Vector2((x + 0.5f) * cellSize, (y + 0.5f) * cellSize));

    public bool WorldToGrid(Vector3 world, out int x, out int y)
    {
        Vector2 p = (Vector2)world - origin;
        x = Mathf.FloorToInt(p.x / cellSize);
        y = Mathf.FloorToInt(p.y / cellSize);
        return InBounds(x, y);
    }

}