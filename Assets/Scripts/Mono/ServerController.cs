using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerController : MonoBehaviour
{
    public static ServerController instance;

    public int maxServerLoad;
    public Slider serverLoadSlider;
    public float timePerOp;
    public Text serverLoadText;
    public Text moneyText, gamesText;
    public int moneyInBank;
    public int moneyPerGame;

    private int curServerLoad = 0;
    private int gamesProcessed;
    private float timeSinceLastOp;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        timeSinceLastOp = 0f;
        serverLoadSlider.maxValue = maxServerLoad;
        UpdateServerUI();
    }

    private void Update()
    {
        if(curServerLoad <= 0) { return; }
        timeSinceLastOp += Time.deltaTime;
        while(curServerLoad > 0 && timeSinceLastOp >= timePerOp)
        {
            curServerLoad--;
            timeSinceLastOp -= timePerOp;
        }
        UpdateServerUI();       
    }

    public void IncreaseServerLoad(int numIncrease)
    {
        curServerLoad += numIncrease;
        moneyInBank += moneyPerGame;
        gamesProcessed += numIncrease;
        UpdateServerUI();
    }

    public void UpdateServerUI()
    {
        serverLoadSlider.value = curServerLoad;
        serverLoadText.text = "Server Load " + curServerLoad + " / " + maxServerLoad;
        moneyText.text = "$" + moneyInBank;
        gamesText.text = "Games Processed: " + gamesProcessed;
    }

    

}
