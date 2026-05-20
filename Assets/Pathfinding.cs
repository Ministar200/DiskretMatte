using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pathfinding : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private int cellWidth = 1;
    [SerializeField] private int cellHeight = 1;

    [SerializeField] private bool newPath;
    [SerializeField] private bool visualiseGrid;
    [SerializeField] private bool showTexts;

    [SerializeField] private Transform textPrefab;
    [SerializeField] private Transform textParent;

    private Dictionary<Vector3, Cell> cells;

    public List<Vector3> cellsToSearch;
    public List<Vector3> searchedCells;
    public List<Vector3> finalPath;

    bool pathGenerated;
    private void Update()
    {
        if (newPath && !pathGenerated)
        {
            GenerateGrid();

            FindPath(new Vector3(0, 0, 0), new Vector3(6, 0, 8));

            if (showTexts)
            {
                VisualiseText();
            }

            pathGenerated = true;
        }
        else if (!newPath)
        {
            pathGenerated = false;
        }
    }

    private void GenerateGrid()
    {
        cells = new Dictionary<Vector3, Cell>();

        for (float x = 0; x < gridWidth; x += cellWidth)
        {
            for (float z = 0; z < gridHeight; z += cellHeight)
            {
                Vector3 pos = new Vector3(x,0,z);
                cells.Add(pos, new Cell(pos));
            }
        }

        for (int i = 0; i < 40; i++)
        {
            Vector3 pos = new Vector3(Random.Range(1, gridWidth), 0, Random.Range(1, gridHeight));
            cells[pos].isWall = true;
            if (pos == new Vector3(6,0,8))
            {
                cells[pos].isWall = false;   
            }
        }
    }


    private void FindPath(Vector3 startPos, Vector3 endPos)
    {
        cellsToSearch = new List<Vector3> { startPos };
        searchedCells = new List<Vector3>();
        finalPath = new List<Vector3>();

        cells[startPos].gCost = 0;
        cells[startPos].hCost = GetDistance(startPos, endPos);
        cells[startPos].fCost = GetDistance(startPos, endPos);

        while (cellsToSearch.Count > 0)
        {
            Vector3 cellToSearch = cellsToSearch[0];

            foreach(Vector3 pos in cellsToSearch)
            {
                Cell c = cells[pos];
                if (c.fCost < cells[cellToSearch].fCost ||
                    c.fCost == cells[cellToSearch].fCost && c.hCost == cells[cellToSearch].hCost)
                {
                    cellToSearch = pos;
                }
            }


            cellsToSearch.Remove(cellToSearch);
            searchedCells.Add(cellToSearch);

            if (cellToSearch == endPos)
            {
                Cell pathCell = cells[endPos];

                while (pathCell.position != startPos)
                {
                    finalPath.Add(pathCell.position);
                    pathCell = cells[pathCell.connection];
                }
                
                finalPath.Add(startPos);
                VisualiseText();
                return;
            }

            SearchCellNeighbors(cellToSearch, endPos);
        }

        if (finalPath.Count == 0)
        {
            Debug.Log("Path not found");
        }
    }
    
    private int GetDistance(Vector3 pos1, Vector3 pos2)
    {
        Vector3Int dist = new Vector3Int(Mathf.Abs((int)pos1.x - (int)pos2.x), 0, Mathf.Abs((int)pos1.z - (int)pos2.z));

        int lowest = Mathf.Min(dist.x, dist.z);
        int highest = Mathf.Max(dist.x, dist.z);
        
        int horizontalMovesRequired = highest - lowest;

        return lowest * 14 + horizontalMovesRequired * 10;
    }

    private void SearchCellNeighbors(Vector3 cellPos, Vector3 endPos)
    {
        for (float x = cellPos.x - cellWidth; x <= cellWidth + cellPos.x; x += cellWidth)
        {
            for (float z = cellPos.z - cellHeight; z <= cellHeight + cellPos.z; z += cellHeight)
            {
                Debug.Log(x);
                Debug.Log(z);
                if (x == cellPos.x || z == cellPos.z)
                {
                    Vector3 neighborPos = new Vector3(x, 0, z);
                    Debug.Log(cellPos);
                    
                    if (cells.TryGetValue(neighborPos, out Cell c) && !searchedCells.Contains(neighborPos) && !cells[neighborPos].isWall)
                    {
                        int GcostToNeighbour= cells[cellPos].gCost + GetDistance(cellPos, neighborPos);
                        if (GcostToNeighbour > cells[neighborPos].gCost)
                        {
                            Cell neighbourNode = cells[neighborPos];

                            neighbourNode.connection = cellPos;
                            neighbourNode.gCost = GcostToNeighbour;
                            neighbourNode.hCost = GetDistance(neighborPos, endPos);
                            neighbourNode.fCost = neighbourNode.gCost + neighbourNode.hCost;

                            if (!cellsToSearch.Contains(neighborPos))
                            {
                                cellsToSearch.Add(neighborPos);
                                pathGenerated = true;
                            }
                        }
                    }
                }
            }
        }
    }

    
    private void VisualiseText()
    {
        foreach (Transform child in textParent)
        {
            Destroy(child.gameObject);
        }

        foreach(Vector3 pos in cells.Keys)
        {
            Transform text = Instantiate(textPrefab, pos + (Vector3)transform.position, new Quaternion(), textParent);
            text.GetChild(0).GetComponent<Text>().text = cells[pos].gCost.ToString();
            text.GetChild(1).GetComponent<Text>().text = cells[pos].hCost.ToString();
            text.GetChild(2).GetComponent<Text>().text = cells[pos].fCost.ToString();
        }
    }

    private void OnDrawGizmos()
    {
        if (!visualiseGrid || cells == null)
        {
            return;
        }

        foreach (KeyValuePair<Vector3, Cell> kvp in cells)
        {
            if (!kvp.Value.isWall)
            {
                Gizmos.color = Color.white;
            }
            else
            {
                Gizmos.color = Color.black;
            }

            if (searchedCells.Contains(kvp.Key))
            {
                Gizmos.color = Color.orange;
            }
            
            if (finalPath.Contains(kvp.Key))
            {
                Gizmos.color = Color.magenta;
            }

            float gizmoSize = showTexts ? 0.2f : 1;

            Gizmos.DrawCube(kvp.Key + (Vector3)transform.position, new Vector3(cellWidth,0, cellHeight) * gizmoSize);
        }
    }


public class Cell
{
    public Vector3 position;
    public int fCost;
    public int gCost;
    public int hCost;
    public Vector3 connection;
    public bool isWall;

    public Cell(Vector3 pos)
    {
        position = pos;
    }
}
}