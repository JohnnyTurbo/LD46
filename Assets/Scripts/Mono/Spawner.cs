using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static Spawner instance;

    public GameObject gameJamPrefab;
    //public GameObject earthObj;
    public Color[] colors;
    public string[] names;
    public int[] pcts;

    public Texture2D imageMap;
    public float distToTarget;
    public float entSpeed;
    public float spawnTime;
    public List<Entity> jamGames;

    private float timeSinceLastSpawn = 0;
    
    EntityManager entityManager;
    Entity gameJamEntity;
    /*
    Ray ray;
    Vector3 rayStart;
    Vector3 rayEnd;
    */

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        gameJamEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameJamPrefab, settings);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        jamGames = new List<Entity>();
    }

    private void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        while (timeSinceLastSpawn >= spawnTime)
        {
            timeSinceLastSpawn -= spawnTime;
            bool foundCandidate = false;
            Vector3 randomPos;
            int layerMask = 1 << 8;
            int randLayer = GetSpawnIndex();
            int indexr = 0;
            int tries = 0;
            do
            {
                randomPos = UnityEngine.Random.onUnitSphere;
                Vector3 rayStart = randomPos * 55;
                Vector3 rayEnd = Vector3.zero - rayStart;
                Ray ray = new Ray(rayStart, rayEnd);
                RaycastHit hit;


                if (Physics.Raycast(ray, out hit, 10f, layerMask))
                {
                    //Debug.Log("HIT");
                    Renderer renderer = hit.transform.GetComponent<MeshRenderer>();
                    Texture2D texture = renderer.material.mainTexture as Texture2D;
                    Vector2 pixelUV = hit.textureCoord;
                    pixelUV.x *= texture.width;
                    pixelUV.y *= texture.height;
                    Vector2 tiling = renderer.material.mainTextureScale;
                    Color color = imageMap.GetPixel(Mathf.FloorToInt(pixelUV.x * tiling.x), Mathf.FloorToInt(pixelUV.y * tiling.y));

                    indexr = FindIndexFromColor(color);
                    if (indexr >= randLayer) { foundCandidate = true; }
                    tries++;
                    if (tries >= 50)
                    {
                        tries = 0;
                        randLayer--;
                        if (randLayer <= 2)
                        {
                            randLayer = 2;
                        }
                    }
                }
            }
            while (!foundCandidate);
            //Debug.Log("Spawning on land");
            //Debug.Log(tries + " for layer " + names[randLayer] + " but actually: " + names[indexr]);
            Vector3 spawnPos = randomPos * 49.5f;
            //Instantiate(gameJamPrefab, spawnPos, Quaternion.identity);
            Entity newJamGame = entityManager.Instantiate(gameJamEntity);
            entityManager.SetComponentData(newJamGame, new Translation { Value = spawnPos });
            entityManager.SetComponentData(newJamGame, new JamGameData
            {
                isAttracted = false,
                curPosIndex = 0,
                attractorID = -1
                //distToTarget = distToTarget,
                //speed = entSpeed
            });
            entityManager.SetComponentData(newJamGame, new JamTravelData
            {
                speed = entSpeed,
                distToTarget = distToTarget,
                startPos = spawnPos
            });
            jamGames.Add(newJamGame);
        }
        /*
        do
        {

        }
        while()
        */
        /*
        var instance = entityManager.Instantiate(gameJamEntity);
        entityManager.SetComponentData(instance, new Translation { Value = position });
        */
    }

    private int FindIndexFromColor(Color col)
    {
        for(int i = 0; i < colors.Length; i++)
        {
            if(colors[i] == col)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetSpawnIndex()
    {
        int totalPct = 0;

        int randNum = UnityEngine.Random.Range(1, 101);

        for(int i = 0; i < pcts.Length; i++)
        {
            totalPct += pcts[i];
            if(randNum <= totalPct)
            {
                return i;
            }
        }
        return 5;
    }

    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayStart, rayEnd);
    }
    */
}