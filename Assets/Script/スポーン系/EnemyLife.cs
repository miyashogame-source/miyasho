using UnityEngine;

public class EnemyLife : MonoBehaviour
{
    EnemySpawnWall ownerWall;

    public void SetOwnerWall(EnemySpawnWall wall)
    {
        ownerWall = wall;
    }

    public void Despawn()
    {
        if (ownerWall != null)
            ownerWall.OnEnemyDespawned();

        Destroy(gameObject);
    }
}
