using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    [SerializeField]
    Waypoint startWaypoint, endWaypoint;

    public enum Type { Blinky = 0, Pinky = 1, Inky = 2, };
    public Type type;

    private Transform target;
    public Transform selectedSurvivor;
    private Waypoint searchCenter;

    private bool moving;
    private bool isRunning = true;

    private Queue<Waypoint> queue = new Queue<Waypoint>();
    private Waypoint[] waypoints;
    private Survivor[] survivorsInGame;
    private List<Survivor> survivors = new List<Survivor>();
    private Dictionary<Vector2Int, Waypoint> grid = new Dictionary<Vector2Int, Waypoint>();
    private List<Waypoint> path = new List<Waypoint>();

    Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    private void Awake()
    {
        int index = Random.Range(0, 3);
        type = (Type)index;
        target = FindObjectOfType<Player>().transform;
    }

    private void Start()
    {
        survivorsInGame = FindObjectsOfType<Survivor>();
        FindSurvivors();
    }

    public IEnumerator Move()
    {
        if (endWaypoint != null && transform.position == new Vector3(endWaypoint.GridPos.x, endWaypoint.GridPos.y, 0f))
        {
            FindSurvivors();
        }

        ClearInfo();
        CreateGrid();
        CheckPositions();
        BreadthFirstSearch();
        CreatePath();
        moving = true;

        int moves = 0;

        while (moves < GameController.instance.numberOfZombieMoves && moving)
        {
            yield return new WaitForSeconds(.5f);

            if (new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y)) == endWaypoint.GridPos)
            {
                moving = false;
            }
            else
            {
                transform.position = new Vector2(path[moves + 1].GridPos.x, path[moves + 1].GridPos.y);
            }

            moves++;
            GameController.instance.zombieMoves++;           
        }

        moving = false;
    }

    private void FindSurvivors()
    {
        selectedSurvivor = null;
        survivors.Clear();
        bool survivorsAvailable = false;

        foreach(Survivor surv in survivorsInGame)
        {
            if (surv.gameObject.activeInHierarchy)
            {
                survivors.Add(surv);
                survivorsAvailable = true;
            }
        }

        if(survivors != null && survivorsAvailable)
        {
            int ind = Random.Range(0, survivors.Count);
            selectedSurvivor = survivors[ind].transform;
        }
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
                    //GameObject _node = Instantiate(node, new Vector2(targetPos.x, targetPos.y), Quaternion.identity);
                    //_node.GetComponent<SpriteRenderer>().color = color;
                    //Destroy(_node.gameObject, 2f);
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
        for (int i = -Mathf.RoundToInt(GameController.instance.xGridSize / 2) - 1; i < (GameController.instance.xGridSize / 2); i++)
        {
            for (int j = -Mathf.RoundToInt(GameController.instance.yGridSize / 2) - 1; j < (GameController.instance.yGridSize / 2); j++)
            {
                Waypoint _waypoint = new Waypoint(false, null, new Vector2Int(i, j));
                grid.Add(new Vector2Int(i, j), _waypoint);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Survivor")
        {
            collision.gameObject.SetActive(false);
            moving = false;
            StartCoroutine(GameController.instance.SurvivorKilled());
            GameController.instance.numberOfZombiesMod++;
            GameController.instance.survivorsKilled++;
        }
    }
}
