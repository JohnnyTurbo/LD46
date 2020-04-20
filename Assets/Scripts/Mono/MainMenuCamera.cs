using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    public Transform earth;
   
    public float rotSpeed;

    void Update()
    {      
        transform.position = Quaternion.AngleAxis(rotSpeed * Time.deltaTime, Vector3.up) * (transform.position - earth.position) + earth.position;   

        transform.LookAt(earth);        
    }
}
