using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Collections;

[ShowOdinSerializedPropertiesInInspector]
public class UpgradeController : SerializedMonoBehaviour
{
    public static UpgradeController instance;

    [OdinSerialize]
    [TableMatrix(HorizontalTitle ="Types", VerticalTitle = "Upgrade Costs")]
    public int[,] upgradeCosts = new int[4,4];

    [OdinSerialize]
    [TableMatrix(HorizontalTitle = "Types", VerticalTitle = "Upgrade Values")]
    public float[,] upgradeValues = new float[4,4];

    [OdinSerialize]
    [TableMatrix(HorizontalTitle = "Types", VerticalTitle = "Upgrade Names")]
    public string[,] upgradeNames = new string[4, 4];

    private int[] curLevels = new int[4] { 0, 0, 0, 0 };

    EntityManager entityManager;

    public Text speedCur, speedNext, bandCur, bandNext, procCur, procNext, upCur, upNext;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    public void UpgradeLinkSpeed()
    {
        if (!CanUpgrade(0)) { return; }
        curLevels[0]++;
        List<Entity> jamGames = Spawner.instance.jamGames;
        foreach(Entity jamGame in jamGames)
        {
            JamTravelData travelData = entityManager.GetComponentData<JamTravelData>(jamGame);
            entityManager.SetComponentData(jamGame, new JamTravelData
            {
                speed = upgradeValues[0, curLevels[0]],
                distToTarget = travelData.distToTarget,
                startPos = travelData.startPos
            });
            Spawner.instance.entSpeed = upgradeValues[0, curLevels[0]];
        }
        ChargeBank(0);
        UpdateUpgradeUI();
    }

    public void UpgradeBandwidth()
    {
        if (!CanUpgrade(1)) { return; }
        curLevels[1]++;
        InputController.instance.GrowAttractors(upgradeValues[1,curLevels[1]]);
        ChargeBank(1);
        UpdateUpgradeUI();
    }

    public void UpgradeProc()
    {
        if (!CanUpgrade(2)) { return; }
        curLevels[2]++;
        ServerController.instance.timePerOp = upgradeValues[2, curLevels[2]];
        ChargeBank(2);
        UpdateUpgradeUI();
    }

    public void UpgradeUplinks()
    {
        if (!CanUpgrade(3)) { return; }
        InputController.instance.SpawnNewAtt();
        curLevels[3]++;
        ChargeBank(3);
        UpdateUpgradeUI();
        
    }

    private void ChargeBank(int index)
    {
        ServerController.instance.moneyInBank -= upgradeCosts[index, curLevels[index]];
    }

    public bool CanUpgrade(int upgradeID)
    {
        Debug.Log(upgradeID);
        if (curLevels[upgradeID] >= 3) {
            Debug.Log("Max LVL");
            return false;

        }

        if (ServerController.instance.moneyInBank < upgradeCosts[upgradeID, curLevels[upgradeID] + 1])
        {
            Debug.Log("Outta Money");
            return false;
        }
        return true;
    }

    public float GetCurScale()
    {
        return upgradeValues[1, curLevels[1]];
    }

    public void UpdateUpgradeUI()
    {
        speedCur.text = GetCurName(0);
        speedNext.text = GetNextName(0);
        bandCur.text = GetCurName(1);
        bandNext.text = GetNextName(1);
        procCur.text = GetCurName(2);
        procNext.text = GetNextName(2);
        upCur.text = GetCurName(3);
        upNext.text = GetNextName(3);
    }

    private string GetCurName(int index)
    {
        return upgradeNames[index, curLevels[index]];
    }

    private string GetNextName(int index)
    {
        if(curLevels[index] >= 3) { return "Max"; }
        string nextName = upgradeNames[index, curLevels[index] + 1];
        nextName += "\n$" + upgradeCosts[index, curLevels[index] + 1];
        return nextName;
    }

    public void OnButtonMainMenu()
    {
        NativeArray<Entity> entities = entityManager.GetAllEntities(Allocator.Temp);
        entityManager.DestroyEntity(entities);
        entities.Dispose();
        SceneManager.LoadScene(0);
    }
}
