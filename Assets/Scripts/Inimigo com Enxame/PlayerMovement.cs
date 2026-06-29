using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace InimigoComEnxame
{
    public class PlayerMovement : MonoBehaviour
    {
        public float velocidade = 6f;
        public float limiteArena = 17f;
        public bool limitarNaArena = true;
        public bool rotacionarParaDirecao = true;
        public float velocidadeRotacao = 10f;

        private void Update()
        {
            Vector3 direcao = LerDirecaoMovimento();
            transform.position += direcao * velocidade * Time.deltaTime;

            if (limitarNaArena)
            {
                Vector3 pos = transform.position;
                pos.x = Mathf.Clamp(pos.x, -limiteArena, limiteArena);
                pos.z = Mathf.Clamp(pos.z, -limiteArena, limiteArena);
                transform.position = pos;
            }

            if (rotacionarParaDirecao && direcao.sqrMagnitude > 0.001f)
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

            return Vector3.ClampMagnitude(new Vector3(horizontal, 0f, vertical), 1f);
        }
    }
}
