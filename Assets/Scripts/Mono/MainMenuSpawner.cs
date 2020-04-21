using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Collections;

public class MainMenuSpawner : MonoBehaviour
{
    public GameObject gameJamPrefab;
    
    public Color[] colors;
    public string[] names;

    public Texture2D imageMap;

    public GameObject mainMenuContainer, howToPlayContainer, loadScreenContainer;
    public Button playButton;
    public float loadDelay;

    EntityManager entityManager;
    Entity gameJamEntity;
    int numEnts = 0;
    bool exitingMenu = false;

    void Start()
    {
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        gameJamEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(gameJamPrefab, settings);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        CheckTutorialStatus();
    }

    private void Update()
    {
        if (exitingMenu) { return; }
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

    public void OnButtonTMGYT()
    {
        Application.OpenURL("https://YouTube.com/TurboMakesGames/");
    }

    public void OnButtonHowToPlay()
    {
        mainMenuContainer.SetActive(false);
        howToPlayContainer.SetActive(true);
        PlayerPrefs.SetInt("KnowsHowToPlay", 1);
    }

    public void OnButtonPlay()
    {
        exitingMenu = true;
        StartCoroutine(LoadGame());
    }

    public void OnButtonQuit()
    {
        Application.Quit();
    }

    private IEnumerator LoadGame()
    {
        loadScreenContainer.SetActive(true);
        NativeArray<Entity> entities = entityManager.GetAllEntities(Allocator.Temp);
        entityManager.DestroyEntity(entities);
        entities.Dispose();
        yield return new WaitForSeconds(loadDelay);
        SceneManager.LoadScene(1);
    }

    public void OnButtonBack()
    {
        howToPlayContainer.SetActive(false);
        mainMenuContainer.SetActive(true);
        CheckTutorialStatus();
    }

    private void CheckTutorialStatus()
    {
        if (PlayerPrefs.GetInt("KnowsHowToPlay", 0) == 0)
        {
            playButton.interactable = false;
        }
        else
        {
            playButton.interactable = true;
        }
    }
}
