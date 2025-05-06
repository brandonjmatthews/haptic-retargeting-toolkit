using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK.MaskedRetargeting {
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class Metaball : MonoBehaviour
    {   
        // Metaball Settings
        [Range(0.001f, 1.0f)]
        public float alphaCutoff = 0.1f;
        public Color cutoffColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            // Metaball Settings
        [Range(0.001f, 1.0f)]
        public float fadeOutRange = 0.02f;

        public Shader metaballShader;
        Material metaballMaterial;

        private void Start() {
            if (metaballShader) {
                metaballMaterial = new Material(metaballShader);
            }
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest) {
            if (Application.isPlaying) {
                metaballMaterial.SetFloat("_Cutoff", alphaCutoff);
                metaballMaterial.SetColor("_Color", cutoffColor);
                metaballMaterial.SetFloat("_Fade", fadeOutRange);
                Vector2 size = new Vector2(src.width, src.height);
                metaballMaterial.SetVector("_MainTex_Size", size);
                Graphics.Blit(src, dest, metaballMaterial);
            }
        }
    }
}
