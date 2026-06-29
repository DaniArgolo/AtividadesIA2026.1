using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerMove : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidade = 5f;

    [Header("Rotacao (opcional)")]
    public bool rotacionarParaDirecao = true;
    public float velocidadeRotacao = 10f;

    private void Update()
    {
        Vector3 direcao = LerDirecaoMovimento();

        transform.Translate(direcao * velocidade * Time.deltaTime, Space.World);

        if (rotacionarParaDirecao && direcao.sqrMagnitude > 0.01f)
        {
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacaoAlvo,
                velocidadeRotacao * Time.deltaTime);
        }
    }

    private static Vector3 LerDirecaoMovimento()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return Vector3.zero;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            horizontal -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            horizontal += 1f;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            vertical -= 1f;
        }

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            vertical += 1f;
        }
#else
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
#endif

        Vector3 direcao = new Vector3(horizontal, 0f, vertical);
        return Vector3.ClampMagnitude(direcao, 1f);
    }
}
