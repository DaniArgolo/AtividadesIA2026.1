using UnityEngine;

namespace InimigoComEnxame
{
    public class BoidSpawner : MonoBehaviour
    {
        [Header("Configuracao do enxame")]
        public BoidAgent boidPrefab;
        public int quantidade = 20;
        public float raioDeSpawn = 6f;
        public Transform alvo;

        [Header("Organizacao")]
        public Transform parent;
        public int grupo = 0;
        public bool manterNoPlanoXZ = true;

        private void Start()
        {
            if (boidPrefab == null)
            {
                Debug.LogError("BoidSpawner: boidPrefab nao atribuido.", this);
                return;
            }

            for (int i = 0; i < quantidade; i++)
            {
                Vector3 pos = transform.position + Random.insideUnitSphere * raioDeSpawn;

                if (manterNoPlanoXZ)
                {
                    pos.y = transform.position.y;
                }

                Quaternion rotacao = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                BoidAgent boid = Instantiate(boidPrefab, pos, rotacao);
                boid.alvo = alvo;
                boid.centroMundo = transform.position;
                boid.grupo = grupo;
                boid.manterNoPlanoXZ = manterNoPlanoXZ;

                if (parent != null)
                {
                    boid.transform.SetParent(parent);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.55f, 0f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, raioDeSpawn);
        }
    }
}
