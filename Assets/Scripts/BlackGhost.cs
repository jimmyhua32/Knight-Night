using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackGhost : GhostScript
{

    private float health = 2;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        otherScore = GameObject.FindGameObjectWithTag("ScoreGUI");
        moveSpeed = 100;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Weapon"))
        {
            health--;
            if (health == 0) {
                otherScore.GetComponent<Score>().score += 2;
                Destroy(gameObject);
            }
        } else if (collision.collider.CompareTag("DeathWall")) {
            otherScore.GetComponent<Score>().score += 3;
            Destroy(gameObject);
        }
    }
}
