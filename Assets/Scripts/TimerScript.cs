using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{

    public Text timer;
    public int time;
    private float timeF;

    void Start()
    {
        time = 0;
        timer.text = "0";
        timeF = 0F;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            timeF += Time.deltaTime;
            time = Convert.ToInt32(timeF);
            timer.text = "" + time;
        }
    }
}
