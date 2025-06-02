/*
 * HRTK: Inpainting.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;
using System.IO;
using System;

namespace HRTK.MaskedRetargeting
{

    [System.Serializable]
    struct RenderTextureSet
    {
        public RenderTexture imageTexture;
        public RenderTexture maskTexture;
        public RenderTexture maskedImageTexture;
        public RenderTexture outputTexture;

        public RenderTexture[] mipmaps;
        public RenderTexture[] upscaled;

        public RenderTextureSet(Vector2Int dimensions, int iterations)
        {
            imageTexture = RenderTexture.GetTemporary(dimensions.x, dimensions.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
            // imageTexture = RenderTexture.GetTemporary(XRSettings.eyeTextureDesc);
            imageTexture.vrUsage = VRTextureUsage.None;
            // imageCamera.targetTexture = imageTexture;

            maskTexture = RenderTexture.GetTemporary(dimensions.x, dimensions.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
            // maskTexture = RenderTexture.GetTemporary(XRSettings.eyeTextureDesc);
            maskTexture.vrUsage = VRTextureUsage.None;
            maskTexture.filterMode = FilterMode.Point;

            maskedImageTexture = RenderTexture.GetTemporary(dimensions.x, dimensions.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
            // maskedImageTexture = RenderTexture.GetTemporary(XRSettings.eyeTextureDesc);
            maskedImageTexture.vrUsage = VRTextureUsage.None;
            maskedImageTexture.filterMode = FilterMode.Point;

            outputTexture = RenderTexture.GetTemporary(dimensions.x, dimensions.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
            // outputTexture = RenderTexture.GetTemporary(XRSettings.eyeTextureDesc);
            outputTexture.vrUsage = VRTextureUsage.None;
            outputTexture.filterMode = FilterMode.Bilinear;

            // Configure mipmapping
            mipmaps = new RenderTexture[iterations];
            upscaled = new RenderTexture[iterations];
        }
    }

    // [RequireComponent(typeof(Camera))]
    [DefaultExecutionOrder(300)]
    public class Inpainting : MonoBehaviour
    {

        public bool InpaintingEnabled;
        public Vector2Int dimensions;
        public bool singleEye;
        [SerializeField] Camera.MonoOrStereoscopicEye eye;
        [SerializeField] Camera baseCamera;
        [SerializeField] SourceCamera maskCamera;
        [SerializeField] SourceCamera imageCamera;
        [SerializeField] Camera outputCamera;


        [Range(1, 10)][SerializeField] int iterations = 5;

        [SerializeField] Shader mergeShader;
        Material mergeMaterial;
        [SerializeField] Shader downsampleShader;
        Material downsampleMaterial;
        [SerializeField] Shader upscaleShader;
        Material upscaleMaterial;
        TrackedPoseDriver driver;
        [SerializeField] MeshRenderer imageRenderer;
        [SerializeField] MeshRenderer outputRenderer;

        [SerializeField] RenderTextureSet leftTextures;
        [SerializeField] RenderTextureSet rightTextures;

        bool maskRendered, imageRendered;
        bool loaded;

        private void Start()
        {
            /*
            Render Order
            Mask = -4 (First)
            Capture = -3 (Second)
            Output = -2 (Third)
            Base = -1 (Last) 
            */

            mergeMaterial = new Material(mergeShader);
            downsampleMaterial = new Material(downsampleShader);
            upscaleMaterial = new Material(upscaleShader);

            int baseLayerMask = baseCamera.cullingMask & ~(1 << LayerMask.NameToLayer(Constants.CaptureLayer(eye)));
            baseLayerMask = baseLayerMask & ~(1 << LayerMask.NameToLayer(Constants.OutputLayer(eye)));
            baseCamera.clearFlags = CameraClearFlags.Depth;
            baseCamera.cullingMask = baseLayerMask;

            // Configure mask camera
            // maskCamera.CopyFrom(baseCamera);
            // int maskLayer = LayerMask.NameToLayer(Constants.MaskLayer);
            // maskCamera.cullingMask = (1 << maskLayer);
            // maskCamera.clearFlags = CameraClearFlags.SolidColor;
            // maskCamera.backgroundColor = Color.clear;
            // maskCamera.depth = baseCamera.depth - 3;
            ConfigureCamera(maskCamera.Camera, CameraClearFlags.SolidColor, Color.clear, Constants.MaskLayer, baseCamera.depth - 3);
            maskCamera.onRenderImage = OnRenderMask;

            // Configure image camera
            // imageCamera.CopyFrom(baseCamera);
            // imageCamera.clearFlags = CameraClearFlags.SolidColor;
            // imageCamera.backgroundColor = Color.black;
            // int imageLayer = LayerMask.NameToLayer(Constants.CaptureLayer(eye));
            // imageCamera.cullingMask = (1 << imageLayer);
            // imageCamera.depth = baseCamera.depth - 2;
            ConfigureCamera(imageCamera.Camera, CameraClearFlags.SolidColor, Color.clear, Constants.CaptureLayer(eye), baseCamera.depth - 2);
            imageCamera.onRenderImage = OnRenderCapture;

            // // Configure output camera
            // outputCamera.CopyFrom(baseCamera);
            // outputCamera.clearFlags = CameraClearFlags.SolidColor;
            // outputCamera.backgroundColor = Color.black;
            // int outputLayer = LayerMask.NameToLayer(Constants.OutputLayer(eye));
            // outputCamera.cullingMask = (1 << outputLayer);
            // imageCamera.depth = baseCamera.depth - 1;
            ConfigureCamera(outputCamera, CameraClearFlags.SolidColor, Color.clear, Constants.OutputLayer(eye), baseCamera.depth - 1);
            // outputCamera.onPreRender = OnPreRenderOutput;

            // maskTexture = RenderTexture.GetTemporary(maskCamera.pixelWidth, maskCamera.pixelHeight, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
            // maskTexture.filterMode = FilterMode.Point;
            // maskCamera.targetTexture = maskTexture;

            // Configure render textures


            if (singleEye) {
                if (eye == Camera.MonoOrStereoscopicEye.Left) {
                    leftTextures = new RenderTextureSet(dimensions, iterations);
                    outputRenderer.material.SetTexture("_LeftTex", leftTextures.outputTexture);
                }
                else if (eye == Camera.MonoOrStereoscopicEye.Right) {
                    rightTextures = new RenderTextureSet(dimensions, iterations);
                    outputRenderer.material.SetTexture("_RightTex", rightTextures.outputTexture);
                }
            } else {
                leftTextures = new RenderTextureSet(dimensions, iterations);
                rightTextures = new RenderTextureSet(dimensions, iterations);
                outputRenderer.material.SetTexture("_LeftTex", leftTextures.outputTexture);
                outputRenderer.material.SetTexture("_RightTex", rightTextures.outputTexture);
            }
            // outputRenderer.material.mainTexture = leftTextures.outputTexture;

            // Configure renderers
            imageRenderer.gameObject.layer = LayerMask.NameToLayer(Constants.CaptureLayer(eye));
            imageRenderer.GetComponent<FillStereoCamera>().cam = imageCamera.Camera;

            outputRenderer.gameObject.layer = LayerMask.NameToLayer(Constants.OutputLayer(eye));
            outputRenderer.GetComponent<FillStereoCamera>().cam = outputCamera;

            loaded = true;


            StartCoroutine(SetWidthHeight());
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                string filename = Path.Combine(Application.persistentDataPath, string.Format("image_{0}.png", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")));
                SaveTexture.SaveRenderTextureToFile(leftTextures.imageTexture, filename);
            }

            maskCamera.Camera.projectionMatrix = baseCamera.projectionMatrix;
            imageCamera.Camera.projectionMatrix = baseCamera.projectionMatrix;
            outputCamera.projectionMatrix = baseCamera.projectionMatrix;
        }

        void ConfigureCamera(Camera cam, CameraClearFlags clearFlags, Color background, string layer, float depth)
        {
            // Configure output camera
            cam.CopyFrom(baseCamera);
            cam.clearFlags = clearFlags;
            cam.backgroundColor = background;
            int renderLayer = LayerMask.NameToLayer(layer);
            cam.cullingMask = (1 << renderLayer);
            cam.depth = depth;
        }

        IEnumerator SetWidthHeight()
        {
            while (XRSettings.eyeTextureWidth == 0)
                yield return new WaitForEndOfFrame();
            Debug.Log($"w: {XRSettings.eyeTextureWidth} h: {XRSettings.eyeTextureHeight}");
            Screen.SetResolution(XRSettings.eyeTextureWidth, XRSettings.eyeTextureHeight, false);
        }

        private void OnRenderMask(RenderTexture src, RenderTexture dest)
        {
            RenderTextureSet textureSet = maskCamera.Camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left ?
                                                                    leftTextures : rightTextures;
            Graphics.Blit(src, textureSet.maskTexture);
        }

        private void OnRenderCapture(RenderTexture src, RenderTexture dest)
        {
            RenderTextureSet textureSet = imageCamera.Camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left ?
                                                                    leftTextures : rightTextures;
            Graphics.Blit(src, textureSet.imageTexture);

            mergeMaterial.SetTexture("_MaskTex", textureSet.maskTexture);
            Graphics.Blit(textureSet.imageTexture, textureSet.maskedImageTexture, mergeMaterial);


            Inpaint(textureSet);
        }

        private void Inpaint(RenderTextureSet textureSet)
        {
            if (!InpaintingEnabled)
            {
                // Blit source to output
                Graphics.Blit(textureSet.imageTexture, textureSet.outputTexture);
                return;
            }
            // Downscale output
            int rtW = textureSet.maskedImageTexture.width / 2;
            int rtH = textureSet.maskedImageTexture.height / 2;

            textureSet.mipmaps[0] = textureSet.maskedImageTexture;

            for (int i = 1; i < iterations; i++)
            {
                if (textureSet.mipmaps[i] == null)
                {
                    RenderTextureDescriptor descriptor = XRSettings.eyeTextureDesc;
                    descriptor.width = rtW;
                    descriptor.height = rtH;
                    textureSet.mipmaps[i] = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
                    // textureSet.mipmaps[i] = RenderTexture.GetTemporary(descriptor);
                    textureSet.mipmaps[i].vrUsage = VRTextureUsage.None;
                    // mipmaps[i] = new RenderTexture(rtW, rtH, 0, RenderTextureFormat.ARGB32);
                    textureSet.mipmaps[i].filterMode = FilterMode.Point;
                }

                Graphics.Blit(textureSet.mipmaps[i - 1], textureSet.mipmaps[i], downsampleMaterial, 0);
                rtW = rtW / 2;
                rtH = rtH / 2;
            }

            for (int i = iterations - 2; i >= 0; i--)
            {
                // Fill map is the map that mask pixels are being filled
                RenderTexture fillMap = textureSet.mipmaps[i];
                int fillW = fillMap.width;
                int fillH = fillMap.height;

                // Source map is the smaller mipmap that is being used to fill 'fillMap' 
                RenderTexture srcMap = textureSet.mipmaps[i + 1];

                if (textureSet.upscaled[i + 1] == null)
                {
                    RenderTextureDescriptor descriptor = XRSettings.eyeTextureDesc;
                    descriptor.width = fillW;
                    descriptor.height = fillH;
                    textureSet.upscaled[i + 1] = RenderTexture.GetTemporary(fillW, fillH, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
                    textureSet.upscaled[i + 1].vrUsage = VRTextureUsage.None;
                    // textureSet.upscaled[i + 1] = RenderTexture.GetTemporary(descriptor);
                    // textureSet.upscaled[i + 1] = new RenderTexture(fillW, fillH, 0, RenderTextureFormat.ARGB32);
                }
                RenderTexture dstMap = textureSet.upscaled[i + 1];

                upscaleMaterial.SetTexture("_FillTex", fillMap);
                // Write to dstMap so fillMap can be passed as an argument
                Graphics.Blit(srcMap, dstMap, upscaleMaterial);
                Graphics.Blit(dstMap, fillMap);
            }

            // Blit result to output
            Graphics.Blit(textureSet.upscaled[1], textureSet.outputTexture);
            // Graphics.Blit(textureSet.maskTexture, textureSet.outputTexture);
        }

        // private void OnPreRenderOutput(Camera cam) {
        //     Debug.Log(Time.frameCount + " output pre frame " + outputCamera.Camera.stereoActiveEye);
        //     if (cam.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left) {
        //         outputRenderer.material.mainTexture = leftTextures.outputTexture;
        //     } else {
        //         outputRenderer.material.mainTexture = rightTextures.outputTexture;
        //     }
            
        // }

        // private void OnRenderImage(RenderTexture src, RenderTexture dest)
        // {
        // if (Application.isPlaying && loaded)
        // {
        //     Debug.Log(baseCamera.stereoActiveEye);
        //     // Merge mask
        //     mergeMaterial.SetTexture("_MaskTex", maskTexture);
        //     Graphics.Blit(src, maskedImageTexture, mergeMaterial);

        //     // Downscale output
        //     int rtW = maskedImageTexture.width / 2;
        //     int rtH = maskedImageTexture.height / 2;

        //     mipmaps[0] = maskedImageTexture;

        //     for (int i = 1; i < iterations; i++)
        //     {
        //         if (mipmaps[i] == null)
        //         {
        //             RenderTextureDescriptor descriptor = XRSettings.eyeTextureDesc;
        //             descriptor.width = rtW;
        //             descriptor.height = rtH;
        //             mipmaps[i] = RenderTexture.GetTemporary(rtW, rtH, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
        //             // mipmaps[i] = RenderTexture.GetTemporary(descriptor);
        //             // mipmaps[i] = new RenderTexture(rtW, rtH, 0, RenderTextureFormat.ARGB32);
        //             mipmaps[i].filterMode = FilterMode.Point;
        //         }

        //         Graphics.Blit(mipmaps[i - 1], mipmaps[i], downsampleMaterial, 0);
        //         rtW = rtW / 2;
        //         rtH = rtH / 2;
        //     }

        //     for (int i = iterations - 2; i >= 0; i--)
        //     {
        //         // Fill map is the map that mask pixels are being filled
        //         RenderTexture fillMap = mipmaps[i];
        //         int fillW = fillMap.width;
        //         int fillH = fillMap.height;

        //         // Source map is the smaller mipmap that is being used to fill 'fillMap' 
        //         RenderTexture srcMap = mipmaps[i + 1];

        //         if (upscaled[i + 1] == null)
        //         {
        //             RenderTextureDescriptor descriptor = XRSettings.eyeTextureDesc;
        //             descriptor.width = fillW;
        //             descriptor.height = fillH;
        //             upscaled[i + 1] = RenderTexture.GetTemporary(fillW, fillH, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
        //             // upscaled[i + 1] = RenderTexture.GetTemporary(descriptor);
        //             // upscaled[i + 1] = new RenderTexture(fillW, fillH, 0, RenderTextureFormat.ARGB32);
        //         }
        //         RenderTexture dstMap = upscaled[i + 1];

        //         upscaleMaterial.SetTexture("_FillTex", fillMap);
        //         // Write to dstMap so fillMap can be passed as an argument
        //         Graphics.Blit(srcMap, dstMap, upscaleMaterial);
        //         Graphics.Blit(dstMap, fillMap);
        //     }

        //     if (InpaintingEnabled)
        //     {
        //         // Blit result to output
        //         Graphics.Blit(upscaled[1], outputTexture);
        //     }
        //     else
        //     {
        //         // Blit source to output
        //         Graphics.Blit(src, outputTexture);
        //     }

        //     Graphics.Blit(src, dest);
        // }
        // else
        // {
        //     Graphics.Blit(src, dest);
        // }
        // }
    }
}