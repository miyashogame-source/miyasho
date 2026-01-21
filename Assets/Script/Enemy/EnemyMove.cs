using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyRandomPatrol : MonoBehaviour
{
    [Header("Move")]
    public float moveDistance = 5f;
    public float waitTime = 0.5f;
    public float obstacleCheckDistance = 2.0f;

    [Header("Direction")]
    public bool preferForward = true;
    public int maxDirectionTry = 8;

    [Header("OffMeshLink")]
    public float offMeshLinkMoveSpeed = 2.0f;

    NavMeshAgent agent;
    Vector3 currentDirection;
    float waitTimer;
    bool isTraversingLink;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;

        currentDirection = transform.forward.normalized;
        SetNextDestination();
    }

    void Update()
    {
        // ★ GameOver中は停止
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            if (agent != null)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
            return;
        }

        if (agent.isOnOffMeshLink && !isTraversingLink)
        {
            StartCoroutine(TraverseOffMeshLink());
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                SetNextDestination();
                waitTimer = 0f;
            }
        }
    }

    void SetNextDestination()
    {
        Vector3 origin = transform.position;

        if (TrySetDestination(origin, currentDirection))
            return;

        for (int i = 1; i <= maxDirectionTry; i++)
        {
            float angle = 45f * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * currentDirection;

            if (TrySetDestination(origin, dir))
            {
                currentDirection = dir.normalized;
                return;
            }

            dir = Quaternion.Euler(0, -angle, 0) * currentDirection;
            if (TrySetDestination(origin, dir))
            {
                currentDirection = dir.normalized;
                return;
            }
        }

        currentDirection = Quaternion.Euler(0, Random.Range(0, 8) * 45f, 0) * Vector3.forward;
        TrySetDestination(origin, currentDirection);
    }

    bool TrySetDestination(Vector3 origin, Vector3 direction)
    {
        direction = direction.normalized;

        if (Physics.Raycast(origin + Vector3.up * 0.5f, direction, obstacleCheckDistance))
            return false;

        Vector3 target = origin + direction * moveDistance;

        NavMeshHit hit;
        if (!NavMesh.SamplePosition(target, out hit, 1.0f, NavMesh.AllAreas))
            return false;

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(hit.position, path))
            return false;

        if (path.status != NavMeshPathStatus.PathComplete)
            return false;

        agent.SetDestination(hit.position);
        return true;
    }

    IEnumerator TraverseOffMeshLink()
    {
        isTraversingLink = true;

        OffMeshLinkData data = agent.currentOffMeshLinkData;
        Vector3 start = transform.position;
        Vector3 end = data.endPos + Vector3.up * agent.baseOffset;

        agent.isStopped = true;

        float time = 0f;
        float duration = Vector3.Distance(start, end) / offMeshLinkMoveSpeed;

        while (time < 1f)
        {
            time += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, end, time);
            yield return null;
        }

        transform.position = end;

        agent.CompleteOffMeshLink();
        agent.isStopped = false;
        isTraversingLink = false;
    }
}
