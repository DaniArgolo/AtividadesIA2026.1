using System.Collections.Generic;
using UnityEngine;

namespace InimigoComEnxame
{
    public class BoidAgent : MonoBehaviour
    {
        [Header("Percepcao")]
        public float raioVizinhanca = 3f;
        public float raioSeparacao = 1.2f;

        [Header("Pesos das forcas")]
        [Min(0f)] public float pesoSeparacao = 1.5f;
        [Min(0f)] public float pesoAlinhamento = 1f;
        [Min(0f)] public float pesoCoesao = 1f;
        [Min(0f)] public float pesoAlvo = 1f;

        [Header("Movimento")]
        public float velocidadeMax = 4f;
        public float velocidadeInicial = 2f;
        public float limiteMundo = 18f;
        public float pesoLimite = 1f;
        public bool manterNoPlanoXZ = true;

        [Header("Orbita (bonus)")]
        public bool usarOrbita = false;
        public float distanciaOrbita = 4f;
        [Range(0f, 2f)] public float pesoTangencialOrbita = 0.65f;
        public bool sentidoHorario = true;

        [Header("Agrupamento")]
        public int grupo = 0;

        [HideInInspector] public Transform alvo;
        [HideInInspector] public Vector3 centroMundo = Vector3.zero;

        private Vector3 velocidade;
        private float alturaInicial;
        private static readonly List<BoidAgent> Todos = new List<BoidAgent>();

        private void OnEnable()
        {
            if (!Todos.Contains(this))
            {
                Todos.Add(this);
            }
        }

        private void OnDisable()
        {
            Todos.Remove(this);
        }

        private void Start()
        {
            alturaInicial = transform.position.y;

            Vector3 direcaoInicial = Random.insideUnitSphere;

            if (manterNoPlanoXZ)
            {
                direcaoInicial.y = 0f;
            }

            if (direcaoInicial.sqrMagnitude < 0.001f)
            {
                direcaoInicial = transform.forward;
            }

            velocidade = direcaoInicial.normalized * velocidadeInicial;
        }

        private void Update()
        {
            List<BoidAgent> vizinhos = GetNeighbors();

            Vector3 forca = Vector3.zero;
            forca += Separation(vizinhos) * pesoSeparacao;
            forca += Alignment(vizinhos) * pesoAlinhamento;
            forca += Cohesion(vizinhos) * pesoCoesao;
            forca += SeekTarget() * pesoAlvo;
            forca += KeepInsideWorld() * pesoLimite;

            forca = Vector3.ClampMagnitude(forca, velocidadeMax);
            velocidade += forca * Time.deltaTime;

            if (manterNoPlanoXZ)
            {
                velocidade.y = 0f;
            }

            velocidade = Vector3.ClampMagnitude(velocidade, velocidadeMax);

            Vector3 novaPosicao = transform.position + velocidade * Time.deltaTime;

            if (manterNoPlanoXZ)
            {
                novaPosicao.y = alturaInicial;
            }

            transform.position = novaPosicao;

            if (velocidade.sqrMagnitude > 0.001f)
            {
                transform.forward = velocidade.normalized;
            }
        }

        // Parte 1: percepcao local.
        private List<BoidAgent> GetNeighbors()
        {
            List<BoidAgent> vizinhos = new List<BoidAgent>();
            float raioQuadrado = raioVizinhanca * raioVizinhanca;

            foreach (BoidAgent agente in Todos)
            {
                if (agente == this || agente.grupo != grupo)
                {
                    continue;
                }

                Vector3 deslocamento = agente.transform.position - transform.position;

                if (manterNoPlanoXZ)
                {
                    deslocamento.y = 0f;
                }

                if (deslocamento.sqrMagnitude <= raioQuadrado)
                {
                    vizinhos.Add(agente);
                }
            }

            return vizinhos;
        }

        // Parte 2: evita colisoes locais. Quanto mais perto, mais forte.
        private Vector3 Separation(List<BoidAgent> vizinhos)
        {
            Vector3 resultado = Vector3.zero;

            foreach (BoidAgent vizinho in vizinhos)
            {
                Vector3 fuga = transform.position - vizinho.transform.position;

                if (manterNoPlanoXZ)
                {
                    fuga.y = 0f;
                }

                float distancia = fuga.magnitude;

                if (distancia > 0.001f && distancia < raioSeparacao)
                {
                    resultado += fuga.normalized / distancia;
                }
            }

            return resultado.sqrMagnitude > 0.001f ? resultado.normalized : Vector3.zero;
        }

        // Parte 2: tenta apontar na mesma direcao media dos vizinhos.
        private Vector3 Alignment(List<BoidAgent> vizinhos)
        {
            if (vizinhos.Count == 0)
            {
                return Vector3.zero;
            }

            Vector3 mediaDirecoes = Vector3.zero;

            foreach (BoidAgent vizinho in vizinhos)
            {
                mediaDirecoes += vizinho.transform.forward;
            }

            mediaDirecoes /= vizinhos.Count;

            if (manterNoPlanoXZ)
            {
                mediaDirecoes.y = 0f;
            }

            return mediaDirecoes.sqrMagnitude > 0.001f ? mediaDirecoes.normalized : Vector3.zero;
        }

        // Parte 2: aproxima o agente do centro de massa local.
        private Vector3 Cohesion(List<BoidAgent> vizinhos)
        {
            if (vizinhos.Count == 0)
            {
                return Vector3.zero;
            }

            Vector3 centroMassa = Vector3.zero;

            foreach (BoidAgent vizinho in vizinhos)
            {
                centroMassa += vizinho.transform.position;
            }

            centroMassa /= vizinhos.Count;

            Vector3 direcao = centroMassa - transform.position;

            if (manterNoPlanoXZ)
            {
                direcao.y = 0f;
            }

            return direcao.sqrMagnitude > 0.001f ? direcao.normalized : Vector3.zero;
        }

        // Parte 3 e bonus: busca o alvo; se orbita estiver ligada, afasta no raio minimo.
        private Vector3 SeekTarget()
        {
            if (alvo == null)
            {
                return Vector3.zero;
            }

            Vector3 ateAlvo = alvo.position - transform.position;

            if (manterNoPlanoXZ)
            {
                ateAlvo.y = 0f;
            }

            float distancia = ateAlvo.magnitude;

            if (distancia < 0.001f)
            {
                return Vector3.zero;
            }

            Vector3 direcao = ateAlvo / distancia;

            if (!usarOrbita)
            {
                return direcao;
            }

            Vector3 tangente = Vector3.Cross(Vector3.up, direcao).normalized;

            if (!sentidoHorario)
            {
                tangente = -tangente;
            }

            if (distancia < distanciaOrbita)
            {
                return (-direcao + tangente * pesoTangencialOrbita).normalized;
            }

            return (direcao + tangente * pesoTangencialOrbita).normalized;
        }

        private Vector3 KeepInsideWorld()
        {
            if (limiteMundo <= 0f)
            {
                return Vector3.zero;
            }

            Vector3 ateCentro = centroMundo - transform.position;

            if (manterNoPlanoXZ)
            {
                ateCentro.y = 0f;
            }

            return ateCentro.magnitude > limiteMundo ? ateCentro.normalized : Vector3.zero;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, raioVizinhanca);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, raioSeparacao);

            if (usarOrbita)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, distanciaOrbita);
            }
        }
    }
}
