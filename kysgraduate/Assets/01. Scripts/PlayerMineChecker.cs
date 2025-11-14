using Unity.VisualScripting;
using UnityEngine;

public class PlayerMineChecker : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Cell")
        {
            Cell cell = collision.gameObject.GetComponent<Cell>();
            if (cell.IsMine == true && cell.IsFlagged == false)
            {
                GameManager.Instance.ShowAllMine();
            }
        }
    }
}
