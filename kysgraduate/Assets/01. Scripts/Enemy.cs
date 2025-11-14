using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))] // 충돌 처리용(선택)
public class Enemy : MonoBehaviour
{
    [Header("Chase")]
    public float moveSpeed = 1f;     // 직접 이동 시 속도
    public float delayedSpeed = 0.5f;
    public float turnSpeed = 720f;     // 초당 회전 각도(도)
    public float stopDistance = 0.5f;  // 너무 붙지 않도록 멈추는 거리

    private Transform _player;
    private NavMeshAgent _agent;   // 있으면 사용
    private Rigidbody _rb;         // 있으면 MovePosition 사용
    private Animator _anim;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();

        // Rigidbody를 쓰는 경우, 넘어짐 방지 권장
        if (_rb)
        {
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void Start()
    {
        // 시작 시 플레이어 찾기, 없으면 반복적으로 재시도
        TryFindPlayer();
        if (_player == null) StartCoroutine(CoRetryFindPlayer());

        // NavMeshAgent가 있다면 기본값 동기화
        if (_agent)
        {
            _agent.stoppingDistance = stopDistance;
            _agent.speed = moveSpeed;
            _agent.updateRotation = true; // Agent가 회전 관리
        }
    }

    private void Update()
    {
        if (_player == null) return;

        if (_agent && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
        {
            // NavMeshAgent 추적
            _agent.stoppingDistance = stopDistance;
            _agent.SetDestination(_player.position);
        }
        else
        {
            // 직접 이동(Transform/Rigidbody)
            ManualChase();
        }
        // ✅ 플레이어와 가까우면 게임 종료
        CheckPlayerCatch();
    }
    private void CheckPlayerCatch()
    {
        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist <= stopDistance + 1f) // 살짝 여유 주기
        {
            GameManager.Instance.ShowAllMine();
        }
    }


    private void ManualChase()
    {
        Vector3 toTarget = _player.position - transform.position;
        toTarget.y = 0f;
        float dist = toTarget.magnitude;

        if (dist <= stopDistance) return;

        Vector3 dir = toTarget / dist;

        // 이동
        Vector3 delta = dir * moveSpeed * Time.deltaTime;
        if (_rb) _rb.MovePosition(_rb.position + delta);
        else transform.position += delta;

        // 회전(부드럽게)
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            if (_rb) _rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime));
            else transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }

    private void TryFindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        _player = go ? go.transform : null;
    }

    private IEnumerator CoRetryFindPlayer()
    {
        // 씬에서 Player가 늦게 생성되는 경우 대비
        while (_player == null)
        {
            TryFindPlayer();
            if (_player != null) yield break;
            yield return new WaitForSeconds(0.25f);
        }
    }
    public void ApplyLightSlow()
    {
        if (_agent) _agent.speed = delayedSpeed;
        if (_anim) _anim.speed = 0.3f;
    }

    public void RemoveLightSlow()
    {
        if (_agent) _agent.speed = moveSpeed;
        if (_anim) _anim.speed = 1.5f;
    }
}
