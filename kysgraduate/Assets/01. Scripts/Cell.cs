using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2Int GridPos;
    public bool IsMine;
    public int NeighborMines;

    [Header("Renderers")]
    [SerializeField] private MeshRenderer mrNumber;

    public Texture2D[] numberTextures = new Texture2D[9];
    private MaterialPropertyBlock _mpb;
    private static readonly int BaseMapID = Shader.PropertyToID("_BaseMap");

    public void SetNumberTexture()
    {
        if (!mrNumber) return;
        //if (IsMine==true) NeighborMines = 0;
        if (_mpb == null) _mpb = new MaterialPropertyBlock();

        int idx = Mathf.Clamp(NeighborMines, 0, numberTextures.Length - 1);
        _mpb.Clear();
        _mpb.SetTexture(BaseMapID, numberTextures[idx]);
        mrNumber.SetPropertyBlock(_mpb);
    }
}