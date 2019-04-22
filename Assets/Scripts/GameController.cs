using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    Waypoint waypointPrefab;

    public int xGridSize;
    public int yGridSize;

    public int numberOfPlayerMoves;
    public int playerMoves;
    public int numberOfZombieMoves;
    public int zombieMoves;

    float timer = 1.5f;
    Zombie[] zombies;

    public enum State { PlayerTurn, ZombieTurn, Waiting };
    public State state = State.PlayerTurn;

    private void Awake()
    {
        zombies = FindObjectsOfType<Zombie>();
    }

    private void Update()
    {
        if(playerMoves == numberOfPlayerMoves)
        {
            playerMoves = 0;
            state = State.ZombieTurn;
        }

        if(state == State.ZombieTurn)
        {
            foreach(Zombie z in zombies)
            {
                StartCoroutine(z.Move());
            }
            timer = 1.5f;
            state = State.Waiting;
        }

        if(state == State.Waiting)
        {
            timer -= Time.deltaTime;
            if(timer < 0)
            {
                state = State.PlayerTurn;
            }
        }
    }
}
