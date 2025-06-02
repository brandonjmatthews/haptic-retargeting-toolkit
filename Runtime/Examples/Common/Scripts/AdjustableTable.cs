/*
 * HRTK: AdjustableTable.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustableTable : MonoBehaviour
{
    public Transform heightTransform;
    public Transform tabletop;
    public Transform legs;
    public float minHeight;
    public float maxHeight;

    // public float currentHeight;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SetHeightWorld(heightTransform.position.y);
        // SetHeightWorld(currentHeight);        
    }

    // Sets the height given a Y value in world space
    public void SetHeightWorld(float height) {
        float clampHeight = Mathf.Clamp(height, minHeight, maxHeight);
        // Set tabletop position
        Vector3 newPos = new Vector3(tabletop.position.x, clampHeight, tabletop.position.z);
        tabletop.position = newPos;
        // Scale legs
        Vector3 legScale = new Vector3(legs.localScale.x, clampHeight - 0.2f, legs.localScale.z);
        legs.localScale = legScale;
    }
}
