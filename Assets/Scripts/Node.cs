using UnityEngine;

public class Node
{
    public bool walkable;
    public bool obstacle;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    
    public int gCost;
    public int hCost;
    public Node parent;

    public Node(bool _walkable, bool _obstacle, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        obstacle = _obstacle;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }

    public int fCost => gCost + hCost;

}
