using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class GridInput : MonoBehaviour
{
    private GridManager gridManager;
    private AStarPathfinding pathfinding;
    private Camera cam;

    private void Awake()
    {
        gridManager = GetComponent<GridManager>();
        pathfinding = GetComponent<AStarPathfinding>();
        cam = Camera.main;
    }

    private void Update()
    {
        if (gridManager == null || pathfinding == null)
        {
            return;
        }

        if (cam == null)
        {
            cam = Camera.main;

            if (cam == null)
            {
                return;
            }
        }

        if (ResetPressionado())
        {
            gridManager.ResetCompleto();
            return;
        }

        Vector3 mundo = cam.ScreenToWorldPoint(PosicaoMouseNaTela());
        mundo.z = 0f;

        Node nodeClicado = gridManager.NodeNaPosicao(mundo);

        if (nodeClicado == null)
        {
            return;
        }

        if (BotaoMousePressionado(0))
        {
            if (nodeClicado.walkable)
            {
                gridManager.nodeInicio = nodeClicado;
                RecalcularCaminho();
            }
        }
        else if (BotaoMousePressionado(1))
        {
            if (nodeClicado.walkable)
            {
                gridManager.nodeDestino = nodeClicado;
                RecalcularCaminho();
            }
        }
        else if (BotaoMousePressionado(2))
        {
            if (nodeClicado != gridManager.nodeInicio &&
                nodeClicado != gridManager.nodeDestino)
            {
                nodeClicado.walkable = !nodeClicado.walkable;
                RecalcularCaminho();
            }
        }
    }

    private void RecalcularCaminho()
    {
        if (gridManager.nodeInicio == null || gridManager.nodeDestino == null)
        {
            gridManager.caminhoAtual = null;
            gridManager.AtualizarVisualizacao();
            return;
        }

        gridManager.caminhoAtual = pathfinding.EncontrarCaminho(
            gridManager.nodeInicio,
            gridManager.nodeDestino);

        gridManager.AtualizarVisualizacao();
    }

    private static bool ResetPressionado()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }

    private static bool BotaoMousePressionado(int botao)
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;

        if (mouse == null)
        {
            return false;
        }

        return botao switch
        {
            0 => mouse.leftButton.wasPressedThisFrame,
            1 => mouse.rightButton.wasPressedThisFrame,
            2 => mouse.middleButton.wasPressedThisFrame,
            _ => false
        };
#else
        return Input.GetMouseButtonDown(botao);
#endif
    }

    private static Vector3 PosicaoMouseNaTela()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null)
        {
            return Vector3.zero;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        return new Vector3(mousePosition.x, mousePosition.y, 0f);
#else
        return Input.mousePosition;
#endif
    }
}
