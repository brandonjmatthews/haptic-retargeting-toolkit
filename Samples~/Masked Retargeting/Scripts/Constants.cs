/*
 * HRTK: Constants.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using UnityEngine;

namespace HRTK.MaskedRetargeting
{
    public class Constants
    {

        public static string MaskLayer => "Mask";
        public static string LeftCaptureLayer => "LeftCapture";
        public static string LeftOutputLayer => "LeftOutput";
        // public static string RightCaptureLayer => "RightCapture";
        public static string RightCaptureLayer => "RightCapture";
        public static string RightOutputLayer => "RightOutput";

        public static string CaptureLayer(Camera.MonoOrStereoscopicEye eye) {
            return eye == Camera.MonoOrStereoscopicEye.Left ? LeftCaptureLayer : RightCaptureLayer;
        }

        public static string OutputLayer(Camera.MonoOrStereoscopicEye eye) {
            return eye == Camera.MonoOrStereoscopicEye.Left ? LeftOutputLayer : RightOutputLayer;
        }
    }
}
