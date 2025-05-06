using System.Collections;
using System.Collections.Generic;
using Leap.Unity;
using UnityEngine;

namespace HRTK.LeapMotion {
    public class LeapPassthrough : MonoBehaviour
    {
        public bool showPassthrough;

        public float distance = 0.01f;
        public GameObject imageQuad;

        public bool fillCamera;
        public Camera targetCamera;

        public bool dislocate;
        // public LeapEyeDislocator dislocator;

        private void Awake()
        {
            // dislocator = GetComponent<LeapEyeDislocator>();
        }

        private void Start() {
            SetPassthroughVisible(showPassthrough);
        }

        public void Show()
        {
            SetPassthroughVisible(true);
        }

        public void Hide()
        {
            SetPassthroughVisible(false);
        }

        public void SetPassthroughVisible(bool visible)
        {
            showPassthrough = visible;
            imageQuad.SetActive(visible);

            // if (dislocator != null && dislocator) {
            //     dislocator.enabled = visible;
            // }
        }

        void Update()
        {
            if (showPassthrough && fillCamera) FillCamera();
        }

        void FillCamera()
        {
            float pos = (targetCamera.nearClipPlane + distance);

            imageQuad.transform.position = targetCamera.transform.position + targetCamera.transform.forward * pos;

            float h = Mathf.Tan(targetCamera.fieldOfView * Mathf.Deg2Rad * 0.5f) * pos * 2f;

            imageQuad.transform.localScale = new Vector3(h * targetCamera.aspect * 2, h, 1f);
        }
    }
}