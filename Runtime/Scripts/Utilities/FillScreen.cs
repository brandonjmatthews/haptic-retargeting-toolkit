/*
 * HRTK: FillScreen.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK {

    public class FillScreen : MonoBehaviour
    {
        public Camera cam;
        public float distance = 0.01f;

        void Update()
        {

            float pos = (cam.nearClipPlane + distance);

            transform.position = cam.transform.position + cam.transform.forward * pos;

            float h = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f;

            transform.localScale = new Vector3(h * cam.aspect, h, 1f);
        }
    }
}
