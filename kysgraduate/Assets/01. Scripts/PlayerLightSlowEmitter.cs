using System.Collections.Generic;
using UnityEngine;

public class PlayerLightSlowEmitter : MonoBehaviour
{
    [Header("FOV Settings")]
    public float lightRange = 5f;
    [Range(0f, 180f)] public float lightHalfAngle = 60f; // 120° 라이트 → 60

    [Header("Detection")]
    public float checkInterval = 0.1f;
    public LayerMask enemyMask;

    [Header("Gizmos")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.yellow;
    [Range(8, 128)] public int coneSegments = 36;
    public int coneRings = 3;            // 원뿔 사이사이 링 갯수
    public float gizmoYOffset = 0.05f;   // 바닥 겹침 방지

    private float _timer;
    private readonly HashSet<Enemy> _inside = new();

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= checkInterval)
        {
            _timer = 0f;
            CheckEnemiesInCone();
        }
    }

    // === 3D 원뿔 판정 ===
    private void CheckEnemiesInCone()
    {
        // 범위 후보 수집
        var hits = Physics.OverlapSphere(transform.position, lightRange, enemyMask, QueryTriggerInteraction.Ignore);

        // 코사인 임계치(각도 비교를 빠르게)
        float halfRad = lightHalfAngle * Mathf.Deg2Rad;
        float cosThresh = Mathf.Cos(halfRad);

        foreach (var h in hits)
        {
            if (!h.TryGetComponent<Enemy>(out var enemy)) continue;
            // 3D 방향 벡터 (y 축 포함, 평면화 X)
            Vector3 toEnemy = (enemy.transform.position - transform.position);
            float sqr = toEnemy.sqrMagnitude;
            if (sqr < 0.0001f) continue;

            // 원뿔 여부: 정면과의 각도가 halfAngle 이내인가?
            Vector3 dir = toEnemy / Mathf.Sqrt(sqr);
            float cos = Vector3.Dot(transform.forward, dir);
            if (cos < cosThresh) // 각도 밖
            {
                enemy.RemoveLightSlow();
            }
            else
            {
                enemy.ApplyLightSlow();
            }
        }
    }

    // === 씬뷰 원뿔 Gizmo ===
    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // 기준 벡터 세팅
        Vector3 origin = transform.position + Vector3.up * gizmoYOffset;
        Vector3 fwd = transform.forward.normalized;

        float halfRad = lightHalfAngle * Mathf.Deg2Rad;
        float baseRadius = lightRange * Mathf.Tan(halfRad);

        // fwd와 수직인 보조축 만들기 (fwd가 위/아래일 때도 안전하게)
        Vector3 tempUp = Mathf.Abs(Vector3.Dot(fwd, Vector3.up)) > 0.99f ? Vector3.right : Vector3.up;
        Vector3 right = Vector3.Normalize(Vector3.Cross(tempUp, fwd));
        Vector3 up = Vector3.Normalize(Vector3.Cross(fwd, right));

        // 링(원) 그리기 함수
        void DrawCircle(Vector3 center, float radius, Color c)
        {
            Gizmos.color = c;
            Vector3 prev = center + right * radius;
            for (int i = 1; i <= coneSegments; i++)
            {
                float t = (float)i / coneSegments * Mathf.PI * 2f;
                Vector3 p = center + (Mathf.Cos(t) * right + Mathf.Sin(t) * up) * radius;
                Gizmos.DrawLine(prev, p);
                prev = p;
            }
        }

        // 원뿔 측면 라인 몇 개
        Gizmos.color = gizmoColor;
        int spokes = Mathf.Max(4, coneSegments / 6);
        for (int i = 0; i < spokes; i++)
        {
            float t = (float)i / spokes * Mathf.PI * 2f;
            Vector3 dirOnBase = (Mathf.Cos(t) * right + Mathf.Sin(t) * up);
            Vector3 tipToRim = fwd * lightRange + dirOnBase * baseRadius;
            Gizmos.DrawLine(origin, origin + tipToRim);
        }

        // 링(가이드)
        for (int r = 1; r <= coneRings; r++)
        {
            float k = (float)r / coneRings;
            Vector3 center = origin + fwd * (lightRange * k);
            float radius = baseRadius * k;
            DrawCircle(center, radius, new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f));
        }

        // 끝면(베이스) 강조
        DrawCircle(origin + fwd * lightRange, baseRadius, gizmoColor);

        // 중심선
        Gizmos.color = Color.white;
        Gizmos.DrawLine(origin, origin + fwd * lightRange);
    }
}
