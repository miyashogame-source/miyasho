using UnityEngine;

public class EnemyDespawnTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Enter: " + other.name);

        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy Despawn!");
            Destroy(other.gameObject);
        }
    }
}
