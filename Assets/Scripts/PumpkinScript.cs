using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PumpkinScript : MonoBehaviour
{
    public Rigidbody2D myRigidbody2D;
    public BoxCollider2D myBoxCollider2D;
    public GameObject otherScore;
    public BoxCollider2D attackBox;
    
    private float moveSpeed = 200;
    public GameObject player;

    private float waitTime = 1.25F;
    private bool exploding = false;
    private bool dead = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        otherScore = GameObject.FindGameObjectWithTag("ScoreGUI");
    }

    // Update is called once per frame
    void Update()
    {  
        attackBox.enabled = false;
        Vector2 playerPosition = player.transform.position;
        if (Vector2.Distance(transform.position, playerPosition) > 100 && waitTime >= 1.25)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerPosition, moveSpeed * Time.deltaTime);
        } else {
            exploding = true;
            myRigidbody2D.velocity = new Vector2(0, 0);
            waitTime -= Time.deltaTime;
            if (waitTime < .5) {
                dead = true;
                attackBox.enabled = true;
            }
            if (waitTime <= 0) {
                attackBox.enabled = false;
                Destroy(gameObject);
            }
        }

        if (dead)
        {
            gameObject.GetComponent<Spritimation>().SetAnimation("Death");
        }
        else if (Vector2.MoveTowards(transform.position, playerPosition, moveSpeed * Time.deltaTime).x - transform.position.x < 0)
        {
            if (exploding)
            {
                gameObject.GetComponent<Spritimation>().SetAnimation("ExplodeLeft");
            }
            else
            {
                gameObject.GetComponent<Spritimation>().SetAnimation("HopLeft");
            }
        }
        else if (Vector2.MoveTowards(transform.position, playerPosition, moveSpeed * Time.deltaTime).x - transform.position.x > 0)
        {
            if (exploding)
            {
                gameObject.GetComponent<Spritimation>().SetAnimation("ExplodeRight");
            }
            else
            {
                gameObject.GetComponent<Spritimation>().SetAnimation("HopRight");
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Weapon"))
        {
            otherScore.GetComponent<Score>().score += 3;
            Destroy(gameObject);
        } else if (collision.collider.CompareTag("DeathWall")) {
            otherScore.GetComponent<Score>().score += 4;
            Destroy(gameObject);
        }
    }

}
