using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Perseguidor : MonoBehaviour
{
    [Header("Alvo")]
    public Transform player;

    [Header("Otimizacao")]
    [Tooltip("Recalcular destino a cada X segundos (0 = todo frame).")]
    public float intervaloAtualizacao = 0.2f;

    private NavMeshAgent agente;
    private float timer;

    private void Start()
    {
        agente = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            Debug.LogWarning("Perseguidor: arraste o player no Inspector!", this);
        }
    }

    private void Update()
    {
        if (player == null || agente == null || !agente.isOnNavMesh)
        {
            return;
        }

        if (intervaloAtualizacao <= 0f)
        {
            agente.SetDestination(player.position);
            return;
        }

        timer += Time.deltaTime;

        if (timer >= intervaloAtualizacao)
        {
            agente.SetDestination(player.position);
            timer = 0f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, player.position);
    }
}
