/*
 * HRTK: WhiteOnly.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HTRK.MaskedRetargeting {
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(301)]
    public class WhiteOnly : MonoBehaviour
    {        
        [SerializeField] Shader whiteShader;
        Camera cam;

        private void Start() {
            cam = GetComponent<Camera>();
            if (whiteShader)
            {
                // cam.SetReplacementShader(whiteShader, "RenderType");
            }
        }
    }
}
