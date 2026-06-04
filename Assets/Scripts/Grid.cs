using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Grid : MonoBehaviour
{
    //ADD THE TEXT FOR THE G-COST AND H-COST AND F-COST
    
    [Header("Values you can change")]
    public Vector2 gridWorldSize;
    public float nodeRadius;
    [SerializeField] private bool generatePhysicalGrid;
    [SerializeField] private float timer;
    [SerializeField] private float waitTime;
    [SerializeField] private int obstaclePenalty;
    
    [Header("DO NOT CHANGE THESE VALUES")]
    public LayerMask unwalkableMask;
    public LayerMask obstacleMask;
    [SerializeField] public PhysicalNodes nonSearchedCube;
    [SerializeField] private GameObject searchedCube;
    [SerializeField] private GameObject pathCube;
    [SerializeField] private GameObject cubeParent;
    [SerializeField] private Transform seeker, target;

    private Node[,] grid;
    private PhysicalNodes[] physicalGrid;
    public List<Node> path;
    public HashSet<Node> searchedSet;
    
    private float nodeDiameter;
    private int gridSizeX;
    private int gridSizeY;

    public Transform Seeker => seeker;
    public Transform Target => target;
    public float Timer => timer;
    public float WaitTime => waitTime;
    public int ObstaclePenalty => obstaclePenalty;
    
    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        nonSearchedCube.gameObject.transform.localScale = new Vector3(nodeDiameter,nodeDiameter,nodeDiameter);
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        StartCoroutine(Clock());

        CreateGrid();
    }

    private Vector3 worldPoint;
    private Vector3 worldBottomLeft;
    
    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        physicalGrid = new PhysicalNodes[grid.Length];
        worldBottomLeft =
            transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        int num = 0;
        
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) +
                                     Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                bool obstacle = Physics.CheckSphere(worldPoint, nodeRadius, obstacleMask);

                grid[x, y] = new Node(walkable, obstacle, worldPoint, x, y);
            }
        }
        
        foreach (Node n in grid)
        {
            if (num < physicalGrid.Length - 1)
            {
                num += 1;
            }
            
            PhysicalNodes node = Instantiate(nonSearchedCube, n.worldPosition, Quaternion.identity, cubeParent.transform); 
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
        if (timer >= waitTime * 10)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) +
                                 Vector3.forward * (y * nodeDiameter + nodeRadius);
                    bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                    bool obstacle = Physics.CheckSphere(worldPoint, nodeRadius, obstacleMask);

                    grid[x,y].walkable = walkable;
                    grid[x,y].obstacle = obstacle;
                }
            }
            timer = 0;
            ChangeColors();
            StartCoroutine(Clock());
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0 || x != 0 && y != 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX,checkY]);
                }
            }
        }

        return neighbours;
    }

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
    
    public void ChangeColors()
    {
        int num = 0;

        if (grid != null)
        {
            foreach(Node n in grid)
            {
                if (num < physicalGrid.Length - 1)
                {
                    num += 1;
                }
                physicalGrid[num].ChangeColor((n.walkable) ? PhysicalNodes.WHITENUM : PhysicalNodes.REDNUM);
                physicalGrid[num].ChangeText(n.gCost, n.hCost, n.fCost);
                if (searchedSet != null)
                {
                    if (searchedSet.Contains(n))
                    {
                        physicalGrid[num].ChangeColor(PhysicalNodes.ORANGENUM);
                        physicalGrid[num].ChangeText(n.gCost, n.hCost, n.fCost);
                    }
                }
                if (path != null)
                {
                    if (path.Contains(n))
                    {
                        physicalGrid[num].ChangeColor(PhysicalNodes.BLACKNUM);
                        physicalGrid[num].ChangeText(n.gCost, n.hCost, n.fCost);
                    }
                }
                if (n.obstacle && n.walkable)
                {
                    physicalGrid[num].ChangeColor(PhysicalNodes.BLUENUM);
                }
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
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

        if (grid != null)
        {
            foreach(Node n in grid)
            {
                Gizmos.color = (n.obstacle) ?  Color.blue : Color.white;
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
                
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));
            }
        }
    }
    
    IEnumerator Clock()
    {
        yield return new WaitForSecondsRealtime(waitTime);
        timer = waitTime*100;
    }
}
