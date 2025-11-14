using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Camera")]
    public Camera mainCamera;

    private Rigidbody _rb;
    private Animator _animator;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; // 넘어지지 않게
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (mainCamera == null)
            mainCamera = Camera.main;

        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        RotateTowardsMouse();
        AnimateMove();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v).normalized;

        Vector3 velocity = input * moveSpeed;
        Vector3 newPos = _rb.position + velocity * Time.fixedDeltaTime;

        _rb.MovePosition(newPos);
    }

    private void AnimateMove()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool moving = !(Mathf.Abs(h) < 0.01f && Mathf.Abs(v) < 0.01f);
        _animator.SetBool("Moving", moving);
    }

    private void RotateTowardsMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 dir = (hitPoint - transform.position);
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                _rb.MoveRotation(targetRot);
            }
        }
    }
}
