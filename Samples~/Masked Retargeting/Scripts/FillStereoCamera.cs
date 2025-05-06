using UnityEngine;

namespace HRTK.MaskedRetargeting
{
    [ExecuteInEditMode]
    public class FillStereoCamera : MonoBehaviour
    {
        public Camera cam;
        public float distance = 0.01f;
        public float excessSize = 0f;
        //  public Camera.MonoOrStereoscopicEye eye;
        void Update()
        {
            if (cam == null) return;
            float pos = (cam.nearClipPlane + distance);

            Vector3 sep = Vector3.zero;
            float sepAmount = cam.stereoSeparation / 2.0f;
            if (cam.stereoTargetEye == StereoTargetEyeMask.Left) sep = cam.transform.right * (-1 * sepAmount);
            if (cam.stereoTargetEye == StereoTargetEyeMask.Right) sep = cam.transform.right * sepAmount;

            transform.position = cam.transform.position + (cam.transform.forward * pos) + sep;

            float h = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f + excessSize;

            transform.localScale = new Vector3(h * cam.aspect, h, 1f);
        }
    }
}