using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceAtMidpoint : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    // Start is called before the first frame update
    void Start()
    {
        if (pointA == null || pointB == null) this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
        transform.position = (pointA.position + pointB.position) / 2.0f;
    }
}
