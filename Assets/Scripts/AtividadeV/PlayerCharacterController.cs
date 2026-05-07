using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    // ============================================================
    // REFERENCIAS PRINCIPAIS
    // ============================================================

    [Header("Referencias")]
    [Tooltip("Animator do personagem. Usado para trocar Idle, Walk, Run, Hit e Death.")]
    [SerializeField] private Animator animator;

    [Tooltip("CharacterController do Player. Ele controla o movimento e a colisao basica do personagem.")]
    [SerializeField] private CharacterController characterController;

    [Tooltip("Camera usada como referencia para o movimento. O personagem anda relativo a direcao da camera.")]
    [SerializeField] private Transform cameraReferencia;

    // ============================================================
    // PAINEL DE VIDA WORLD SPACE
    // ============================================================

    [Header("Painel de Vida no Mundo")]
    [Tooltip("Canvas ou painel World Space que fica acima do personagem.")]
    [SerializeField] private Transform painelVidaPersonagem;

    [Tooltip("Imagem preenchida da barra de vida. Precisa estar com Image Type como Filled.")]
    [SerializeField] private Image barraVidaFill;

    [Tooltip("Se ativado, o script usa a posicao inicial do painel na cena como distancia fixa em relacao ao Player.")]
    [SerializeField] private bool usarPosicaoInicialDoPainel = true;

    [Tooltip("Offset manual do painel de vida em relacao ao Player. So e usado se 'Usar Posicao Inicial Do Painel' estiver desativado.")]
    [SerializeField] private Vector3 offsetPainelVida = new Vector3(0f, 2.2f, 0f);

    [Tooltip("Se ativado, o painel deixa de ser filho do Player no Start. Isso impede que ele gire junto com o personagem.")]
    [SerializeField] private bool soltarPainelVidaDoPlayer = true;

    [Tooltip("Ative se o painel ficar virado ao contrario para a camera.")]
    [SerializeField] private bool inverterPainelVida = false;

    // ============================================================
    // RESET DO NPC
    // ============================================================

    [Header("Referencia do NPC para Resetar")]
    [Tooltip("NPC que sera resetado junto com o Player ao clicar no botao do painel de morte.")]
    [SerializeField] private Transform npcParaResetar;

    // ============================================================
    // MOVIMENTO
    // ============================================================

    [Header("Movimento")]
    [Tooltip("Velocidade normal do personagem ao andar.")]
    [SerializeField] private float velocidadeWalk = 3f;

    [Tooltip("Velocidade do personagem ao correr segurando Shift.")]
    [SerializeField] private float velocidadeRun = 6f;

    [Tooltip("Velocidade com que o personagem gira para olhar na direcao do movimento.")]
    [SerializeField] private float velocidadeRotacao = 12f;

    // ============================================================
    // GRAVIDADE
    // ============================================================

    [Header("Gravidade")]
    [Tooltip("Forca da gravidade aplicada no CharacterController. Valor negativo faz o personagem cair.")]
    [SerializeField] private float gravidade = -20f;

    [Tooltip("Forca vertical aplicada quando o personagem esta no chao. Ajuda a manter o CharacterController grudado no solo.")]
    [SerializeField] private float forcaGravidadeNoChao = -2f;

    // ============================================================
    // VIDA
    // ============================================================

    [Header("Vida")]
    [Tooltip("Vida maxima do personagem.")]
    [SerializeField] private float vidaMaxima = 100f;

    [Tooltip("Vida atual do personagem. Diminui quando o NPC ataca.")]
    [SerializeField] private float vidaAtual = 100f;

    // ============================================================
    // PAINEL DE MORTE
    // ============================================================

    [Header("Painel de Morte")]
    [Tooltip("Painel do Canvas que aparece quando o Player morre. Deve conter o botao de reset.")]
    [SerializeField] private GameObject painelMorte;

    // ============================================================
    // ANIMATOR
    // ============================================================

    [Header("Animator")]
    [Tooltip("Nome do parametro Float usado para controlar Idle, Walk e Run no Animator.")]
    [SerializeField] private string parametroVelocidade = "Speed";

    [Tooltip("Nome do Trigger usado para tocar a animacao de dano.")]
    [SerializeField] private string triggerDano = "Hit";

    [Tooltip("Nome do Trigger usado para tocar a animacao de morte.")]
    [SerializeField] private string triggerMorte = "Death";

    [Tooltip("Suavizacao do parametro de velocidade no Animator. Quanto maior, mais suave a troca entre Idle, Walk e Run.")]
    [SerializeField] private float suavizacaoAnimator = 0.12f;

    // Entrada de movimento lida pelo New Input System.
    private Vector2 inputMovimento;

    // Guarda se o jogador esta segurando Shift.
    private bool inputCorrida;

    // Velocidade vertical usada pela gravidade.
    private Vector3 velocidadeVertical;

    // Valor suavizado enviado para o Animator.
    private float velocidadeAnimacaoAtual;

    // Posicao e rotacao inicial do Player para reset.
    private Vector3 posicaoInicialPlayer;
    private Quaternion rotacaoInicialPlayer;

    // Posicao e rotacao inicial do NPC para reset.
    private Vector3 posicaoInicialNPC;
    private Quaternion rotacaoInicialNPC;

    // Offset salvo entre o painel de vida e o Player.
    private Vector3 offsetInicialPainelVida;

    // Controla se o Player morreu.
    private bool estaMorto;

    private void Reset()
    {
        // Preenche referencias automaticamente quando o script e colocado no objeto.
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        if (Camera.main != null)
        {
            cameraReferencia = Camera.main.transform;
        }
    }

    private void Awake()
    {
        // Garante que as referencias existam mesmo que o usuario esqueca de arrastar no Inspector.
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (cameraReferencia == null && Camera.main != null)
        {
            cameraReferencia = Camera.main.transform;
        }
    }

    private void Start()
    {
        // Salva a posicao inicial do Player para o reset.
        posicaoInicialPlayer = transform.position;
        rotacaoInicialPlayer = transform.rotation;

        // Salva a posicao inicial do NPC para o reset.
        if (npcParaResetar != null)
        {
            posicaoInicialNPC = npcParaResetar.position;
            rotacaoInicialNPC = npcParaResetar.rotation;
        }

        // Configura o painel de vida World Space.
        ConfigurarPainelVidaInicial();

        // Inicializa a vida.
        vidaAtual = vidaMaxima;
        estaMorto = false;

        AtualizarBarraVida();
        AtualizarPainelVidaPersonagem();

        // Esconde o painel de morte no inicio.
        if (painelMorte != null)
        {
            painelMorte.SetActive(false);
        }
    }

    private void Update()
    {
        // Se morreu, bloqueia movimento e deixa a animacao de velocidade voltar para zero.
        if (estaMorto)
        {
            ZerarAnimacaoMovimento();
            return;
        }

        LerInputs();
        Mover();
        AplicarGravidade();
    }

    private void LateUpdate()
    {
        // Atualiza o painel no LateUpdate para acompanhar o Player depois do movimento.
        AtualizarPainelVidaPersonagem();
    }

    private void ConfigurarPainelVidaInicial()
    {
        if (painelVidaPersonagem == null)
        {
            return;
        }

        // Salva a distancia inicial entre o painel e o Player.
        if (usarPosicaoInicialDoPainel)
        {
            offsetInicialPainelVida = painelVidaPersonagem.position - transform.position;
        }
        else
        {
            offsetInicialPainelVida = offsetPainelVida;
        }

        // Solta o painel da hierarquia do Player para ele nao herdar a rotacao do personagem.
        if (soltarPainelVidaDoPlayer)
        {
            painelVidaPersonagem.SetParent(null, true);
        }
    }

    private void AtualizarPainelVidaPersonagem()
    {
        if (painelVidaPersonagem == null)
        {
            return;
        }

        if (cameraReferencia == null)
        {
            if (Camera.main == null)
            {
                return;
            }

            cameraReferencia = Camera.main.transform;
        }

        // Mantem o painel acima do Player usando o offset salvo.
        painelVidaPersonagem.position = transform.position + offsetInicialPainelVida;

        // Faz o painel olhar para a camera.
        Vector3 direcaoParaCamera = cameraReferencia.position - painelVidaPersonagem.position;

        if (direcaoParaCamera.sqrMagnitude <= 0.001f)
        {
            return;
        }

        painelVidaPersonagem.rotation = Quaternion.LookRotation(-direcaoParaCamera.normalized, Vector3.up);

        // Corrige caso o Canvas fique de costas.
        if (inverterPainelVida)
        {
            painelVidaPersonagem.rotation *= Quaternion.Euler(0f, 180f, 0f);
        }
    }

    private void LerInputs()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        inputMovimento = Vector2.zero;

        // Leitura manual do WASD usando New Input System.
        if (Keyboard.current.wKey.isPressed)
        {
            inputMovimento.y += 1;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            inputMovimento.y -= 1;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            inputMovimento.x -= 1;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            inputMovimento.x += 1;
        }

        inputMovimento = inputMovimento.normalized;

        // Shift ativa corrida.
        inputCorrida =
            Keyboard.current.leftShiftKey.isPressed ||
            Keyboard.current.rightShiftKey.isPressed;
    }

    private void Mover()
    {
        Vector3 input = new Vector3(inputMovimento.x, 0f, inputMovimento.y);

        bool estaMovendo = input.sqrMagnitude > 0.01f;

        float velocidadeAtual = inputCorrida ? velocidadeRun : velocidadeWalk;

        // Converte o input para direcao baseada na camera.
        Vector3 direcaoMovimento = CalcularDirecaoComCamera(input);

        if (estaMovendo)
        {
            // Move usando CharacterController.
            characterController.Move(direcaoMovimento * velocidadeAtual * Time.deltaTime);

            // Gira o personagem para olhar na direcao do movimento.
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcaoMovimento);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacaoAlvo,
                velocidadeRotacao * Time.deltaTime
            );
        }

        // O Animator usa 0 para Idle, 1 para Walk e 2 para Run.
        float valorAnimator = 0f;

        if (estaMovendo)
        {
            valorAnimator = inputCorrida ? 2f : 1f;
        }

        // Suaviza a troca de animacao.
        velocidadeAnimacaoAtual = Mathf.Lerp(
            velocidadeAnimacaoAtual,
            valorAnimator,
            1f - Mathf.Exp(-Time.deltaTime / suavizacaoAnimator)
        );

        if (animator != null)
        {
            animator.SetFloat(parametroVelocidade, velocidadeAnimacaoAtual);
        }
    }

    private Vector3 CalcularDirecaoComCamera(Vector3 input)
    {
        if (cameraReferencia == null)
        {
            return input;
        }

        // Pega frente e direita da camera.
        Vector3 frenteCamera = cameraReferencia.forward;
        Vector3 direitaCamera = cameraReferencia.right;

        // Ignora inclinacao vertical da camera.
        frenteCamera.y = 0f;
        direitaCamera.y = 0f;

        frenteCamera.Normalize();
        direitaCamera.Normalize();

        // Combina input com direcoes da camera.
        Vector3 direcao =
            frenteCamera * input.z +
            direitaCamera * input.x;

        if (direcao.sqrMagnitude > 1f)
        {
            direcao.Normalize();
        }

        return direcao;
    }

    private void AplicarGravidade()
    {
        // Mantem o personagem grudado no chao.
        if (characterController.isGrounded && velocidadeVertical.y < 0f)
        {
            velocidadeVertical.y = forcaGravidadeNoChao;
        }

        // Aplica gravidade.
        velocidadeVertical.y += gravidade * Time.deltaTime;

        characterController.Move(velocidadeVertical * Time.deltaTime);
    }

    public void ReceberDano(float dano)
    {
        if (estaMorto)
        {
            return;
        }

        // Reduz vida.
        vidaAtual -= dano;
        vidaAtual = Mathf.Clamp(vidaAtual, 0f, vidaMaxima);

        AtualizarBarraVida();

        // Se a vida chegou a zero, morre.
        if (vidaAtual <= 0f)
        {
            Morrer();
            return;
        }

        // Se ainda esta vivo, toca animacao de dano.
        if (animator != null)
        {
            animator.SetTrigger(triggerDano);
        }
    }

    private void Morrer()
    {
        estaMorto = true;

        // Para movimento.
        inputMovimento = Vector2.zero;
        inputCorrida = false;
        velocidadeVertical = Vector3.zero;

        ZerarAnimacaoMovimento();

        // Toca animacao de morte.
        if (animator != null)
        {
            animator.SetTrigger(triggerMorte);
        }

        // Mostra painel de morte.
        if (painelMorte != null)
        {
            painelMorte.SetActive(true);
        }
    }

    private void AtualizarBarraVida()
    {
        if (barraVidaFill == null)
        {
            return;
        }

        if (vidaMaxima <= 0f)
        {
            barraVidaFill.fillAmount = 0f;
            return;
        }

        // Atualiza fill da barra entre 0 e 1.
        barraVidaFill.fillAmount = vidaAtual / vidaMaxima;
    }

    private void ZerarAnimacaoMovimento()
    {
        // Faz o parametro Speed voltar suavemente para 0.
        velocidadeAnimacaoAtual = Mathf.Lerp(
            velocidadeAnimacaoAtual,
            0f,
            1f - Mathf.Exp(-Time.deltaTime / suavizacaoAnimator)
        );

        if (animator != null)
        {
            animator.SetFloat(parametroVelocidade, velocidadeAnimacaoAtual);
        }
    }

    public void ResetarJogoDoBotao()
    {
        // Reativa o Player.
        estaMorto = false;

        // Reseta vida.
        vidaAtual = vidaMaxima;
        AtualizarBarraVida();

        // Limpa movimento.
        velocidadeVertical = Vector3.zero;
        inputMovimento = Vector2.zero;
        inputCorrida = false;
        velocidadeAnimacaoAtual = 0f;

        // Esconde painel de morte.
        if (painelMorte != null)
        {
            painelMorte.SetActive(false);
        }

        // Desativa CharacterController antes de teleportar.
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        transform.position = posicaoInicialPlayer;
        transform.rotation = rotacaoInicialPlayer;

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        // Reseta o NPC.
        if (npcParaResetar != null)
        {
            NPC_IA_Trabalho npcIA = npcParaResetar.GetComponent<NPC_IA_Trabalho>();

            if (npcIA != null)
            {
                npcIA.ResetarNPC();
            }
            else
            {
                CharacterController npcController = npcParaResetar.GetComponent<CharacterController>();

                if (npcController != null)
                {
                    npcController.enabled = false;
                }

                npcParaResetar.position = posicaoInicialNPC;
                npcParaResetar.rotation = rotacaoInicialNPC;

                if (npcController != null)
                {
                    npcController.enabled = true;
                }
            }
        }

        // Reseta Animator do Player.
        if (animator != null)
        {
            animator.ResetTrigger(triggerDano);
            animator.ResetTrigger(triggerMorte);
            animator.SetFloat(parametroVelocidade, 0f);

            // O State precisa se chamar exatamente "Idle".
            animator.Play("Idle", 0, 0f);
        }

        AtualizarPainelVidaPersonagem();
    }

    [ContextMenu("Teste Receber 25 de Dano")]
    private void TesteReceberDano()
    {
        ReceberDano(25f);
    }
}