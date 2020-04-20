using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerController : MonoBehaviour
{
    public static ServerController instance;

    public int maxServerLoad;
    public Slider serverLoadSlider;
    //public int opsPerSecond;
    public float timePerOp;
    public Text serverLoadText;

    private int curServerLoad = 50;
    
    private float timeSinceLastOp;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //timePerOp = 1f / opsPerSecond;
        timeSinceLastOp = 0f;
        serverLoadSlider.maxValue = maxServerLoad;
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
        UpdateServerUI();
    }

    public void UpdateServerUI()
    {
        serverLoadSlider.value = curServerLoad;
        serverLoadText.text = "Server Load " + curServerLoad + " / " + maxServerLoad;
    }

}
