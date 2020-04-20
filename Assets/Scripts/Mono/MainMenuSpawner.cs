using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MainMenuSpawner : MonoBehaviour
{
    public GameObject gameJamPrefab;
    
    public Color[] colors;
    public string[] names;

    public Texture2D imageMap;

    EntityManager entityManager;
    Entity gameJamEntity;
    int numEnts = 0;

    void Start()
    {
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        gameJamEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameJamPrefab, settings);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Update()
    {
        if(numEnts >= 65000) { return; }
        bool foundCandidate = false;
        Vector3 randomPos;
        int layerMask = 1 << 8;
        do
        {
            randomPos = UnityEngine.Random.onUnitSphere;
            Vector3 rayStart = randomPos * 55;
            Vector3 rayEnd = Vector3.zero - rayStart;
            Ray ray = new Ray(rayStart, rayEnd);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f, layerMask))
            {
                Renderer renderer = hit.transform.GetComponent<MeshRenderer>();
                Texture2D texture = renderer.material.mainTexture as Texture2D;
                Vector2 pixelUV = hit.textureCoord;
                pixelUV.x *= texture.width;
                pixelUV.y *= texture.height;
                Vector2 tiling = renderer.material.mainTextureScale;
                Color color = imageMap.GetPixel(Mathf.FloorToInt(pixelUV.x * tiling.x), Mathf.FloorToInt(pixelUV.y * tiling.y));

                int index = FindIndexFromColor(color);
                if (index == 1 || index == 2) { foundCandidate = true; }
            }
        }
        while (!foundCandidate);

        Vector3 spawnPos = randomPos * 49.5f;

        Entity newJamGame = entityManager.Instantiate(gameJamEntity);
        entityManager.SetComponentData(newJamGame, new Translation { Value = spawnPos });
        numEnts++;
    }

    private int FindIndexFromColor(Color col)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i] == col)
            {
                return i;
            }
        }

        return -1;
    }
}
