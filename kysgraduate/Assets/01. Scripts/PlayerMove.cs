using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Camera")]
    public Camera mainCamera; // Inspector에 Camera.main 드래그

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
        // WASD 입력 (XZ 평면 이동)
        float h = Input.GetAxisRaw("Horizontal"); // A, D
        float v = Input.GetAxisRaw("Vertical");   // W, S

        Vector3 input = new Vector3(h, 0f, v).normalized;

        // CharacterController 이동
        controller.SimpleMove(input * moveSpeed);
    }

    private void RotateTowardsMouse()
    {
        // 화면 좌표 → 월드 레이
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // 바닥(y=0) 평면과 충돌점 계산 (플레이어가 있는 높이에 맞춤)
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            // 방향 벡터 (XZ 평면)
            Vector3 dir = (hitPoint - transform.position);
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                // 마우스를 향해 회전
                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = targetRot;
            }
        }
    }
}
