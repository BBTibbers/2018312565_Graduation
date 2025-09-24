using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Camera")]
    public Camera mainCamera; // Inspector�� Camera.main �巡��

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (mainCamera == null)
            mainCamera = Camera.main;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        Move();
        RotateTowardsMouse();
    }

    private void Move()
    {
        // WASD �Է� (XZ ��� �̵�)
        float h = Input.GetAxisRaw("Horizontal"); // A, D
        float v = Input.GetAxisRaw("Vertical");   // W, S

        Vector3 input = new Vector3(h, 0f, v).normalized;

        // CharacterController �̵�
        controller.SimpleMove(input * moveSpeed);
    }

    private void RotateTowardsMouse()
    {
        // ȭ�� ��ǥ �� ���� ����
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // �ٴ�(y=0) ���� �浹�� ��� (�÷��̾ �ִ� ���̿� ����)
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            // ���� ���� (XZ ���)
            Vector3 dir = (hitPoint - transform.position);
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                // ���콺�� ���� ȸ��
                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = targetRot;
            }
        }
    }
}
