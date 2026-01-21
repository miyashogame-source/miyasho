using UnityEngine;

public class EnemySpawnWall : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject enemyPrefab;
    public float spawnInterval = 5f;
    public int maxAliveEnemies = 3;

    [Header("Spawn Offset")]
    public float spawnOffset = 1.5f;   // ← 壁から離す距離（重要）
    public float heightOffset = 0.1f;  // ← 地面めり込み防止

    float timer;
    int aliveEnemies;

    void Awake()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Prefab が未設定です: " + gameObject.name);
            enabled = false;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && aliveEnemies < maxAliveEnemies)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        Vector3 spawnPos = GetSpawnPosition();

        // 壁の内側を向かせる
        Quaternion rot = Quaternion.LookRotation(-transform.forward);

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, rot);

        EnemyLife life = enemy.GetComponent<EnemyLife>();
        if (life != null)
            life.SetOwnerWall(this);

        aliveEnemies++;
    }

    Vector3 GetSpawnPosition()
    {
        // 壁の「内側」方向にオフセット
        Vector3 pos =
            transform.position +
            (-transform.forward * spawnOffset) +
            (Vector3.up * heightOffset);

        return pos;
    }

    public void OnEnemyDespawned()
    {
        aliveEnemies--;
        aliveEnemies = Mathf.Max(0, aliveEnemies);
    }

    // ===== Sceneビュー確認用 =====
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 pos =
            transform.position +
            (-transform.forward * spawnOffset) +
            (Vector3.up * heightOffset);

        Gizmos.DrawSphere(pos, 0.3f);
        Gizmos.DrawLine(transform.position, pos);
    }
}
