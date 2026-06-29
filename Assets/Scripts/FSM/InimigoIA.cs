using UnityEngine;
using UnityEngine.AI;

public enum EstadoIA
{
    Patrulhando,
    Perseguindo,
    Investigando
}

[RequireComponent(typeof(NavMeshAgent))]
public class InimigoIA : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Transform[] pontosPatrulha;

    [Header("Deteccao")]
    [Tooltip("Raio em que o inimigo ve o player.")]
    public float raioVisao = 8f;

    [Tooltip("Distancia em que o inimigo perde o player de vista.")]
    public float raioPerda = 12f;

    [Tooltip("Angulo do cone de visao em graus.")]
    public float anguloVisao = 110f;

    [Tooltip("Layers que bloqueiam linha de visao, como Paredes.")]
    public LayerMask obstaculosLayer;

    [Header("Comportamento")]
    public float tempoInvestigando = 5f;
    public float distanciaChegada = 0.5f;
    public float velocidadePatrulha = 2.5f;
    public float velocidadePerseguicao = 5f;

    [Header("Debug (somente leitura)")]
    [SerializeField] private EstadoIA estadoAtual = EstadoIA.Patrulhando;

    private NavMeshAgent agente;
    private int indicePonto;
    private Vector3 ultimaPosicaoConhecida;
    private float timerInvestigando;

    private void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.stoppingDistance = 0.1f;

        if (player == null)
        {
            Debug.LogWarning("InimigoIA: arraste o Player no Inspector.", this);
        }

        if (pontosPatrulha != null && pontosPatrulha.Length > 0)
        {
            IrParaProximoPonto();
        }
        else
        {
            Debug.LogWarning("InimigoIA: configure os pontos de patrulha no Inspector.", this);
        }
    }

    private void Update()
    {
        if (agente == null || !agente.isOnNavMesh)
        {
            return;
        }

        switch (estadoAtual)
        {
            case EstadoIA.Patrulhando:
                AtualizarPatrulhando();
                break;
            case EstadoIA.Perseguindo:
                AtualizarPerseguindo();
                break;
            case EstadoIA.Investigando:
                AtualizarInvestigando();
                break;
        }
    }

    private void AtualizarPatrulhando()
    {
        agente.speed = velocidadePatrulha;

        if (!agente.pathPending && agente.remainingDistance < distanciaChegada)
        {
            IrParaProximoPonto();
        }

        if (PodeVerPlayer())
        {
            estadoAtual = EstadoIA.Perseguindo;
        }
    }

    private void IrParaProximoPonto()
    {
        if (pontosPatrulha == null || pontosPatrulha.Length == 0)
        {
            return;
        }

        Transform ponto = pontosPatrulha[indicePonto];
        indicePonto = (indicePonto + 1) % pontosPatrulha.Length;

        if (ponto != null)
        {
            agente.SetDestination(ponto.position);
        }
    }

    private void AtualizarPerseguindo()
    {
        if (player == null)
        {
            estadoAtual = EstadoIA.Patrulhando;
            IrParaProximoPonto();
            return;
        }

        agente.speed = velocidadePerseguicao;
        agente.SetDestination(player.position);

        float distancia = Vector3.Distance(transform.position, player.position);

        if (distancia > raioPerda || !PodeVerPlayer())
        {
            ultimaPosicaoConhecida = player.position;
            timerInvestigando = tempoInvestigando;
            estadoAtual = EstadoIA.Investigando;
        }
    }

    private void AtualizarInvestigando()
    {
        agente.speed = velocidadePerseguicao * 0.7f;
        agente.SetDestination(ultimaPosicaoConhecida);
        timerInvestigando -= Time.deltaTime;

        if (PodeVerPlayer())
        {
            estadoAtual = EstadoIA.Perseguindo;
            return;
        }

        bool chegou = !agente.pathPending && agente.remainingDistance < distanciaChegada;

        if (chegou || timerInvestigando <= 0f)
        {
            estadoAtual = EstadoIA.Patrulhando;
            IrParaProximoPonto();
        }
    }

    private bool PodeVerPlayer()
    {
        if (player == null)
        {
            return false;
        }

        Vector3 origem = transform.position + Vector3.up * 0.8f;
        Vector3 destino = player.position + Vector3.up * 0.8f;
        Vector3 direcao = destino - origem;
        float distancia = direcao.magnitude;

        if (distancia > raioVisao)
        {
            return false;
        }

        float angulo = Vector3.Angle(transform.forward, direcao);

        if (angulo > anguloVisao * 0.5f)
        {
            return false;
        }

        if (Physics.Raycast(origem, direcao.normalized, distancia, obstaculosLayer))
        {
            return false;
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.84f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, raioVisao);

        Gizmos.color = new Color(1f, 0.25f, 0.5f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, raioPerda);

        Vector3 esquerda = Quaternion.Euler(0f, -anguloVisao * 0.5f, 0f) * transform.forward;
        Vector3 direita = Quaternion.Euler(0f, anguloVisao * 0.5f, 0f) * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + esquerda * raioVisao);
        Gizmos.DrawLine(transform.position, transform.position + direita * raioVisao);

        if (estadoAtual == EstadoIA.Investigando)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, ultimaPosicaoConhecida);
            Gizmos.DrawWireSphere(ultimaPosicaoConhecida, 0.5f);
        }
    }
}
