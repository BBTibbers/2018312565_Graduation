using UnityEngine;

public class CellClickHandler : MonoBehaviour
{
    [SerializeField] private Camera _cam; // ����θ� �ڵ� MainCamera
    [SerializeField] private LayerMask _cellLayer = ~0; // �� ���� ���̾� ������ ����

    private void Awake()
    {
        if (_cam == null) _cam = Camera.main;
    }

    private void Update()
    {
        // PC: ��Ŭ��
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000f, _cellLayer))
            {
                var cell = hit.collider.GetComponent<Cell>();
                if (cell != null) cell.ToggleFlag();
            }
        }
    }
}
