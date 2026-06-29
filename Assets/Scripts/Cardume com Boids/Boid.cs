using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Movimento")]
    public float velocidadeMin = 2f;
    public float velocidadeMax = 5f;

    [Header("Percepcao")]
    public float raioPercepcao = 3f;
    public float raioSeparacao = 1.5f;

    [Header("Pesos das regras")]
    [Range(0f, 5f)] public float pesoSeparacao = 1.5f;
    [Range(0f, 5f)] public float pesoAlinhamento = 1f;
    [Range(0f, 5f)] public float pesoCoesao = 1f;
    [Range(0f, 5f)] public float pesoLimite = 2f;

    [Header("Limites do mundo")]
    public Vector3 centroMundo = Vector3.zero;
    public float raioMundo = 15f;

    [HideInInspector] public Vector3 velocidade;

    private void Start()
    {
        velocidade = Random.insideUnitSphere * ((velocidadeMin + velocidadeMax) * 0.5f);

        if (velocidade.sqrMagnitude < 0.01f)
        {
            velocidade = Random.onUnitSphere * velocidadeMin;
        }
    }

    private void Update()
    {
        Vector3 separacao = Vector3.zero;
        Vector3 alinhamento = Vector3.zero;
        Vector3 coesao = Vector3.zero;

        int vizinhosPercebidos = 0;
        int vizinhosMuitoProximos = 0;

        Collider[] vizinhos = Physics.OverlapSphere(
            transform.position,
            raioPercepcao,
            ~0,
            QueryTriggerInteraction.Collide);

        foreach (Collider col in vizinhos)
        {
            if (col.gameObject == gameObject)
            {
                continue;
            }

            Boid outro = col.GetComponent<Boid>();

            if (outro == null)
            {
                continue;
            }

            Vector3 deltaPos = outro.transform.position - transform.position;
            float distancia = deltaPos.magnitude;

            if (distancia < raioSeparacao)
            {
                separacao -= deltaPos.normalized / Mathf.Max(distancia, 0.001f);
                vizinhosMuitoProximos++;
            }

            alinhamento += outro.velocidade;
            coesao += outro.transform.position;
            vizinhosPercebidos++;
        }

        if (vizinhosPercebidos > 0)
        {
            alinhamento /= vizinhosPercebidos;
            alinhamento = alinhamento.normalized * velocidadeMax;

            coesao /= vizinhosPercebidos;
            coesao = (coesao - transform.position).normalized * velocidadeMax;
        }

        if (vizinhosMuitoProximos > 0)
        {
            separacao = separacao.normalized * velocidadeMax;
        }

        Vector3 limite = Vector3.zero;
        float distanciaCentro = Vector3.Distance(transform.position, centroMundo);

        if (distanciaCentro > raioMundo)
        {
            limite = (centroMundo - transform.position).normalized * velocidadeMax;
        }

        Vector3 aceleracao =
            separacao * pesoSeparacao +
            alinhamento * pesoAlinhamento +
            coesao * pesoCoesao +
            limite * pesoLimite;

        velocidade += aceleracao * Time.deltaTime;

        float speed = velocidade.magnitude;

        if (speed > velocidadeMax)
        {
            velocidade = velocidade.normalized * velocidadeMax;
        }
        else if (speed < velocidadeMin && speed > 0.001f)
        {
            velocidade = velocidade.normalized * velocidadeMin;
        }

        transform.position += velocidade * Time.deltaTime;

        if (velocidade.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(velocidade),
                5f * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.9f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, raioPercepcao);

        Gizmos.color = new Color(1f, 0.25f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, raioSeparacao);
    }
}
