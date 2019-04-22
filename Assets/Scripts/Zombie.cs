using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    [SerializeField]
    Waypoint startWaypoint, endWaypoint;

    public GameObject node;

    public enum Type { Blinky, Pinky, Inky, Clyde, };
    public Type type;

    public Color color;
    public Transform target;
    public Transform selectedSurvivor;
    private GameController gc;
    private Waypoint searchCenter;

    private bool moving;
    public bool isRunning = true;

    private Queue<Waypoint> queue = new Queue<Waypoint>();
    private Waypoint[] waypoints;
    private Survivor[] survivors;
    private Dictionary<Vector2Int, Waypoint> grid = new Dictionary<Vector2Int, Waypoint>();
    private List<Waypoint> path = new List<Waypoint>();

    Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    private void Awake()
    {
        gc = FindObjectOfType<GameController>();
        
        if(type == Type.Blinky)
        {
            color = Color.red;
        }
        else if (type == Type.Pinky)
        {
            color = Color.magenta;
        }
        else if (type == Type.Inky)
        {
            survivors = FindObjectsOfType<Survivor>();
            int ind = Random.Range(0, survivors.Length);
            selectedSurvivor = survivors[ind].transform;
            color = Color.blue;
        }
        else if (type == Type.Clyde)
        {
            survivors = FindObjectsOfType<Survivor>();
            int ind = Random.Range(0, survivors.Length);
            selectedSurvivor = survivors[ind].transform;
            color = Color.green; 
        }
    }

    public IEnumerator Move()
    {
        ClearInfo();
        CreateGrid();
        CheckPositions();
        BreadthFirstSearch();
        CreatePath();

        int moves = 0;
        while(moves < gc.numberOfZombieMoves)
        {
            if(new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)) == endWaypoint.GridPos)
            {
                // skip
            }
            else
            {
                transform.position = new Vector2(path[moves + 1].GridPos.x, path[moves + 1].GridPos.y);
            }

            moves++;
            gc.zombieMoves++;

            yield return new WaitForSeconds(.5f);
        }

        moving = false;
    }

    private void CheckPositions()
    {
        Vector2Int myPos = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        Vector2Int targetPos = Vector2Int.zero;

        if (type == Type.Blinky)
        {
            targetPos = new Vector2Int(Mathf.RoundToInt(target.position.x), Mathf.RoundToInt(target.position.y));
        }
        else if (type == Type.Pinky)
        {
            targetPos = new Vector2Int(Mathf.RoundToInt(target.GetComponent<Player>().facingDirection.x), Mathf.RoundToInt(target.GetComponent<Player>().facingDirection.y));
        }
        else if (type == Type.Inky)
        {
            Vector3 dist = selectedSurvivor.position - new Vector3(target.position.x, target.position.y, 0f).normalized;
            Vector3 tar = new Vector3(target.position.x, target.position.y, 0f) + dist;
            Debug.Log(tar);
            targetPos = new Vector2Int(Mathf.RoundToInt(tar.x), Mathf.RoundToInt(tar.y));
        }
        else
        {
            targetPos = new Vector2Int(Mathf.RoundToInt(selectedSurvivor.position.x), Mathf.RoundToInt(selectedSurvivor.position.y));
        }

        foreach (KeyValuePair<Vector2Int, Waypoint> entry in grid)
        {
            if (grid.ContainsKey(myPos))
            {
                if(entry.Value.GridPos == myPos)
                {
                    startWaypoint = entry.Value;
                }
            }
            if (grid.ContainsKey(targetPos))
            {
                if (entry.Value.GridPos == targetPos)
                {
                    endWaypoint = entry.Value;
                    GameObject _node = Instantiate(node, new Vector2(targetPos.x, targetPos.y), Quaternion.identity);
                    _node.GetComponent<SpriteRenderer>().color = color;
                    Destroy(_node.gameObject, 2f);
                }
            }
        }
    }

    private void BreadthFirstSearch()
    {
        queue.Clear();
        queue.Enqueue(startWaypoint);

        while (queue.Count > 0 && isRunning)
        {
            searchCenter = queue.Dequeue();
            HaltIfEndFound();
            ExploreNeighbours();
            searchCenter.isExplored = true;
        }
    }

    private void ClearInfo()
    {
        if(path.Count > 0)
        {
            startWaypoint = null;
            endWaypoint = null;

            foreach (Waypoint waypoint in path)
            {
                waypoint.isExplored = false;
                waypoint.exploredFrom = null;
            }

            isRunning = true;
            grid.Clear();
            path.Clear();
        }
    }

    private void CreatePath()
    {
        SetAsPath(endWaypoint);

        Waypoint previousWaypoint = endWaypoint.exploredFrom;
        while (previousWaypoint != startWaypoint)
        {
            SetAsPath(previousWaypoint);
            previousWaypoint = previousWaypoint.exploredFrom;
        }

        SetAsPath(startWaypoint);
        path.Reverse();
    }

    private void SetAsPath(Waypoint waypoint)
    {
        path.Add(waypoint);
    }

    private void HaltIfEndFound()
    {
        if (searchCenter == endWaypoint)
        {
            isRunning = false;
        }
    }

    private void ExploreNeighbours()
    {
        if (!isRunning) { return; }

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighbourCoordinates = searchCenter.GridPos + direction;
            if (grid.ContainsKey(neighbourCoordinates))
            {
                QueueNewNeighbours(neighbourCoordinates);
            }
        }
    }

    private void QueueNewNeighbours(Vector2Int neighbourCoordinates)
    {
        Waypoint neighbour = grid[neighbourCoordinates];
        if (neighbour.isExplored || queue.Contains(neighbour))
        {
            // do nothing
        }
        else
        {
            queue.Enqueue(neighbour);
            neighbour.exploredFrom = searchCenter;
        }
    }

    private void CreateGrid()
    {
        for (int i = -Mathf.RoundToInt(gc.xGridSize / 2) + 1; i < (gc.xGridSize / 2) + 1; i++)
        {
            for (int j = -Mathf.RoundToInt(gc.xGridSize / 2) + 1; j < (gc.yGridSize / 2) + 1; j++)
            {
                Waypoint _waypoint = new Waypoint(false, null, new Vector2Int(i, j));
                grid.Add(new Vector2Int(i, j), _waypoint);
            }
        }
        Debug.Log("New Grid Loaded for " + transform.name);
    }
}
