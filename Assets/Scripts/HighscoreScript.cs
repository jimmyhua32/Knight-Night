using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighscoreScript : MonoBehaviour
{

    private string[] keys =
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

    void Start()
    {
        GameObject[] scores = GameObject.FindGameObjectsWithTag("Score");
        for (int i = 0; i < keys.Length; i++)
        {
            int score = PlayerPrefs.GetInt(keys[i], 0);
            scores[i].GetComponent<Text>().text = "" + score;
            if (keys[i] == PlayerPrefs.GetString("yourScore"))
            {
                scores[i].GetComponent<Text>().color = Color.magenta;
                PlayerPrefs.SetString("yourScore", "none");
            }
        }
    }
}
