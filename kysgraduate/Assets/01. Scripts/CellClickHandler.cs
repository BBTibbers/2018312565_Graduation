using UnityEngine;

public class CellClickHandler : MonoBehaviour
{
    [SerializeField] private Camera _cam; // 비워두면 자동 MainCamera
    [SerializeField] private LayerMask _cellLayer = ~0; // 셀 전용 레이어 있으면 지정

    private void Awake()
    {
        if (_cam == null) _cam = Camera.main;
    }

    private void Update()
    {
        // PC: 우클릭
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000f, _cellLayer))
            {
                var cell = hit.collider.GetComponent<Cell>();
                if (cell != null) cell.ToggleFlag();
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000f, _cellLayer))
            {
                var cell = hit.collider.GetComponent<Cell>();
                if (cell != null) cell.Open();
            }
        }
    }
}
