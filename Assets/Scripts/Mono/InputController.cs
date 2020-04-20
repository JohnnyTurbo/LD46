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
    public Transform earth;
    public Transform ldMainServer;
    public int lineSegments;
    public float hitOffset;
    public float attractRadius;
    public float maxLineHeight;
    public float sinMod;
    public Vector3 initSpawnPos;
    public List<float3[]> movePaths;

    private Camera mainCam;
    private LineRenderer arc;
    private Entity attractorEntity;
    private Entity curAttractor;
    private EntityManager entityManager;
    private bool isMovingAttractor;
    private int curAttIndex;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        mainCam = Camera.main;
        arc = GetComponent<LineRenderer>();
        arc.positionCount = lineSegments;
        movePaths = new List<float3[]>();

        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        attractorEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(attractorPrefab, settings);
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        SpawnDealio(initSpawnPos);
    }

    private void Update()
    {

        if (isMovingAttractor)
        {
            int layerMask = 1 << 8;
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                ShowArc(hit.point);
                Vector3 newPos = DeterminePos(hit.point);
                Quaternion newRot = DetermineRot(hit.point);
                entityManager.SetComponentData(curAttractor, new Translation { Value = newPos });
                entityManager.SetComponentData(curAttractor, new Rotation { Value = newRot });
                movePaths[0] = GetAttractionPath(hit.point);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            int layerMask = LayerMask.GetMask("Earth", "Collector");
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
                    curAttractor = Entity.Null;
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
                    Destroy(hit.transform.gameObject);
                }
            }
        }
    }

    private void SpawnDealio(Vector3 spawnPos)
    {
        Quaternion selectionRot = DetermineRot(spawnPos);
        Vector3 hitSpawnPoint = DeterminePos(spawnPos);
        ShowArc(spawnPos);

        Entity newEnt = entityManager.Instantiate(attractorEntity);
        entityManager.SetComponentData(newEnt, new Translation { Value = hitSpawnPoint });
        entityManager.SetComponentData(newEnt, new Rotation { Value = selectionRot });
        
        entityManager.SetComponentData(newEnt, new CollectorData
        {
            canCollect = true
        });

        GameObject newClickTarget = Instantiate(clickTargetPrefab, hitSpawnPoint, Quaternion.identity);
        newClickTarget.GetComponent<ClickTarget>().attractorEnt = newEnt;
        movePaths.Add(GetAttractionPath(spawnPos));
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

    private void ShowArc(Vector3 startPoint)
    {
        for (int i = 0; i < arc.positionCount; i++)
        {
            float slerpPct = i / (lineSegments - 1f);

            float heightMod = sinMod * Mathf.Sin(slerpPct * Mathf.PI);

            Vector3 arcPointPos = Vector3.Slerp(startPoint, ldMainServer.position, slerpPct);
            arcPointPos = arcPointPos + (arcPointPos.normalized * heightMod * maxLineHeight);
            arc.SetPosition(i, arcPointPos);
        }
    }

    public float3[] GetAttractionPath(Vector3 startPos)
    {
        float3[] attractorPos = new float3[arc.positionCount + 1];
        attractorPos[0] = startPos;
        for (int i = 1; i < attractorPos.Length; i++)
        {
            attractorPos[i] = arc.GetPosition(i - 1);
        }
        return attractorPos;
    }
}
