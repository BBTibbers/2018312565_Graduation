using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2Int GridPos;
    public bool IsMine;
    public int NeighborMines;
    public bool IsFlagged;
    public bool IsCovered = true;

    [Header("Renderers")]
    [SerializeField] private MeshRenderer mrNumber;

    public Texture2D[] NumberTextures = new Texture2D[9];
    public Texture2D FlagTexture;
    public Texture2D CoveredTexture;
    public Texture2D MineTexture;
    private MaterialPropertyBlock _mpb;
    private static readonly int BaseMapID = Shader.PropertyToID("_BaseMap");
    public void RefreshTexture()
    {
        if (!mrNumber) return;

        if(_mpb != null)
        _mpb.Clear();

        if (IsFlagged)
        {
            // 깃발 상태 → 깃발 텍스처 세팅
            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            if (FlagTexture != null)
                _mpb.SetTexture(BaseMapID, FlagTexture);

            mrNumber.SetPropertyBlock(_mpb);
        }
        else if (IsCovered)
        {
            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            if (CoveredTexture != null)
                _mpb.SetTexture(BaseMapID, CoveredTexture);

            mrNumber.SetPropertyBlock(_mpb);
        }
        else
        {
            SetNumberTexture();
        }

    }

    public void SetNumberTexture()
    {
        if (!mrNumber) return;
        if (IsMine==true) NeighborMines = 0;
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        _mpb.Clear();

        if (IsMine)
        {
            _mpb.SetTexture(BaseMapID, MineTexture);
        }
        else
        {
            int idx = Mathf.Clamp(NeighborMines, 0, NumberTextures.Length - 1);
            _mpb.SetTexture(BaseMapID, NumberTextures[idx]);
        }
        mrNumber.SetPropertyBlock(_mpb);
    }
    public void ToggleFlag()
    {
        if(!IsCovered) { return; }
        IsFlagged = !IsFlagged;
        RefreshTexture();
    }
    public void Open()
    {
        if (IsFlagged) return;
        if (!IsCovered) return;

        if (IsMine)
        {
            GridManager.Instance.ShowAllMine();
            Debug.Log("게임오버");
            // 필요하면 게임오버 처리 추가
            return;
        }

        // 숫자칸/0칸 공통: 최소한 현재 칸은 열린다
        IsCovered = false;
        RefreshTexture();

        if (NeighborMines == 0)
        {
            // 0이면 연결 오픈(플러드필)
            GridManager.Instance.RevealFlood(GridPos.x, GridPos.y);
        }
        else
        {
            RefreshTexture();
        }
    }
}