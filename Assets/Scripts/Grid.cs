using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [Header("Values you can change")]
    [SerializeField, Tooltip("The size of the generated grid in the respective x and z dimensions. " +
                             "Use the plane in the scene to help with visualising where they would be in the grid. " +
                             "The plane to grid size ratio is 1:10, meaning that the scale (3, 1, 3) for the plane equals the grid size of 30 in both x and in y.")]
    public Vector2 gridWorldSize;
    [SerializeField, Tooltip("The radius of each node. This determines the size of each node.")]
    public float nodeRadius;
    [SerializeField, Tooltip("The bool which decides whether or not to generate a Game Object based grid upon game start.")]
    private bool generatePhysicalGrid;
    [SerializeField,Min(0) , Tooltip("This decides the time between each update for functions. On lower end specs, a higher Wait Time is recommended")]
    private float waitTime;
    [SerializeField, Tooltip("This is the multiplier that the obstacles apply to the G-Cost of nodes")] 
    private int obstaclePenalty;
    
    [Header("DO NOT CHANGE THESE VALUES")]
    [Tooltip("The layerMask that is checked when deciding which nodes should be flagged as walkable")]
    public LayerMask unwalkableMask;
    [Tooltip("The layerMask that is checked when deciding which nodes should apply the obstaclePenalty to their G-Costs")]
    public LayerMask obstacleMask;
    [SerializeField, Tooltip("The Game Object that is instantiated when generated a object-based grid.")]
    public GameObjectNodes nodeGameObjectPrefab;
    [SerializeField, Tooltip("The parent Game Object that each instantiated Game Object is created to be a child of.")]
    private GameObject nodeGameObjectParent;
    [SerializeField, Tooltip("The transforms of the seeker, which is our starting node, and the target, which is our ending node.")]
    private Transform seeker, target;

    private Node[,] grid;
    private GameObjectNodes[] physicalGrid;
    public List<Node> path;
    public HashSet<Node> searchedSet;

    
    private float nodeDiameter;
    private int gridSizeX;
    private int gridSizeY;
    private float timer;

    public Transform Seeker => seeker;
    public Transform Target => target;
    public float Timer => timer;
    public float WaitTime => waitTime;
    public int ObstaclePenalty => obstaclePenalty;
    
    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        nodeGameObjectPrefab.gameObject.transform.localScale = new Vector3(nodeDiameter,nodeDiameter,nodeDiameter);
        
        //Here we are setting the X- and Y Sizes which is the amount of nodes horizontally and vertically respectively.
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        
        StartCoroutine(Clock());

        CreateGrid();
    }

    private Vector3 worldBottomLeft;
    
    void CreateGrid()
    {
        //We begin everything creating a new reference for grid which is a 2-dimensional array.
        //Using the amount of nodes horizontally for the first dimension and the amount of nodes vertically for the second.
        grid = new Node[gridSizeX, gridSizeY];
        physicalGrid = new GameObjectNodes[grid.Length];
        
        //Get the bottom-left corner of the grid so that we can access the first node in the grid array.
        worldBottomLeft =
            transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                //Create a world position for the node using the bottom left corner.
                //Add by 1 to the right (1, 0, 0) multiplied by the x integer in the for-loop multiplied with the nodeDiamater added with the node radius.
                //^- this is effectively ((x * nodeDiameter + nodeRadius), 0, 0).
                //Once again add, this time by 1 forward (0, 0, 1) multiplied by the same factors as before.
                //^- this gives us (0, 0, (y * nodeDiameter + nodeRadius)).
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) +
                                     Vector3.forward * (y * nodeDiameter + nodeRadius);
                
                //Make a CheckSphere function which checks for if it finds either an object or an object with the specified layerMask.
                //It does this by: (The position it checks at, The radius of the sphere that checks for an object, (optional) The specific layerMask it is seeing if it finds.)
                //One is opposite of whatever the function returns because we want to be walkable when we DON'T find anything.
                //The other is not the opposite because we WANT to be an obstacle if we do find something.
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                bool obstacle = Physics.CheckSphere(worldPoint, nodeRadius, obstacleMask);

                //Create a new reference for each element in the grid array.
                grid[x, y] = new Node(walkable, obstacle, worldPoint, x, y);
            }
        }

        //If we don't want a Game Object based grid then do not go past this point.
        if (!generatePhysicalGrid)
        {
            return;
        }
        
        //Have a numeric value which we increment to access the relevant elements within the physicalGrid array.
        int num = 0;
        foreach (Node nodeInGrid in grid)
        {
            //Increment the integer so on the next loop we can access a different index within the physicalGrid array
            //We are doing it this way because: assume there are 900 elements within the array. The indices range from 0-899 since the first element has the 0 index. 
            if (num < physicalGrid.Length - 1)
            {
                num += 1;
            }
            //Instantiate a Game Object with the script: GameObjectNodes, that won't be rotated at the nodeInGrid's Position in the world as a child to the nodeGameObjectParent Game Object
            GameObjectNodes node = Instantiate(nodeGameObjectPrefab, nodeInGrid.worldPosition, Quaternion.identity, nodeGameObjectParent.transform); 
            //Make every node the base color (white) since we have yet to run the actual pathfinding algorithm.
            node.ChangeColor(0); 
            physicalGrid[num] = node;
        }
    }

    private void Update()
    {
        CheckForWalkability();
    }

    void CheckForWalkability()
    {
        if (timer >= waitTime)
        {
            //This is the same as in the CreateGrid() function.
            //The difference is that instead of creating new references we are actively updating the existing references. 
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) +
                                 Vector3.forward * (y * nodeDiameter + nodeRadius);
                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                    bool obstacle = Physics.CheckSphere(worldPoint, nodeRadius, obstacleMask);
                    
                    grid[x,y].walkable = walkable;
                    grid[x,y].obstacle = obstacle;
                }
            }
            timer = 0;
            ChangeColorsAndTexts();
            StartCoroutine(Clock());
        }
    }
    
    //This function returns a list of neighbouring nodes using the (Node node) parameter as the base we are checking from.
    public List<Node> GetNeighbours(Node node)
    {
        //Since we are not storing the neighbours for each node in this model, we must find them.
        //Begin with making a new local list of neighbours.
        List<Node> neighbours = new List<Node>();

        //We are only going between -1 and 1 because: assume that each increment of x and y are a step in that direction.
        //So to begin with where both are equal to -1 we are to the left of the node we are checking while also below it as well. Meaning we are diagonally down and to the left. 
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                //If we are on the nodes current position, meaning x and y are both equal to 0 or if we are on any of the diagonals meaning that neither are equal to 0 (since this model focuses only on adjacent nodes)
                if (x == 0 && y == 0 || x != 0 && y != 0) continue;

                //We create integers that are the index positions of the corresponding neighbouring nodes.
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;
                
                //Ensure that neither CheckX nor CheckY are outside of the grid.
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    //If so we add them to the local neighbours list.
                    neighbours.Add(grid[checkX,checkY]);
                }
            }
        }
        
        return neighbours;
    }
    
    //Using a Vector3, meaning a position in the world, find the corresponding node in the grid array by converting the position to integers and using them to access the node's index.
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }
    
    //Change the colors of all the nodes and make sure that their text components show their costs.
    public void ChangeColorsAndTexts()
    {
        int num = 0;

        if (grid != null && generatePhysicalGrid)
        {
            foreach(Node n in grid)
            {
                if (num < physicalGrid.Length - 1)
                {
                    num += 1;
                }
                
                physicalGrid[num].ChangeColor((n.walkable) ? ColorState.NonSearchedColorState : ColorState.BlockadeColorState);
                physicalGrid[num].ChangeText(n.gCost, n.hCost, n.fCost);
                
                if (searchedSet != null)
                {
                    if (searchedSet.Contains(n))
                    {
                        physicalGrid[num].ChangeColor(ColorState.SearchedColorState);
                        physicalGrid[num].ChangeText(n.gCost, n.hCost, n.fCost);
                    }
                }
                if (path != null)
                {
                    if (path.Contains(n))
                    {
                        physicalGrid[num].ChangeColor(ColorState.PathColorState);
                        physicalGrid[num].ChangeText(n.gCost, n.hCost, n.fCost);
                    }
                }
                if (n.obstacle && n.walkable)
                {
                    physicalGrid[num].ChangeColor(ColorState.ObstacleColorState);
                }
                //If a node is neither searched through nor is part of the path, set its costs to 0.
                if (searchedSet != null && path != null)
                {
                    if (!searchedSet.Contains(n) && !path.Contains(n))
                    {
                        n.gCost = 0;
                        n.hCost = 0;
                    }
                }
            }
        }
    }
    
    //This does the same as the ChangeColorsAndTexts() function except for that it does not change the texts as this only changes the color for the drawn Gizmos.
    //This function also draws a WireCube that shows the size of the grid in the x and z dimension. 
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            foreach(Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                if (searchedSet != null)
                {
                    if (searchedSet.Contains(n))
                    {
                        Gizmos.color = Color.orange;
                        
                    }
                }
                if (path != null)
                {
                    if (path.Contains(n))
                    {
                        Gizmos.color = Color.black;
                    }
                }
                if (n.obstacle && n.walkable)
                { 
                    Gizmos.color = Color.blue;
                }
                
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));
            }
        }
    }
    
    //A clock that ensures that we can control the pace of the updates. Although not necessary, useful for lower end specs that want to use this model.
    IEnumerator Clock()
    {
        yield return new WaitForSecondsRealtime(waitTime);
        timer = waitTime;
    }
}
