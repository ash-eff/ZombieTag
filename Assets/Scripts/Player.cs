using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2Int facingDirection;

    private GameController gc;

    private void Awake()
    {
        gc = FindObjectOfType<GameController>();
    }

    private void Update()
    {
        if (gc.state == GameController.State.PlayerTurn)
        {
            Move();
        }
    }

    void Move()
    {
        // only allow and count valid moves once you clamp the player to an area
        if (Input.GetKeyDown(KeyCode.W))
        {
            gc.playerMoves++;
            transform.position += Vector3.up;
            facingDirection = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y + 2));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            gc.playerMoves++;
            transform.position += Vector3.right;
            facingDirection = new Vector2Int(Mathf.RoundToInt(transform.position.x + 2), Mathf.RoundToInt(transform.position.y));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            gc.playerMoves++;
            transform.position -= Vector3.up;
            facingDirection = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y - 2));
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            gc.playerMoves++;
            transform.position -= Vector3.right;
            facingDirection = new Vector2Int(Mathf.RoundToInt(transform.position.x - 2), Mathf.RoundToInt(transform.position.y));
        }
    }
}
