using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController instance = null;

    public enum State { PlayerTurn, ZombieTurn, Waiting, };
    public State state = State.PlayerTurn;

    public int numberOfPlayerMoves;
    private int playerMoves;
    public int numberOfZombieMoves;
    public int zombieMoves;
    public int totalSurvivorsCollected;
    public int survivorsCollected;
    public int survivorsKilled;

    public int numberOfZombies;
    public int numberOfZombiesMod;
    public int numberOfSurvivors;

    public int xGridSize;
    public int yGridSize;

    public bool gameStarted;
    public bool gameOver;
    public bool paused;
    public bool roundOver;
    public bool playerDead;

    [SerializeField]
    private TextMeshProUGUI survivorText;
    [SerializeField]
    private TextMeshProUGUI pausedText;
    [SerializeField]
    private TextMeshProUGUI survivorDeadText;
    [SerializeField]
    private GameObject[] turnIndicators;
    [SerializeField]
    private GameObject menuScreen;
    [SerializeField]
    private GameObject portal;
    [SerializeField]
    private TextMeshProUGUI menuScreenText;
    [SerializeField]
    private Zombie zombie;
    [SerializeField]
    private Survivor survivor;

    private List<Vector2Int> spawnLocations = new List<Vector2Int>();

    private float timer = 1.5f;
    private Zombie[] zombies;

    public int PlayerMoves
    {
        get { return playerMoves; }
        set { playerMoves = value; TurnGUI(playerMoves); }
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        playerMoves = numberOfPlayerMoves;
    }

    private void Start()
    {
        Debug.Log("Running");
        SpawnZombies();
        SpawnSurvivors();
        gameStarted = true;
    }

    private void Update()
    {
        survivorText.text = "Survivors: " + totalSurvivorsCollected.ToString();

        if (!gameStarted)
        {
            return;
        }

        IsGameOver();
        TakeTurns();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    void IsGameOver()
    {
        if (!roundOver && !gameOver)
        {
            if (survivorsKilled == numberOfSurvivors)
            {
                StartCoroutine(MenuScreen("The survivors all died.\n Game Over."));
                gameOver = true;
            }

            if (playerDead)
            {
                StartCoroutine(MenuScreen("You were overtaken by zombies.\n Game Over."));
                gameOver = true;
            }

            if (survivorsCollected >= 1 && survivorsKilled + survivorsCollected == numberOfSurvivors)
            {
                roundOver = true;
                StartCoroutine(EndRound());
            }
        }
    }

    void TakeTurns()
    {
        if (!roundOver || !gameOver)
        {
            if (playerMoves == 0)
            {
                playerMoves = numberOfPlayerMoves;
                state = State.ZombieTurn;
            }

            if (state == State.ZombieTurn && !gameOver)
            {
                state = State.Waiting;
                foreach (Zombie z in zombies)
                {
                    StartCoroutine(z.Move());
                }
                timer = 1f;
            }

            if (state == State.Waiting)
            {
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    TurnGUIReset();
                    state = State.PlayerTurn;
                }
            }
        }
    }

    void SpawnZombies()
    {
        numberOfZombies += numberOfZombiesMod;
        for(int i = 0; i < numberOfZombies; i++)
        {
            Vector2 spawnPos = GetSpawnPosition();
            Instantiate(zombie, spawnPos, Quaternion.identity);
        }

        zombies = FindObjectsOfType<Zombie>();
    }

    void SpawnSurvivors()
    {
        for (int i = 0; i < numberOfSurvivors; i++)
        {
            Vector2Int spawnPos = GetSpawnPosition();
            Instantiate(survivor, new Vector2(spawnPos.x, spawnPos.y), Quaternion.identity);
        }
    }

    Vector2Int GetSpawnPosition()
    {
        Vector2Int pos = Vector2Int.zero;
        bool solved = false;

        while (!solved)
        {
            int xAxis = Random.Range(-(xGridSize / 2 - 1), xGridSize / 2);
            int yAxis = Random.Range(-(yGridSize / 2 - 1), yGridSize / 2);
            pos = new Vector2Int(xAxis, yAxis);

            if (pos != Vector2Int.zero)
            {
                if (!spawnLocations.Contains(pos))
                {
                    spawnLocations.Add(pos);
                    solved = true;
                }
            }
        }

        return pos;
    }

    void TurnGUI(int i)
    {
        turnIndicators[i].SetActive(false);
    }

    void TurnGUIReset()
    {
        foreach(GameObject g in turnIndicators)
        {
            g.SetActive(true);
        }
    }

    void Pause()
    {
        paused = !paused;
        if (paused)
        {
            Time.timeScale = 0;
            pausedText.text = "- Stage Complete -";
        }
        else
        {
            Time.timeScale = 1;
            pausedText.text = "";
        }
    }

    IEnumerator ResetLevel()
    {
        survivorsKilled = 0;
        survivorsCollected = 0;

        gameOver = false;
        paused = false;
        roundOver = false;
        playerDead = false;
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        GameObject[] survivors = GameObject.FindGameObjectsWithTag("Survivor");

        foreach (GameObject z in zombies)
        {
            Destroy(z.gameObject);
        }

        foreach(GameObject s in survivors)
        {
            Destroy(s.gameObject);
        }

        pausedText.text = "- next stage -";
        yield return new WaitForSecondsRealtime(1f);
        pausedText.text = "";

        Transform target = FindObjectOfType<Player>().transform;
        target.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.None;
        target.GetComponent<SpriteRenderer>().enabled = true;

        playerMoves = numberOfPlayerMoves;
        SpawnZombies();
        SpawnSurvivors();
        numberOfZombiesMod = 0;
        
        state = State.PlayerTurn;
        gameStarted = true;
        Time.timeScale = 1;
        yield return null;
    }

    IEnumerator MenuScreen(string message)
    {
        menuScreen.SetActive(true);
        menuScreen.GetComponent<Animator>().SetBool("Open", true);

        yield return new WaitForSeconds(.5f);
        menuScreenText.text = message;
    }

    IEnumerator MenuScreenClose()
    {
        menuScreenText.text = "";
        menuScreen.GetComponent<Animator>().SetBool("Open", false);

        yield return new WaitForSeconds(.5f);
        menuScreen.SetActive(false);
    }

    IEnumerator EndRound()
    {
        Time.timeScale = 0;
        gameStarted = false;
        Transform target = FindObjectOfType<Player>().transform;

        GameObject portalObj = Instantiate(portal, new Vector2(target.position.x, target.position.y - .5f), Quaternion.identity);

        yield return new WaitForSecondsRealtime(1f);
        target.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
        portalObj.GetComponent<Animator>().SetBool("Close", true);

        yield return new WaitForSecondsRealtime(.5f);
        target.GetComponent<SpriteRenderer>().enabled = false;
        pausedText.text = "- Stage Complete -";
        yield return new WaitForSecondsRealtime(1f);
        pausedText.text = "- " + survivorsKilled.ToString() + " suriviors lost -";
        yield return new WaitForSecondsRealtime(1f);
        Destroy(portalObj);
        StartCoroutine(ResetLevel());
    }

    public IEnumerator SurvivorKilled()
    {
        survivorDeadText.text = "A survivor died!";
        yield return new WaitForSecondsRealtime(1);
        survivorDeadText.text = "";
    }
}