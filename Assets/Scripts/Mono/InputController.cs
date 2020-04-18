using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public GameObject LDMainServer;
    public GameObject hitPrefab;
    public int lineSegments;

    private Camera mainCam;
    private LineRenderer arc;

    private void Start()
    {
        mainCam = Camera.main;
        arc = GetComponent<LineRenderer>();
        arc.positionCount = lineSegments;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                Instantiate(hitPrefab, hit.point, Quaternion.identity);
                for(int i = 0; i < arc.positionCount; i++)
                {
                    //float slerpPct = 
                    Vector3 arcPointPos = Vector3.Slerp(hit.point, LDMainServer.transform.position, i / (lineSegments - 1f));
                    arc.SetPosition(i, arcPointPos);
                }
            }
        }
    }
}
