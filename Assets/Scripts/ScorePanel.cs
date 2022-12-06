using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public class ScorePanel : MonoBehaviour
{
    public Image pnlColor;
    public TMPro.TMP_Text Score;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetScore(string newScore)
    {
        Score.text = newScore;
        return;
    }

    public string GetScore()
    {
        return Score.text;
    }

    public void SetColor(Color c)
    {
        pnlColor.GetComponent<Image>().color = c;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
