using UnityEngine;

public class Node
{
    public Vector2Int gridPosition;
    public Vector3 worldPosition;
    public bool walkable = true;

    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;

    public Node parent;

    public Node(int x, int y, Vector3 worldPosition)
    {
        gridPosition = new Vector2Int(x, y);
        this.worldPosition = worldPosition;
    }
}
