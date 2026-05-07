using UnityEngine;

public class ReactiveAgent : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform player;

    [Header("Distâncias")]
    [SerializeField] private float pursueRange = 5f;
    [SerializeField] private float escapeRange = 2f;

    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Patrulha Opcional")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 1f;

    [Header("Debug")]
    [SerializeField] private bool ignoreYDistance = true;

    private int currentPatrolIndex = 0;
    private float patrolWaitCounter = 0f;

    private enum AgentState
    {
        Idle,
        Pursue,
        Escape,
        Patrol
    }

    [SerializeField] private AgentState currentState;

    private void Update()
    {
        if (player == null)
        {
            Debug.LogWarning($"{name}: Player não foi atribuído no ReactiveAgent.");
            return;
        }

        float distance = GetDistanceToPlayer();

        if (distance < escapeRange)
        {
            currentState = AgentState.Escape;
        }
        else if (distance < pursueRange)
        {
            currentState = AgentState.Pursue;
        }
        else
        {
            currentState = (patrolPoints != null && patrolPoints.Length > 0)
                ? AgentState.Patrol
                : AgentState.Idle;
        }

        switch (currentState)
        {
            case AgentState.Idle:
                break;

            case AgentState.Pursue:
                PursuePlayer();
                break;

            case AgentState.Escape:
                EscapeFromPlayer();
                break;

            case AgentState.Patrol:
                Patrol();
                break;
        }
    }

    private float GetDistanceToPlayer()
    {
        Vector3 myPos = transform.position;
        Vector3 playerPos = player.position;

        if (ignoreYDistance)
        {
            myPos.y = 0f;
            playerPos.y = 0f;
        }

        return Vector3.Distance(myPos, playerPos);
    }

    private Vector3 GetDirectionToPlayer()
    {
        Vector3 direction = player.position - transform.position;

        if (ignoreYDistance)
            direction.y = 0f;

        return direction.normalized;
    }

    private void PursuePlayer()
    {
        Vector3 direction = GetDirectionToPlayer();

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        transform.position += direction * moveSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void EscapeFromPlayer()
    {
        Vector3 direction = -GetDirectionToPlayer();

        if (direction.sqrMagnitude <= 0.0001f)
            return;

        transform.position += direction * moveSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        if (targetPoint == null)
            return;

        Vector3 targetPos = targetPoint.position;
        Vector3 currentPos = transform.position;

        if (ignoreYDistance)
            targetPos.y = currentPos.y;

        Vector3 direction = targetPos - currentPos;
        float distance = direction.magnitude;

        if (distance > 0.1f)
        {
            direction.Normalize();
            transform.position += direction * moveSpeed * Time.deltaTime;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            patrolWaitCounter += Time.deltaTime;

            if (patrolWaitCounter >= patrolWaitTime)
            {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                patrolWaitCounter = 0f;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pursueRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, escapeRange);
    }
}