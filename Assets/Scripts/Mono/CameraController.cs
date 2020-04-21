using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public Transform earth;

    public float horizontalPct, verticalPct;
    public float minCamDist, maxCamDist;
    public float rotSpeed;
    public float zoomSpeed;

    float leftZone;
    float rightZone;
    float botZone;
    float topZone;
    bool shouldFreeze = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        leftZone = horizontalPct * Screen.width;
        rightZone = Screen.width - (horizontalPct * Screen.width);
        botZone = verticalPct * Screen.height;
        topZone = Screen.height - (verticalPct * Screen.height);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            shouldFreeze = !shouldFreeze;
        }

        if (shouldFreeze)
        {
            return;
        }

        if (Input.mousePosition.x <= leftZone)
        {
            //Debug.Log("Go Left " + leftZone);
            transform.position = Quaternion.AngleAxis(rotSpeed * Time.deltaTime, Vector3.up) * (transform.position - earth.position) + earth.position;
        }
        else if (Input.mousePosition.x >= rightZone)
        {
            //Debug.Log("Go Right " + rightZone);
            transform.position = Quaternion.AngleAxis(rotSpeed * Time.deltaTime, Vector3.down) * (transform.position - earth.position) + earth.position;
        }

        Vector3 camVertAxis = Vector3.Cross((earth.position - transform.position), Vector3.up);

        if (Input.mousePosition.y <= botZone)
        {
            //Debug.Log("Go Bot " + botZone);
            transform.position = Quaternion.AngleAxis(rotSpeed * Time.deltaTime, camVertAxis) * (transform.position - earth.position) + earth.position;
        }
        else if (Input.mousePosition.y >= topZone)
        {
            //Debug.Log("Go Top " + topZone);
            transform.position = Quaternion.AngleAxis(rotSpeed * Time.deltaTime, camVertAxis * -1) * (transform.position - earth.position) + earth.position;
        }

        transform.LookAt(earth);

        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            //Debug.Log("above zero");
            Vector3 zoomInTarget = Vector3.ClampMagnitude(transform.position - earth.position, minCamDist);
            transform.position = Vector3.MoveTowards(transform.position, zoomInTarget, Time.deltaTime * zoomSpeed);
            if (Vector3.Distance(transform.position, earth.position) <= minCamDist)
            {
                transform.position = zoomInTarget;
            }
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            //Debug.Log("sub zero");
            Vector3 zoomOutTarget = Vector3.Normalize(transform.position - earth.position) * maxCamDist;
            //Debug.Log(zoomOutTarget.ToString());
            transform.position = Vector3.MoveTowards(transform.position, zoomOutTarget, Time.deltaTime * zoomSpeed);
            if (Vector3.Distance(transform.position, earth.position) >= maxCamDist)
            {
                //transform.position = zoomOutTarget;
            }
        }
    }

    public void SetShouldFreeze(bool freezeGame)
    {
        shouldFreeze = freezeGame;
    }
}
