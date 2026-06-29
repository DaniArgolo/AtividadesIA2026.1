using System.Collections.Generic;
using UnityEngine;

public class AStarPathfinding : MonoBehaviour
{
    public GridManager gridManager;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = GetComponent<GridManager>();
        }
    }

    public List<Node> EncontrarCaminho(Node inicio, Node destino)
    {
        if (inicio == null || destino == null || gridManager == null)
        {
            return null;
        }

        if (!inicio.walkable || !destino.walkable)
        {
            return null;
        }

        gridManager.ResetCustos();

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(inicio);

        while (openSet.Count > 0)
        {
            Node atual = openSet[0];

            for (int i = 1; i < openSet.Count; i++)
            {
                Node candidato = openSet[i];

                if (candidato.fCost < atual.fCost ||
                    candidato.fCost == atual.fCost && candidato.hCost < atual.hCost)
                {
                    atual = candidato;
                }
            }

            openSet.Remove(atual);
            closedSet.Add(atual);

            if (atual == destino)
            {
                return ReconstruirCaminho(inicio, destino);
            }

            foreach (Node vizinho in gridManager.GetVizinhos(atual))
            {
                if (!vizinho.walkable || closedSet.Contains(vizinho))
                {
                    continue;
                }

                int novoG = atual.gCost + Distancia(atual, vizinho);

                if (novoG < vizinho.gCost || !openSet.Contains(vizinho))
                {
                    vizinho.gCost = novoG;
                    vizinho.hCost = Distancia(vizinho, destino);
                    vizinho.parent = atual;

                    if (!openSet.Contains(vizinho))
                    {
                        openSet.Add(vizinho);
                    }
                }
            }
        }

        return null;
    }

    private int Distancia(Node a, Node b)
    {
        int dx = Mathf.Abs(a.gridPosition.x - b.gridPosition.x);
        int dy = Mathf.Abs(a.gridPosition.y - b.gridPosition.y);

        return dx + dy;
    }

    private List<Node> ReconstruirCaminho(Node inicio, Node destino)
    {
        List<Node> caminho = new List<Node>();
        Node atual = destino;

        while (atual != inicio)
        {
            caminho.Add(atual);
            atual = atual.parent;

            if (atual == null)
            {
                return null;
            }
        }

        caminho.Add(inicio);
        caminho.Reverse();

        return caminho;
    }
}
