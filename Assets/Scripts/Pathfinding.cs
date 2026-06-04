using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    private Transform seeker, target;
    
    private Grid grid;
    private Vector3 oldSeekerPos;
    private float timer; 
    private float waitTime;

    private void Awake()
    {
        grid = GetComponent<Grid>();
        seeker = grid.Seeker;
        target = grid.Target;
        timer = grid.Timer;
        waitTime = grid.WaitTime;
        oldSeekerPos.x = seeker.position.x + 1;
    }

    private void Update()
    {
        FindPath(seeker.position, target.position);
        if (oldSeekerPos != seeker.position && timer >= waitTime)
        {
            Debug.Log("AAA");
            grid.ChangeColors();
            oldSeekerPos = seeker.position;
            timer = 0;
            StartCoroutine(Clock());
        }
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        HashSet<Node> searchedSet = new HashSet<Node>();
        
        startNode.gCost = 0;
        
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || 
                    openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }
            
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                grid.searchedSet = searchedSet;
                return;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                        
                    }
                }
                if (newMovementCostToNeighbour == neighbour.gCost || newMovementCostToNeighbour > neighbour.gCost)
                {
                    if (!searchedSet.Contains(neighbour))
                    {
                        searchedSet.Add(neighbour);
                    }
                }
            }
        }
    }

    void RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        grid.path = path;
    }
    
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (nodeA.obstacle || nodeB.obstacle)
        {
            dstX *= grid.ObstaclePenalty;
            dstY *= grid.ObstaclePenalty;
        }
        
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }

    IEnumerator Clock()
    {
        yield return new WaitForSecondsRealtime(waitTime);
        timer += waitTime;
    }
}