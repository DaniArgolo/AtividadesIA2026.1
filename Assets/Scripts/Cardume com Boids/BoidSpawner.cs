using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [Header("Configuracao de spawn")]
    public GameObject peixePrefab;
    public int quantidade = 100;
    public float raioSpawn = 10f;

    [Header("Container (opcional)")]
    [Tooltip("Mantem o Hierarchy organizado.")]
    public Transform parent;

    private void Start()
    {
        if (peixePrefab == null)
        {
            Debug.LogError("BoidSpawner: peixePrefab nao atribuido!", this);
            return;
        }

        for (int i = 0; i < quantidade; i++)
        {
            Vector3 posicao = transform.position + Random.insideUnitSphere * raioSpawn;
            Quaternion rotacao = Random.rotation;
            GameObject peixe = Instantiate(peixePrefab, posicao, rotacao);

            if (parent != null)
            {
                peixe.transform.SetParent(parent);
            }

            Boid boid = peixe.GetComponent<Boid>();

            if (boid != null)
            {
                boid.centroMundo = transform.position;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.55f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, raioSpawn);
    }
}
