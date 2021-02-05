using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SkeletonScript : MonoBehaviour
{
    public Rigidbody2D myRigidbody2D;
    public BoxCollider2D myBoxCollider2D;
    public BoxCollider2D attackBox;
    private float health;

    private float moveSpeed = 40;
    public GameObject player;

    private float waitTime = 0;
    private bool attacking = false;
    private GameObject otherScore;
    private float distanceX;
    private float distanceY;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        otherScore = otherScore = GameObject.FindGameObjectWithTag("ScoreGUI");
        attackBox.enabled = false;
        health = 3;
    }

    //Update is called once per frame
    void Update()
    {   
        Vector2 playerPosition = player.transform.position;
        if (Vector2.Distance(transform.position, playerPosition) >= 150 && !attacking)
        {
            distanceX = transform.position.x - playerPosition.x;
            distanceY = transform.position.y - playerPosition.y;
            transform.position = Vector2.MoveTowards(transform.position, playerPosition, moveSpeed * Time.deltaTime);

        }
        else if (Vector2.Distance(transform.position, playerPosition) < 150 || attacking)
        {
            myRigidbody2D.velocity = new Vector2(0, 0);
            if (!attacking && waitTime <= 0)
            {
                attacking = true;
                waitTime = 3;
            }
            else if (attacking && waitTime <= 3 && waitTime > 2)
            {
                gameObject.GetComponent<Spritimation>().SetOneShotAnimation("Jump");
                myBoxCollider2D.enabled = false;
            }
            else if (attacking && waitTime <= 2 && waitTime > 1.5)
            {
                gameObject.GetComponent<Spritimation>().SetAnimation("Landing");
                attackBox.enabled = true;
                myBoxCollider2D.enabled = true;
            }
            else if (waitTime <= 1.5 && waitTime > 1)
            {
                gameObject.GetComponent<Spritimation>().SetAnimation("Landed");
                attackBox.enabled = false;
            }
            else
            {
                attacking = false;
            }
        }

        // Was only tracking attack CD while in range
        waitTime -= Time.deltaTime;

        if (!attacking)
        {
            if (Math.Abs(distanceX) >= Math.Abs(distanceY))
            {
                if (distanceX < 0)
                {
                    gameObject.GetComponent<Spritimation>().SetAnimation("WalkRight");
                }
                else
                {
                    gameObject.GetComponent<Spritimation>().SetAnimation("WalkLeft");
                }
            }
            else
            {
                if (distanceY < 0)
                {
                    gameObject.GetComponent<Spritimation>().SetAnimation("WalkUp");
                }
                else
                {
                    gameObject.GetComponent<Spritimation>().SetAnimation("WalkDown");
                }
            }
        }

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Weapon"))
        {
            health--;
            if (health == 0)
            {
                otherScore.GetComponent<Score>().score += 5;
                Destroy(gameObject);
            }
        } else if (collision.collider.CompareTag("DeathWall")) {
            otherScore.GetComponent<Score>().score += 6;
            Destroy(gameObject);
        }
    }
}
