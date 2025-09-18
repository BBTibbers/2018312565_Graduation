using UnityEngine;

public class Cell : MonoBehaviour
{
    public int Number;
    public bool IsMine;
    public Vector2Int GridPos;

    private SpriteRenderer srNumber;

    private void Start()
    {
        srNumber = GetComponent<SpriteRenderer>();
    }

    public void SetNumberSprite(Sprite[] numberSprites)
    {
        // 0~8 ��������Ʈ ���� �־��ٸ�:
        if (Number >= 0 && Number < numberSprites.Length)
            srNumber.sprite = numberSprites[Number];
    }
}
