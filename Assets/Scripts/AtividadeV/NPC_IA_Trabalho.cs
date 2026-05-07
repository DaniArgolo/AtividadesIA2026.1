using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPC_IA_Trabalho : MonoBehaviour
{
    // Define qual tipo de IA sera usada.
    private enum TipoIA
    {
        FSM,
        ArvoreDeDecisao
    }

    // Estados da maquina de estados finitos.
    private enum EstadoFSM
    {
        Patrol,
        Chase,
        Attack,
        Flee,
        Dead
    }

    // ============================================================
    // MODO DA IA
    // ============================================================

    [Header("Modo da IA")]
    [Tooltip("Escolhe se o NPC usa Maquina de Estados Finitos ou Arvore de Decisao.")]
    [SerializeField] private TipoIA tipoIA = TipoIA.FSM;

    // ============================================================
    // REFERENCIAS
    // ============================================================

    [Header("Referencias")]
    [Tooltip("Transform do Player. O NPC usa essa referencia para detectar, perseguir e atacar.")]
    [SerializeField] private Transform jogador;

    [Tooltip("Lista de pontos que o NPC percorre durante a patrulha.")]
    [SerializeField] private Transform[] pontosPatrulha;

    [Tooltip("Animator do NPC. Usado para disparar a animacao de ataque.")]
    [SerializeField] private Animator animator;

    [Tooltip("CharacterController do NPC. Usado para mover o NPC com colisao basica.")]
    [SerializeField] private CharacterController characterController;

    // ============================================================
    // MOVIMENTO
    // ============================================================

    [Header("Movimento")]
    [Tooltip("Velocidade do NPC enquanto esta patrulhando.")]
    [SerializeField] private float velocidadePatrulha = 2f;

    [Tooltip("Velocidade do NPC enquanto esta perseguindo o Player.")]
    [SerializeField] private float velocidadePerseguicao = 3.5f;

    [Tooltip("Velocidade do NPC enquanto esta fugindo do Player.")]
    [SerializeField] private float velocidadeFuga = 4f;

    [Tooltip("Velocidade com que o NPC gira para olhar para o destino.")]
    [SerializeField] private float velocidadeRotacao = 10f;

    [Tooltip("Distancia minima para considerar que o NPC chegou ao ponto de patrulha e pode ir para o proximo.")]
    [SerializeField] private float distanciaParaTrocarPonto = 0.8f;

    // ============================================================
    // DISTANCIA DE PARADA
    // ============================================================

    [Header("Distancia de Parada")]
    [Tooltip("Distancia em que o NPC para antes de colar no Player. Deve ser menor ou igual a distancia de ataque.")]
    [SerializeField] private float distanciaPararAntesDoJogador = 1.8f;

    // ============================================================
    // GRAVIDADE
    // ============================================================

    [Header("Gravidade")]
    [Tooltip("Forca da gravidade aplicada no NPC.")]
    [SerializeField] private float gravidade = -20f;

    [Tooltip("Forca aplicada para manter o NPC encostado no chao.")]
    [SerializeField] private float forcaGravidadeNoChao = -2f;

    // ============================================================
    // PERCEPCAO
    // ============================================================

    [Header("Percepcao")]
    [Tooltip("Raio em que o NPC detecta o Player e entra em Chase.")]
    [SerializeField] private float distanciaDeteccao = 8f;

    [Tooltip("Raio em que o NPC consegue atacar o Player.")]
    [SerializeField] private float distanciaAtaque = 2f;

    [Tooltip("Distancia em que o NPC considera que perdeu o Player e volta para Patrol.")]
    [SerializeField] private float distanciaPerderJogador = 12f;

    // ============================================================
    // VIDA
    // ============================================================

    [Header("Vida")]
    [Tooltip("Vida maxima do NPC.")]
    [SerializeField] private float vidaMaxima = 100f;

    [Tooltip("Vida atual do NPC.")]
    [SerializeField] private float vidaAtual = 100f;

    [Tooltip("Quando a vida do NPC ficar menor ou igual a esse valor, ele entra no estado Flee.")]
    [SerializeField] private float vidaParaFugir = 30f;

    // ============================================================
    // ATAQUE
    // ============================================================

    [Header("Ataque")]
    [Tooltip("Tempo entre um ataque e outro.")]
    [SerializeField] private float intervaloAtaque = 1f;

    [Tooltip("Dano causado no Player a cada ataque.")]
    [SerializeField] private float danoSimuladoPorAtaque = 10f;

    // ============================================================
    // ANIMATOR
    // ============================================================

    [Header("Animator")]
    [Tooltip("Nome do Trigger usado para tocar a animacao de ataque no Animator do NPC.")]
    [SerializeField] private string triggerAtaque = "Attack";

    // ============================================================
    // DEBUG
    // ============================================================

    [Header("Debug")]
    [Tooltip("Se ativado, mostra logs de mudanca de estado no Console.")]
    [SerializeField] private bool mostrarLogs = true;

    [Tooltip("Se ativado, mostra no Console o indice do ponto de patrulha atual.")]
    [SerializeField] private bool mostrarIndicePatrulha = false;

    // Estado atual da FSM.
    private EstadoFSM estadoAtual = EstadoFSM.Patrol;

    // Indice do ponto de patrulha atual.
    private int indicePatrulhaAtual = 0;

    // Controla o intervalo entre ataques.
    private float tempoProximoAtaque = 0f;

    // Velocidade vertical usada pela gravidade.
    private Vector3 velocidadeVertical;

    // Dados iniciais usados no reset.
    private Vector3 posicaoInicialNPC;
    private Quaternion rotacaoInicialNPC;

    private void Reset()
    {
        // Preenche referencias automaticamente.
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Awake()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        // Salva posicao inicial para reset.
        posicaoInicialNPC = transform.position;
        rotacaoInicialNPC = transform.rotation;

        // Inicializa vida e estado.
        vidaAtual = Mathf.Clamp(vidaAtual, 0f, vidaMaxima);
        estadoAtual = EstadoFSM.Patrol;
        indicePatrulhaAtual = 0;
    }

    private void Update()
    {
        AplicarGravidade();

        if (jogador == null)
        {
            return;
        }

        // Escolhe qual tipo de IA sera processada.
        if (tipoIA == TipoIA.FSM)
        {
            AtualizarFSM();
        }
        else
        {
            AtualizarArvoreDeDecisao();
        }
    }

    // ============================================================
    // FSM
    // ============================================================

    private void AtualizarFSM()
    {
        // A FSM executa apenas o comportamento do estado atual.
        switch (estadoAtual)
        {
            case EstadoFSM.Patrol:
                EstadoPatrol();
                break;

            case EstadoFSM.Chase:
                EstadoChase();
                break;

            case EstadoFSM.Attack:
                EstadoAttack();
                break;

            case EstadoFSM.Flee:
                EstadoFlee();
                break;

            case EstadoFSM.Dead:
                EstadoDead();
                break;
        }
    }

    private void EstadoPatrol()
    {
        Patrulhar();

        if (EstaMorto())
        {
            TrocarEstado(EstadoFSM.Dead);
            return;
        }

        if (VidaBaixa())
        {
            TrocarEstado(EstadoFSM.Flee);
            return;
        }

        if (JogadorDentroDaDeteccao())
        {
            TrocarEstado(EstadoFSM.Chase);
            return;
        }
    }

    private void EstadoChase()
    {
        PerseguirJogador();

        if (EstaMorto())
        {
            TrocarEstado(EstadoFSM.Dead);
            return;
        }

        if (VidaBaixa())
        {
            TrocarEstado(EstadoFSM.Flee);
            return;
        }

        if (JogadorDentroDoAtaque())
        {
            TrocarEstado(EstadoFSM.Attack);
            return;
        }

        if (JogadorMuitoLonge())
        {
            TrocarEstado(EstadoFSM.Patrol);
            return;
        }
    }

    private void EstadoAttack()
    {
        OlharPara(jogador.position);
        Atacar();

        if (EstaMorto())
        {
            TrocarEstado(EstadoFSM.Dead);
            return;
        }

        if (VidaBaixa())
        {
            TrocarEstado(EstadoFSM.Flee);
            return;
        }

        if (!JogadorDentroDoAtaque())
        {
            TrocarEstado(EstadoFSM.Chase);
            return;
        }
    }

    private void EstadoFlee()
    {
        FugirDoJogador();

        if (EstaMorto())
        {
            TrocarEstado(EstadoFSM.Dead);
            return;
        }

        if (DistanciaAteJogador() > distanciaPerderJogador)
        {
            TrocarEstado(EstadoFSM.Patrol);
            return;
        }
    }

    private void EstadoDead()
    {
        // Estado terminal.
        // Quando o NPC entra em Dead, ele nao executa mais movimento nem ataque.
    }

    private void TrocarEstado(EstadoFSM novoEstado)
    {
        // Evita trocar para o mesmo estado.
        if (estadoAtual == novoEstado)
        {
            return;
        }

        // Dead e terminal.
        if (estadoAtual == EstadoFSM.Dead)
        {
            return;
        }

        estadoAtual = novoEstado;

        if (mostrarLogs)
        {
            Debug.Log($"NPC mudou para o estado: {estadoAtual}");
        }
    }

    // ============================================================
    // ARVORE DE DECISAO
    // ============================================================

    private void AtualizarArvoreDeDecisao()
    {
        // A arvore pergunta condicoes em ordem de prioridade.

        if (EstaMorto())
        {
            AcaoDead();
            return;
        }

        if (VidaBaixa())
        {
            AcaoFlee();
            return;
        }

        if (JogadorDentroDaDeteccao())
        {
            if (JogadorDentroDoAtaque())
            {
                AcaoAttack();
            }
            else
            {
                AcaoChase();
            }

            return;
        }

        AcaoPatrol();
    }

    private void AcaoPatrol()
    {
        estadoAtual = EstadoFSM.Patrol;
        Patrulhar();
    }

    private void AcaoChase()
    {
        estadoAtual = EstadoFSM.Chase;
        PerseguirJogador();
    }

    private void AcaoAttack()
    {
        estadoAtual = EstadoFSM.Attack;
        OlharPara(jogador.position);
        Atacar();
    }

    private void AcaoFlee()
    {
        estadoAtual = EstadoFSM.Flee;
        FugirDoJogador();
    }

    private void AcaoDead()
    {
        estadoAtual = EstadoFSM.Dead;
    }

    // ============================================================
    // ACOES DO NPC
    // ============================================================

    private void Patrulhar()
    {
        if (pontosPatrulha == null || pontosPatrulha.Length == 0)
        {
            return;
        }

        indicePatrulhaAtual = Mathf.Clamp(
            indicePatrulhaAtual,
            0,
            pontosPatrulha.Length - 1
        );

        Transform pontoAtual = pontosPatrulha[indicePatrulhaAtual];

        // Se um ponto estiver vazio, pula para o proximo.
        if (pontoAtual == null)
        {
            AvancarIndicePatrulha();
            return;
        }

        MoverPara(pontoAtual.position, velocidadePatrulha);

        // Usa distancia horizontal para ignorar diferenca de altura.
        float distanciaAtePonto = DistanciaHorizontal(transform.position, pontoAtual.position);

        if (mostrarIndicePatrulha)
        {
            Debug.Log($"Patrulhando ponto indice: {indicePatrulhaAtual} | Distancia: {distanciaAtePonto}");
        }

        if (distanciaAtePonto <= distanciaParaTrocarPonto)
        {
            AvancarIndicePatrulha();
        }
    }

    private void AvancarIndicePatrulha()
    {
        if (pontosPatrulha == null || pontosPatrulha.Length == 0)
        {
            return;
        }

        indicePatrulhaAtual++;

        if (indicePatrulhaAtual >= pontosPatrulha.Length)
        {
            indicePatrulhaAtual = 0;
        }

        if (mostrarLogs)
        {
            Debug.Log($"Novo ponto de patrulha: {indicePatrulhaAtual}");
        }
    }

    private void PerseguirJogador()
    {
        float distancia = DistanciaHorizontal(transform.position, jogador.position);

        // Para antes de colar no Player.
        if (distancia <= distanciaPararAntesDoJogador)
        {
            OlharPara(jogador.position);
            return;
        }

        MoverPara(jogador.position, velocidadePerseguicao);
    }

    private void FugirDoJogador()
    {
        // Direcao oposta ao Player.
        Vector3 direcaoFuga = transform.position - jogador.position;
        direcaoFuga.y = 0f;

        if (direcaoFuga.sqrMagnitude < 0.01f)
        {
            direcaoFuga = -transform.forward;
        }

        Vector3 destinoFuga = transform.position + direcaoFuga.normalized;

        MoverPara(destinoFuga, velocidadeFuga);
    }

    private void Atacar()
    {
        // Controla intervalo entre ataques.
        if (Time.time < tempoProximoAtaque)
        {
            return;
        }

        tempoProximoAtaque = Time.time + intervaloAtaque;

        // Toca animacao de ataque.
        if (animator != null)
        {
            animator.SetTrigger(triggerAtaque);
        }

        // Procura o script do Player.
        PlayerCharacterController player = jogador.GetComponent<PlayerCharacterController>();

        if (player == null)
        {
            player = jogador.GetComponentInParent<PlayerCharacterController>();
        }

        if (player == null)
        {
            player = jogador.GetComponentInChildren<PlayerCharacterController>();
        }

        // Aplica dano no Player.
        if (player != null)
        {
            player.ReceberDano(danoSimuladoPorAtaque);
        }
        else
        {
            Debug.LogWarning("NPC tentou atacar, mas nao encontrou PlayerCharacterController no jogador.");
        }

        if (mostrarLogs)
        {
            Debug.Log("NPC atacou o jogador.");
        }
    }

    private void MoverPara(Vector3 destino, float velocidade)
    {
        destino.y = transform.position.y;

        Vector3 direcao = destino - transform.position;
        direcao.y = 0f;

        if (direcao.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Vector3 movimento = direcao.normalized * velocidade;

        // Movimento com CharacterController.
        characterController.Move(movimento * Time.deltaTime);

        OlharPara(destino);
    }

    private void AplicarGravidade()
    {
        if (characterController == null)
        {
            return;
        }

        if (characterController.isGrounded && velocidadeVertical.y < 0f)
        {
            velocidadeVertical.y = forcaGravidadeNoChao;
        }

        velocidadeVertical.y += gravidade * Time.deltaTime;

        characterController.Move(velocidadeVertical * Time.deltaTime);
    }

    private void OlharPara(Vector3 alvo)
    {
        Vector3 direcao = alvo - transform.position;
        direcao.y = 0f;

        if (direcao.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotacaoAlvo,
            velocidadeRotacao * Time.deltaTime
        );
    }

    // ============================================================
    // SENSORES / CONDICOES
    // ============================================================

    private float DistanciaHorizontal(Vector3 origem, Vector3 destino)
    {
        origem.y = 0f;
        destino.y = 0f;

        return Vector3.Distance(origem, destino);
    }

    private float DistanciaAteJogador()
    {
        return DistanciaHorizontal(transform.position, jogador.position);
    }

    private bool JogadorDentroDaDeteccao()
    {
        return DistanciaAteJogador() <= distanciaDeteccao;
    }

    private bool JogadorDentroDoAtaque()
    {
        return DistanciaAteJogador() <= distanciaAtaque;
    }

    private bool JogadorMuitoLonge()
    {
        return DistanciaAteJogador() >= distanciaPerderJogador;
    }

    private bool VidaBaixa()
    {
        return vidaAtual > 0f && vidaAtual <= vidaParaFugir;
    }

    private bool EstaMorto()
    {
        return vidaAtual <= 0f;
    }

    // ============================================================
    // RESET PUBLICO
    // ============================================================

    public void ResetarNPC()
    {
        vidaAtual = vidaMaxima;
        estadoAtual = EstadoFSM.Patrol;
        indicePatrulhaAtual = 0;
        tempoProximoAtaque = 0f;
        velocidadeVertical = Vector3.zero;

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        transform.position = posicaoInicialNPC;
        transform.rotation = rotacaoInicialNPC;

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        if (animator != null)
        {
            animator.ResetTrigger(triggerAtaque);
        }
    }

    // ============================================================
    // TESTES RAPIDOS PELO INSPECTOR
    // ============================================================

    [ContextMenu("Causar 25 de dano")]
    private void CausarDanoTeste()
    {
        vidaAtual -= 25f;
        vidaAtual = Mathf.Clamp(vidaAtual, 0f, vidaMaxima);
    }

    [ContextMenu("Matar NPC")]
    private void MatarNPCTeste()
    {
        vidaAtual = 0f;
    }

    [ContextMenu("Curar NPC")]
    private void CurarNPCTeste()
    {
        vidaAtual = vidaMaxima;

        if (estadoAtual == EstadoFSM.Dead)
        {
            estadoAtual = EstadoFSM.Patrol;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Amarelo: raio de deteccao.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccao);

        // Vermelho: raio de ataque.
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);

        // Rosa: distancia de parada antes do Player.
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, distanciaPararAntesDoJogador);

        if (pontosPatrulha == null || pontosPatrulha.Length == 0)
        {
            return;
        }

        // Ciano: pontos e caminho da patrulha.
        Gizmos.color = Color.cyan;

        for (int i = 0; i < pontosPatrulha.Length; i++)
        {
            if (pontosPatrulha[i] == null)
            {
                continue;
            }

            Gizmos.DrawSphere(pontosPatrulha[i].position, 0.25f);

            int proximoIndice = i + 1;

            if (proximoIndice >= pontosPatrulha.Length)
            {
                proximoIndice = 0;
            }

            if (pontosPatrulha[proximoIndice] != null)
            {
                Gizmos.DrawLine(
                    pontosPatrulha[i].position,
                    pontosPatrulha[proximoIndice].position
                );
            }
        }
    }
}