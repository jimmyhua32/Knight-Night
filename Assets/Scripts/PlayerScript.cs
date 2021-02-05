using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour
{

    private float moveSpeed = 288;
    public float xaxis;
    public float yaxis;
    private float attackCD = 0.3F;
    private float iTime = 1;
    private float dashCD = 2;
    private float attackDur = 0.1F;
    private bool dashing = false;
    private Vector3 moveDirection;
    public BoxCollider2D playerHitbox;
    private Rigidbody2D rigid;
    public PolygonCollider2D mySword;
    public GameObject healthImage;

    private float health = 3;
    private string facing = "Down";

    private IDictionary<string, float> directions =  new Dictionary<string, float>()
    {
        {"Up", 0},
        {"UpRight", -45},
        {"Right", -90},
        {"DownRight", -135},
        {"Down", 180},
        {"DownLeft", 135},
        {"Left", 90},
        {"UpLeft", 45}
    };

    void Start() {
        rigid = GetComponent<Rigidbody2D>();
        healthImage = GameObject.FindGameObjectWithTag("Health");
    }

    void Update()
    {
        if (!dashing) {
            xaxis = Input.GetAxisRaw("Horizontal") * moveSpeed;
            yaxis = Input.GetAxisRaw("Vertical") * moveSpeed;
            Vector2 Velocity = rigid.velocity;
            Velocity.x = xaxis;
            Velocity.y = yaxis;
            rigid.velocity = Velocity;

            if (rigid.velocity.magnitude > moveSpeed) {
                rigid.velocity *= moveSpeed / rigid.velocity.magnitude;
            }
        }

        if (Input.GetKey(KeyCode.Z) && dashCD <= 0) {
            rigid.velocity *= 10;
            dashCD = 2;
            dashing = true;
            iTime += 0.3F;
        }

        if (dashCD <= 1.92) {
            dashing = false;
        }

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            facing = "";
            if (Input.GetKey(KeyCode.UpArrow))
            {
                facing += "Up";
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                facing += "Down";
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                facing += "Right";
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                facing += "Left";
            }
            mySword.transform.eulerAngles = new Vector3(0, 0, directions[facing]);

            // Animation Facing prep + walking animation
            if ((facing == "UpRight" || facing == "DownRight") && !mySword.enabled)
            {
                facing = "Right";
                gameObject.GetComponent<Spritimation>().SetAnimation("WalkingRight");
            }
            else if ((facing == "UpLeft" || facing == "DownLeft") && !mySword.enabled)
            {
                facing = "Left";
                gameObject.GetComponent<Spritimation>().SetAnimation("WalkingLeft");
            }
            else if (!mySword.enabled)
            {
                gameObject.GetComponent<Spritimation>().SetAnimation("Walking" + facing);
            }
        }
        // Animation Facing For Idle
        else if (!mySword.enabled)
        {
            gameObject.GetComponent<Spritimation>().SetAnimation("Idle" + facing);
        }

        if ((Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)) && attackCD <= 0)
        {
            mySword.enabled = true;
            attackCD = 0.3F;
            attackDur = 0.1F;
            gameObject.GetComponent<Spritimation>().SetOneShotAnimation("Swing" + facing);
        } else
        {
            if (attackDur <= 0)
            {
                mySword.enabled = false;
                attackCD -= Time.deltaTime;
            } else
            {
                attackDur -= Time.deltaTime;
            }
        }
        iTime -= Time.deltaTime;
        dashCD -= Time.deltaTime;
    }

    // Handles death conditions
    void OnCollisionEnter2D(Collision2D collision)
    {
        OnTriggerEnter2D(collision.collider);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if ((collider.CompareTag("Enemy") || collider.CompareTag("AttackBox"))
            && !mySword.enabled && iTime <= 0 && !dashing)
        {
            iTime = 1;
            health -= 1;
            healthImage.GetComponent<Health>().health -= 1;
            if (health == 0)
            {
                GameObject[] mobs = GameObject.FindGameObjectsWithTag("Enemy");
                foreach (GameObject enemy in mobs)
                {
                    Destroy(enemy);
                }
                GameObject.FindGameObjectWithTag("ScoreGUI").GetComponent<Score>().score +=
                    GameObject.FindGameObjectWithTag("Timer").GetComponent<TimerScript>().time / 10;
                SetLeaderBoard(GameObject.FindGameObjectWithTag("ScoreGUI").GetComponent<Score>().score);
                Destroy(gameObject);
                SceneManager.LoadScene("EndScreen");
            }
        }
    }

    // Saves the score to leaderboard
    private void SetLeaderBoard(int score)
    {
        string[] keys =
        {
            "first",
            "second",
            "third",
            "fourth",
            "fifth",
            "sixth",
            "seventh",
            "eighth",
            "nineth",
            "tenth"
        };
        PlayerPrefs.SetString("yourScore", "none");
        for (int i = 0; i < keys.Length; i++)
        {
            if (PlayerPrefs.GetInt(keys[i], 0) < score)
            {
                int tempScore = PlayerPrefs.GetInt(keys[i]);
                PlayerPrefs.SetInt(keys[i], score);
                if (PlayerPrefs.GetString("yourScore") == "none")
                {
                    PlayerPrefs.SetString("yourScore", keys[i]);
                }
                score = tempScore;
            }
        }
    }
}
