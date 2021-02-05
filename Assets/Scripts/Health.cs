using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{

    public Sprite onehp;
    public Sprite twohp;
    public Sprite threehp;
    public Sprite zerohp;
    public Sprite[] sprites = new Sprite[4];
    public int health = 3;

    private SpriteRenderer sr;
    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sprites[0] = zerohp;
        sprites[1] = onehp;
        sprites[2] = twohp;
        sprites[3] = threehp;
    }

    // Update is called once per frame
    void Update()
    {   
        sr.sprite = sprites[health];
    }
}
