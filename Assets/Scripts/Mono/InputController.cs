using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class InputController : MonoBehaviour
{
    public static InputController instance;

    public GameObject attractorPrefab;
    public GameObject clickTargetPrefab;
    public GameObject hudContainer, serverUpgradeContainer;
    public Transform earth;
    public Transform ldMainServer;
    public int lineSegments;
    public float hitOffset;
    public float attractRadius;
    public float maxLineHeight;
    public float sinMod;
    public Vector3 initSpawnPos;
    public List<float3[]> movePaths;
    public List<GameObject> curClickPoints;
    public List<Entity> curAttractors;
    public LineRenderer[] uplinks;

    private Camera mainCam;
    //private LineRenderer arc;
    private Entity attractorEntity;
    private Entity curAttractor;
    private EntityManager entityManager;
    private bool isMovingAttractor;
    private int curAttIndex;
    private int curNumUplinks;
    private bool isUpgradingServer;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        mainCam = Camera.main;
        //arc = GetComponent<LineRenderer>();
        curClickPoints = new List<GameObject>();
        curAttractors = new List<Entity>();

        for(int i = 0; i < uplinks.Length; i++)
        {
            uplinks[i].positionCount = lineSegments;
        }
        movePaths = new List<float3[]>();

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        attractorEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(attractorPrefab, settings);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        SpawnDealio(initSpawnPos);
    }

    private void Update()
    {
        if (isUpgradingServer) { return; }
        if (isMovingAttractor)
        {
            int layerMask = 1 << 8;
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                ShowArc(hit.point, uplinks[curAttIndex]);
                Vector3 newPos = DeterminePos(hit.point);
                Quaternion newRot = DetermineRot(hit.point);
                entityManager.SetComponentData(curAttractor, new Translation { Value = newPos });
                entityManager.SetComponentData(curAttractor, new Rotation { Value = newRot });
                movePaths[curAttIndex] = GetAttractionPath(hit.point, uplinks[curAttIndex]);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            int layerMask = LayerMask.GetMask("Earth", "Collector", "LDServer");
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (isMovingAttractor)
                {
                    //test that we can place collector here
                    isMovingAttractor = false;
                    CollectorData curCollectorData = entityManager.GetComponentData<CollectorData>(curAttractor);
                    entityManager.SetComponentData(curAttractor, new CollectorData
                    {
                        canCollect = true,
                        attractorID = curCollectorData.attractorID
                        //attrationPos = GetAttractionPath(hit.point)
                    });
                    GameObject newClickTarget = Instantiate(clickTargetPrefab, hit.point, Quaternion.identity);
                    newClickTarget.GetComponent<ClickTarget>().attractorEnt = curAttractor;
                    float scale = UpgradeController.instance.GetCurScale();
                    newClickTarget.transform.localScale = new Vector3(scale, scale, scale);
                    curClickPoints.Add(newClickTarget);
                    curAttractor = Entity.Null;
                    curAttIndex = -1;
                }
                else if (hit.transform.gameObject.layer == 9) //if hit a collector
                {
                    isMovingAttractor = true;
                    curAttractor = hit.transform.GetComponent<ClickTarget>().attractorEnt;
                    CollectorData curCollectorData = entityManager.GetComponentData<CollectorData>(curAttractor);
                    entityManager.SetComponentData(curAttractor, new CollectorData
                    {
                        canCollect = false,
                        attractorID = curCollectorData.attractorID
                    });
                    curClickPoints.Remove(hit.transform.gameObject);
                    Destroy(hit.transform.gameObject);
                    curAttIndex = curCollectorData.attractorID;
                }
                else if(hit.transform.gameObject.layer == 10) //if hit LDServer
                {
                    //Debug.Log("LDServer");
                    //check if already open
                    //hudContainer.SetActive(false);
                    serverUpgradeContainer.SetActive(true);
                    CameraController.instance.SetShouldFreeze(true);
                    UpgradeController.instance.UpdateUpgradeUI();
                    isUpgradingServer = true;
                }
            }
        }
    }
    
    public void CloseServerUpgrades()
    {
        serverUpgradeContainer.SetActive(false);
        hudContainer.SetActive(true);
        CameraController.instance.SetShouldFreeze(false);
        isUpgradingServer = false;
    }

    public void SpawnNewAtt()
    {
        SpawnDealio(initSpawnPos);
    }

    private void SpawnDealio(Vector3 spawnPos)
    {
        Quaternion selectionRot = DetermineRot(spawnPos);
        Vector3 hitSpawnPoint = DeterminePos(spawnPos);
        ShowArc(spawnPos, uplinks[curNumUplinks]);

        Entity newEnt = entityManager.Instantiate(attractorEntity);
        entityManager.SetComponentData(newEnt, new Translation { Value = hitSpawnPoint });
        entityManager.SetComponentData(newEnt, new Rotation { Value = selectionRot });
        
        entityManager.SetComponentData(newEnt, new CollectorData
        {
            canCollect = true,
            attractorID = curNumUplinks
        });
        float scale = UpgradeController.instance.GetCurScale();
        entityManager.SetComponentData(newEnt, new NonUniformScale { Value = new float3(scale, 5f, scale) });
        GameObject newClickTarget = Instantiate(clickTargetPrefab, hitSpawnPoint, Quaternion.identity);
        newClickTarget.GetComponent<ClickTarget>().attractorEnt = newEnt;
        newClickTarget.transform.localScale = new Vector3(scale, scale, scale);
        curClickPoints.Add(newClickTarget);
        curAttractors.Add(newEnt);
        movePaths.Add(GetAttractionPath(spawnPos, uplinks[curNumUplinks]));

        curNumUplinks++;
    }

    private Vector3 DeterminePos(Vector3 initPos)
    {
        Vector3 postition = initPos - (((earth.position - initPos).normalized) * hitOffset);
        return postition;
    }

    private Quaternion DetermineRot(Vector3 pos)
    {
        Vector3 selectionLeft = Vector3.Cross(pos - earth.position, Vector3.up);
        Vector3 selectionFwd = Vector3.Cross(pos - earth.position, selectionLeft);
        Quaternion selectionRot = Quaternion.LookRotation(selectionFwd);
        return selectionRot;
    }

    private void ShowArc(Vector3 startPoint, LineRenderer uplink)
    {
        for (int i = 0; i < uplink.positionCount; i++)
        {
            float slerpPct = i / (lineSegments - 1f);

            float heightMod = sinMod * Mathf.Sin(slerpPct * Mathf.PI);

            Vector3 arcPointPos = Vector3.Slerp(startPoint, ldMainServer.position, slerpPct);
            arcPointPos = arcPointPos + (arcPointPos.normalized * heightMod * maxLineHeight);
            uplink.SetPosition(i, arcPointPos);
        }
    }

    public float3[] GetAttractionPath(Vector3 startPos, LineRenderer uplink)
    {
        float3[] attractorPos = new float3[uplink.positionCount + 1];
        attractorPos[0] = startPos;
        for (int i = 1; i < attractorPos.Length; i++)
        {
            attractorPos[i] = uplink.GetPosition(i - 1);
        }
        return attractorPos;
    }

    public void GrowAttractors(float scale)
    {
        float3 newEntScale = new float3(scale, 5f, scale);

        foreach(Entity ent in curAttractors)
        {
            entityManager.SetComponentData(ent, new NonUniformScale { Value = newEntScale });
        }

        Vector3 newGOScale = new Vector3(scale, scale, scale);

        foreach(GameObject clicker in curClickPoints)
        {
            clicker.transform.localScale = newGOScale;
        }
    }
}
