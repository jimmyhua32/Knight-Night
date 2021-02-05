using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GioSpawnerLogic : MonoBehaviour
{
    // Enemies the spawner will spawn
    public GameObject ghost;
    public GameObject blackGhost;
    public GameObject skeleton;
    public GameObject pumpkin;

    // Time between spawns in seconds
    private double timer;

    // Where the enemy will spawn
    private Vector3 spawnLoc;

    // Location of the SpawnZone's top left corner
    private float zoneX;
    private float zoneY;

    // Padding in px from the edges of the zone for spawning
    public float padX;
    public float padY;

    // Number of enemies that are in the scene
    private float numMobs;

    // Game duration
    private GameObject currTime;

    void Start()
    {
        currTime = GameObject.FindGameObjectWithTag("Timer");
        timer = Random.Range(3, 4);
        zoneX = transform.position.x - (transform.localScale.x * 1440 / 2);
        zoneY = transform.position.y + (transform.localScale.y * 100 / 2);
    }

    // Update is called once per frame
    void Update()
    {
        numMobs = GameObject.FindGameObjectsWithTag("Enemy").Length;
        int time = currTime.GetComponent<TimerScript>().time;
        if (timer <= 0 && GameObject.FindGameObjectWithTag("Player") != null)
        {
            if (numMobs < (10 + time / 10))
            {
                spawnLoc = new Vector3(Random.Range(zoneX + padX, zoneX + (transform.localScale.x * 1440) - padX),
                    Random.Range(zoneY - padY, zoneY - (transform.localScale.y * 100) + padY), 0);
                float rand = Random.Range(0F, 1F);
                GameObject enemy;
                if (rand < 0.1F)
                {
                    enemy = blackGhost;
                } else if (rand < 0.7F) {
                    enemy = ghost;
                } else if (rand < 0.95F)
                {
                    enemy = pumpkin;
                } else
                {
                    enemy = skeleton;
                }
                Instantiate(enemy, spawnLoc, Quaternion.identity);
            }
            timer = Random.Range(3F,4F);
        } 
        else
        {
            timer -= Time.deltaTime;
        }
    }
}
