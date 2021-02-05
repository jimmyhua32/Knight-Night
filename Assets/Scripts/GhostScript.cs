using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostScript : MonoBehaviour
{

    public Rigidbody2D myRigidbody2D;
    public BoxCollider2D myBoxCollider2D;
    protected GameObject otherScore;

    protected float moveSpeed = 50;
    protected GameObject player;
    private bool jumping = false;

    private float waitTime = 1;
    private Vector2 lastPosition;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        otherScore = GameObject.FindGameObjectWithTag("ScoreGUI");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 playerPosition = player.transform.position;
        if (Vector2.Distance(transform.position, playerPosition) > 200 && waitTime == 1)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerPosition, moveSpeed * Time.deltaTime);
        }
        else
        {
            if (waitTime > 0)
            {
                jumping = true;
                if (waitTime > 0.2)
                {
                    lastPosition = playerPosition;
                    }
                waitTime -= Time.deltaTime;
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, lastPosition, moveSpeed * Time.deltaTime * 15);
                if (Vector2.Distance(transform.position, lastPosition) < 10)
                {
                    waitTime = 1;
                    jumping = false;
                }

            }
        }

        if (Vector2.MoveTowards(transform.position, playerPosition, moveSpeed * Time.deltaTime).x - transform.position.x < 0)
        {
            if (jumping)
            {
                gameObject.GetComponent<Spritimation>().SetAnimation("AngryHoverLeft");
            }
            else
            {
                gameObject.GetComponent<Spritimation>().SetAnimation("HoverLeft");
            }
        }
        else if (Vector2.MoveTowards(transform.position, playerPosition, moveSpeed * Time.deltaTime).x - transform.position.x> 0)
        {
            if (jumping)
            {
                gameObject.GetComponent<Spritimation>().SetAnimation("AngryHoverRight");
            }
            else
            {
                gameObject.GetComponent<Spritimation>().SetAnimation("HoverRight");
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Weapon"))
        {
            otherScore.GetComponent<Score>().score += 1;
            Destroy(gameObject);
        } else if (collision.collider.CompareTag("DeathWall")) {
            otherScore.GetComponent<Score>().score += 2;
            Destroy(gameObject);
        }
    }

}
