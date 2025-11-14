using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public GameObject enemyPrefab;  // 생성할 에너미 프리팹
    public float spawnInterval = 5f;

    private Transform player;

    private void Start()
    {
        StartCoroutine(WaitForPlayerThenStartSpawning());
    }

    private IEnumerator WaitForPlayerThenStartSpawning()
    {
        // Player가 생성될 때까지 계속 찾기
        while (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null)
            {
                player = go.transform;
                break;
            }
            yield return null; // ← 다음 프레임까지 대기
        }

        // 찾았다면 스폰 시작
        StartCoroutine(SpawnLoop());
    }


    private IEnumerator SpawnLoop()
    {
        var wait = new WaitForSeconds(spawnInterval);

        while (true)
        {
            yield return wait;
            SpawnEnemyAtFarthestCorner();
        }
    }

    private void SpawnEnemyAtFarthestCorner()
    {
        if (enemyPrefab == null || player == null) return;

        GameManager gm = GameManager.Instance;

        // 보드의 네 꼭짓점 계산
        Vector3 c1 = new Vector3(gm.origin.x+1, gm.yLevel, gm.origin.z+1);
        Vector3 c2 = new Vector3(gm.origin.x + gm.width * gm.cellSize-1, gm.yLevel, gm.origin.z + 1);
        Vector3 c3 = new Vector3(gm.origin.x+1, gm.yLevel, gm.origin.z + gm.height * gm.cellSize-1);
        Vector3 c4 = new Vector3(gm.origin.x + gm.width * gm.cellSize-1, gm.yLevel, gm.origin.z + gm.height * gm.cellSize-1);

        // 플레이어와의 거리 비교
        Vector3 playerPos = player.position;
        Vector3[] corners = { c1, c2, c3, c4 };

        Vector3 farthest = corners[0];
        float maxDist = Vector3.Distance(playerPos, farthest);

        for (int i = 1; i < 4; i++)
        {
            float d = Vector3.Distance(playerPos, corners[i]);
            if (d > maxDist)
            {
                maxDist = d;
                farthest = corners[i];
            }
        }

        // 실제 생성 위치 (y 약간 위로 띄우기)
        Vector3 spawnPos = farthest + Vector3.up * 0.5f;

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
}
