using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK.MaskedRetargeting
{
    public class MaskRig : MonoBehaviour
    {
        [SerializeField] Camera.MonoOrStereoscopicEye eye;
        [SerializeField] Camera eyeCamera;
        [SerializeField] Camera maskCamera;
        [SerializeField] GameObject maskQuad;
        // MaskRenderCamera renderCamera;

        [SerializeField] MeshRenderer outputQuad;
        [SerializeField] MeshRenderer outlineQuad;

        RenderTexture outputTexture;

        private void Start()
        {
            FillStereoCamera fill = maskQuad.GetComponent<FillStereoCamera>();
            fill.cam = eyeCamera;

            int maskLayer = LayerMask.NameToLayer(Constants.MaskLayer);
            maskCamera.cullingMask = (1 << maskLayer);

            int renderLayer = LayerMask.NameToLayer(Constants.OutputLayer(eye));
            maskQuad.layer = renderLayer;

            int dontRenderLayer = LayerMask.NameToLayer(eye == Camera.MonoOrStereoscopicEye.Left ? Constants.RightOutputLayer : Constants.LeftOutputLayer);
            eyeCamera.cullingMask = eyeCamera.cullingMask &= ~(1 << dontRenderLayer);

            int cullingMask = maskCamera.cullingMask;

            maskCamera.CopyFrom(eyeCamera);
            maskCamera.cullingMask = cullingMask;
            maskCamera.clearFlags = CameraClearFlags.SolidColor;
            maskCamera.backgroundColor = Color.black;

            maskCamera.transform.localPosition = Vector3.zero;
            maskCamera.transform.localEulerAngles = Vector3.zero;

            if (outputTexture == null)
            {
                outputTexture = RenderTexture.GetTemporary(maskCamera.pixelWidth, maskCamera.pixelHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 4);
                maskCamera.targetTexture = outputTexture;

                if (outputQuad)
                {
                    outputQuad.sharedMaterial.SetTexture("_MainTex", outputTexture);
                }

                if (outlineQuad)
                {
                    outlineQuad.sharedMaterial.SetTexture("_MainTex", outputTexture);
                }
            }
        }
    }
}