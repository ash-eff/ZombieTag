using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2Int facingDirection;

    private SpriteRenderer spr;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        GameController.instance = FindObjectOfType<GameController>();
        spr = GetComponent<SpriteRenderer>();
        spr.enabled = true;
    }

    private void Update()
    {
        if (GameController.instance.state == GameController.State.PlayerTurn && !GameController.instance.gameOver)
        {
            Move();
        }
    }

    void Move()
    {
        anim.SetBool("Up", false);
        anim.SetBool("Right", false);
        anim.SetBool("Down", false);
        anim.SetBool("Left", false);

        if (Input.GetKeyDown(KeyCode.W))
        {
            anim.SetBool("Up", true);
            MoveCheck(Vector3.up);
            facingDirection = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y + 2));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            anim.SetBool("Right", true);
            MoveCheck(Vector3.right);
            facingDirection = new Vector2Int(Mathf.RoundToInt(transform.position.x + 2), Mathf.RoundToInt(transform.position.y));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            anim.SetBool("Down", true);
            MoveCheck(-Vector3.up);
            facingDirection = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y - 2));
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            anim.SetBool("Left", true);
            MoveCheck(-Vector3.right);
            facingDirection = new Vector2Int(Mathf.RoundToInt(transform.position.x - 2), Mathf.RoundToInt(transform.position.y));
        }

    }

    void MoveCheck(Vector3 direction)
    {     
        Vector2 clampedPos = transform.position;
        if(Mathf.Abs(transform.position.x + direction.x) > GameController.instance.xGridSize / 2 - 1)
        {
            return;
        }

        if (Mathf.Abs(transform.position.y + direction.y) > GameController.instance.yGridSize / 2 - 1)
        {
            return;
        }

        GameController.instance.PlayerMoves--;
        transform.position += direction;
        clampedPos.x = Mathf.Clamp(transform.position.x, -(GameController.instance.xGridSize / 2 - 1), GameController.instance.xGridSize / 2);
        clampedPos.y = Mathf.Clamp(transform.position.y, -(GameController.instance.yGridSize / 2 - 1), GameController.instance.yGridSize / 2);

        transform.position = clampedPos;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Survivor")
        {
            collision.gameObject.SetActive(false);
            GameController.instance.survivorsCollected++;
            GameController.instance.totalSurvivorsCollected++;
            GameController.instance.remainingSurvivors--;
            StartCoroutine(GameController.instance.GameMessage("You saved a survivor!"));
        }
        if (collision.tag == "Zombie")
        {          
            spr.enabled = false;
            GameController.instance.playerDead = true;
        }
    }
}
