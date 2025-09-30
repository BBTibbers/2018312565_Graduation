using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2Int GridPos;
    public bool IsMine;
    public int NeighborMines;
    public bool IsFlagged;

    [Header("Renderers")]
    [SerializeField] private MeshRenderer mrNumber;

    public Texture2D[] NumberTextures = new Texture2D[9];
    public Texture2D FlagTexture;
    private MaterialPropertyBlock _mpb;
    private static readonly int BaseMapID = Shader.PropertyToID("_BaseMap");
    public void RefreshTexture()
    {
        if (!mrNumber) return;

        _mpb.Clear();

        if (IsFlagged)
        {
            // 깃발 상태 → 깃발 텍스처 세팅
            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            if (FlagTexture != null)
                _mpb.SetTexture(BaseMapID, FlagTexture);

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

        int idx = Mathf.Clamp(NeighborMines, 0, NumberTextures.Length - 1);
        _mpb.Clear();
        _mpb.SetTexture(BaseMapID, NumberTextures[idx]);
        mrNumber.SetPropertyBlock(_mpb);
    }
    public void ToggleFlag()
    {
        IsFlagged = !IsFlagged;
        RefreshTexture();
    }
}