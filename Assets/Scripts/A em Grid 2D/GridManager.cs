using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Dimensoes")]
    public int width = 20;
    public int height = 20;
    public float cellSize = 1f;

    public Node[,] grid;

    [Header("Visualizacao (Gizmos)")]
    public Color corChao = new Color(0.1f, 0.12f, 0.18f);
    public Color corParede = new Color(0.3f, 0.32f, 0.42f);
    public Color corInicio = new Color(0.41f, 0.94f, 0.68f);
    public Color corDestino = new Color(1f, 0.25f, 0.5f);
    public Color corCaminho = new Color(0f, 0.9f, 1f);

    [Header("Visualizacao em Jogo")]
    public bool criarVisualizacaoRuntime = true;
    public float profundidadeTile = 0.05f;

    [HideInInspector] public Node nodeInicio;
    [HideInInspector] public Node nodeDestino;
    [HideInInspector] public List<Node> caminhoAtual;

    private int larguraGerada;
    private int alturaGerada;
    private float tamanhoCelulaGerado;
    private Renderer[,] renderizadores;
    private Material materialChao;
    private Material materialParede;
    private Material materialInicio;
    private Material materialDestino;
    private Material materialCaminho;
    private Transform visualizacaoRuntime;

    private void Awake()
    {
        GerarGrid();
    }

    private void Start()
    {
        CriarVisualizacaoRuntime();
        AtualizarVisualizacao();
    }

    private void OnValidate()
    {
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        cellSize = Mathf.Max(0.1f, cellSize);
    }

    private void GerarGrid()
    {
        grid = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPosition = new Vector3(x * cellSize, y * cellSize, 0f);
                grid[x, y] = new Node(x, y, worldPosition);
            }
        }

        larguraGerada = width;
        alturaGerada = height;
        tamanhoCelulaGerado = cellSize;
        nodeInicio = null;
        nodeDestino = null;
        caminhoAtual = null;
        renderizadores = null;
    }

    private void GarantirGridGerado()
    {
        if (grid == null ||
            larguraGerada != width ||
            alturaGerada != height ||
            !Mathf.Approximately(tamanhoCelulaGerado, cellSize))
        {
            GerarGrid();

            if (Application.isPlaying)
            {
                CriarVisualizacaoRuntime();
                AtualizarVisualizacao();
            }
        }
    }

    public Node NodeNaPosicao(Vector3 worldPosition)
    {
        GarantirGridGerado();

        int x = Mathf.RoundToInt(worldPosition.x / cellSize);
        int y = Mathf.RoundToInt(worldPosition.y / cellSize);

        x = Mathf.Clamp(x, 0, width - 1);
        y = Mathf.Clamp(y, 0, height - 1);

        return grid[x, y];
    }

    public List<Node> GetVizinhos(Node node)
    {
        GarantirGridGerado();

        List<Node> vizinhos = new List<Node>();
        int[] dx = { 0, 0, 1, -1 };
        int[] dy = { 1, -1, 0, 0 };

        for (int i = 0; i < dx.Length; i++)
        {
            int nx = node.gridPosition.x + dx[i];
            int ny = node.gridPosition.y + dy[i];

            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
            {
                vizinhos.Add(grid[nx, ny]);
            }
        }

        return vizinhos;
    }

    public void ResetCustos()
    {
        GarantirGridGerado();

        foreach (Node node in grid)
        {
            node.gCost = 0;
            node.hCost = 0;
            node.parent = null;
        }
    }

    public void ResetCompleto()
    {
        GarantirGridGerado();

        foreach (Node node in grid)
        {
            node.walkable = true;
            node.gCost = 0;
            node.hCost = 0;
            node.parent = null;
        }

        nodeInicio = null;
        nodeDestino = null;
        caminhoAtual = null;

        AtualizarVisualizacao();
    }

    public void AtualizarVisualizacao()
    {
        if (!Application.isPlaying || renderizadores == null)
        {
            return;
        }

        foreach (Node node in grid)
        {
            renderizadores[node.gridPosition.x, node.gridPosition.y].sharedMaterial =
                node.walkable ? materialChao : materialParede;
        }

        if (caminhoAtual != null)
        {
            foreach (Node node in caminhoAtual)
            {
                if (node != nodeInicio && node != nodeDestino)
                {
                    renderizadores[node.gridPosition.x, node.gridPosition.y].sharedMaterial = materialCaminho;
                }
            }
        }

        if (nodeInicio != null)
        {
            renderizadores[nodeInicio.gridPosition.x, nodeInicio.gridPosition.y].sharedMaterial = materialInicio;
        }

        if (nodeDestino != null)
        {
            renderizadores[nodeDestino.gridPosition.x, nodeDestino.gridPosition.y].sharedMaterial = materialDestino;
        }
    }

    private void CriarVisualizacaoRuntime()
    {
        if (!criarVisualizacaoRuntime || !Application.isPlaying)
        {
            return;
        }

        if (visualizacaoRuntime != null)
        {
            Destroy(visualizacaoRuntime.gameObject);
        }

        CriarMateriaisRuntime();

        GameObject raiz = new GameObject("Grid Runtime Visual");
        raiz.transform.SetParent(transform);
        visualizacaoRuntime = raiz.transform;
        renderizadores = new Renderer[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Node node = grid[x, y];
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Tile {x},{y}";
                tile.transform.SetParent(visualizacaoRuntime);
                tile.transform.position = node.worldPosition;
                tile.transform.localScale = new Vector3(
                    cellSize * 0.9f,
                    cellSize * 0.9f,
                    profundidadeTile);

                Collider colliderTile = tile.GetComponent<Collider>();

                if (colliderTile != null)
                {
                    Destroy(colliderTile);
                }

                Renderer rendererTile = tile.GetComponent<Renderer>();
                rendererTile.sharedMaterial = materialChao;
                renderizadores[x, y] = rendererTile;
            }
        }
    }

    private void CriarMateriaisRuntime()
    {
        materialChao = CriarMaterialRuntime("A* Chao", corChao);
        materialParede = CriarMaterialRuntime("A* Parede", corParede);
        materialInicio = CriarMaterialRuntime("A* Inicio", corInicio);
        materialDestino = CriarMaterialRuntime("A* Destino", corDestino);
        materialCaminho = CriarMaterialRuntime("A* Caminho", corCaminho);
    }

    private Material CriarMaterialRuntime(string nome, Color cor)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader)
        {
            name = nome,
            color = cor,
            hideFlags = HideFlags.DontSave
        };

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", cor);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", cor);
        }

        return material;
    }

    private void OnDrawGizmos()
    {
        GarantirGridGerado();

        foreach (Node node in grid)
        {
            Gizmos.color = node.walkable ? corChao : corParede;
            Gizmos.DrawCube(node.worldPosition, Vector3.one * (cellSize * 0.9f));
        }

        if (nodeInicio != null)
        {
            Gizmos.color = corInicio;
            Gizmos.DrawCube(nodeInicio.worldPosition, Vector3.one * cellSize);
        }

        if (nodeDestino != null)
        {
            Gizmos.color = corDestino;
            Gizmos.DrawCube(nodeDestino.worldPosition, Vector3.one * cellSize);
        }

        if (caminhoAtual == null)
        {
            return;
        }

        Gizmos.color = corCaminho;

        foreach (Node node in caminhoAtual)
        {
            if (node != nodeInicio && node != nodeDestino)
            {
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (cellSize * 0.6f));
            }
        }
    }
}
