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

    //Make a constructor so that we can assign these values when creating references for our arrays within grid. 
    public Node(bool _walkable, bool _obstacle, Vector3 _worldPos, int _gridX, int _gridY)
    {
        walkable = _walkable;
        obstacle = _obstacle;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
    }

    //Since the F-Cost is the sum of the G-Cost and H-Cost this shouldn't be settable and instead should only be gettable.
    public int fCost => gCost + hCost;

}
