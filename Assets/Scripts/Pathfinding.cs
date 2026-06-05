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
        //Although most of these would make sense to be set inside of this class. For ease of use in the inspector, this is moved to the Grid class. This way everything is gathered in one spot.
        seeker = grid.Seeker;
        target = grid.Target;
        waitTime = grid.WaitTime;
        timer = waitTime;
        
        //Make sure that the oldSeekerPos will never be equal to the seekers current position upon game start so that a path is constructed upon the first possible frame.
        oldSeekerPos.x = seeker.position.x + 1;
    }

    private void Update()
    {
        //Since we wouldn't want to run things unnecessarily if nothing is moving or changing we check if the seeker, our starting node, has moved and if enough time has passed.
        if (oldSeekerPos != seeker.position && timer >= waitTime)
        {
            FindPath(seeker.position, target.position);
            grid.ChangeColorsAndTexts();
            oldSeekerPos = seeker.position;
            timer = 0;
            StartCoroutine(Clock());
        }
    }

    //This is the actual pathfinding algorithm. We take in a starting position, and a target position, the target position being the ending node we are trying to reach.
    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        //Convert these from positions to nodes using the NodeFromWorldPoint function.
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);
        
        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        HashSet<Node> searchedSet = new HashSet<Node>();
        
        //Reset the startNode's G-Cost so that it doesn't increment whenever we change starting node.
        startNode.gCost = 0;
        
        //Add it to the "openSet" HashSet, which is the collection of Nodes that we will check and eventually pick out a node from to add to our closedSet.
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            //Compare every node within the openSet and if it has the lowest F-Cost set the currentNode local reference to it.
            //If the H-Cost is higher than the current node we are checking from, then we don't want to go there since it is farther away from the target node than we previously were.
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || 
                    openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }
            
            //Remove it from the openSet and add it to the closedSet.
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            //If we have reached the targetNode, meaning we have reached the endNode. Create the path and assign the searchedSet from grid to the local searchedSet within this function.
            //Then return out of the function as we are done with the pathfinding algorithm and have found the shortest path.
            if (currentNode == targetNode)
            {
                RetracePath(startNode, targetNode);
                grid.searchedSet = searchedSet;
                return;
            }

            // Search each neighbouring node to see which will lead to the shortest path.
            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                // If we can't walk on the neighbour or they are already part of the path that is being built, skip over it.
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }
                
                //Calculate the distance to the neighbour.
                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                //Check if the distance, that is to say the G-Cost to move to the neighbour, is lower than the neighbour's G-Cost
                //Alternatively add the neighbour if it is a node that hasn't already been checked.
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    //Assign the costs and the parent for the node being checked.
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    //And if it isn't already within the openSet (A second check just to make sure a node isn't being added twice)
                    //add it to the openSet and the searchedSet.
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                        searchedSet.Add(neighbour);
                    }
                }
            }
        }
    }
    
    //This is the function that will give us the final path from the starting node to the ending node.
    void RetracePath(Node startNode, Node endNode)
    {
        //Create the local path List.
        List<Node> path = new List<Node>();
        //Since we want to go backwards from the endNode to the startNode using the nodes' parent we assigned earlier, we assign currentNode to be equal to the endNode.
        Node currentNode = endNode;
        
        //A loop that adds each node and then assigns currentNode to be its parent. Ending when the currentNode equals the startNode.
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        //Since we don't want to add any unwalkable nodes, we first check for it before adding the actual startNode. 
        if (startNode.walkable)
        {
            path.Add(startNode);
        }
        //Since we want the list of nodes itself to be in order from start to end instead of end to start, we reverse it.
        path.Reverse();

        //Print to the console some values relevant to the model.
        for (int i = 0; i < path.Count; i++)
        {
            Debug.Log("Step: " + i + " World Position: " + path[i].worldPosition + " G-Cost: " + path[i].gCost + " H-Cost: " + path[i].hCost + " F-Cost: " + path[i].fCost);
        }
        
        //Assign the path from grid to the local path reference from this function.
        grid.path = path;
    }
    
    //The function which returns the distance from one node to the other.
    //Of note here is the fact that the diagonals are still calculated normally instead of excluded.
    int GetDistance(Node nodeA, Node nodeB)
    {
        //Since we just want to know how far apart they are in each axis we get the absolute value of the distance between them.
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        //Apply the Obstacle Penalty multiplier to the distances if either of the nodes are an obstacle.
        if (nodeA.obstacle || nodeB.obstacle)
        {
            dstX *= grid.ObstaclePenalty;
            dstY *= grid.ObstaclePenalty;
        }
        
        //The calculation which defines the final costs making it so that the diagonals when going horizontally or vertically are incremented by 14, and the adjacent nodes are incremented by 10. 
        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        return 14 * dstX + 10 * (dstY - dstX);
    }

    //The same Clock function seen in Grid.
    IEnumerator Clock()
    {
        yield return new WaitForSecondsRealtime(waitTime);
        timer += waitTime;
    }
}