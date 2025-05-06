using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK {
    [RequireComponent(typeof(Camera))]
    public class RetargetingCamera : MonoBehaviour
    {
        public PassthroughHandler Passthrough;
        
        [SerializeField]
        Camera cam;
        [SerializeField]
        Visibility _renderingVisibility;
        
        public Visibility RenderingVisibility {
            get {
                return _renderingVisibility;
            }
            set {
                if (cam == null) GetComponent<Camera>();
                _renderingVisibility = value;
                int trackedLayerIndex = LayerMask.NameToLayer(Constants.TrackedEnvironmentLayer);
                int virtualLayerIndex = LayerMask.NameToLayer(Constants.VirtualEnvironmentLayer);
                
                if (LayerInMask(cam.cullingMask, trackedLayerIndex))  cam.cullingMask = cam.cullingMask & ~(1 << trackedLayerIndex);
                if (LayerInMask(cam.cullingMask, virtualLayerIndex))  cam.cullingMask = cam.cullingMask & ~(1 << virtualLayerIndex);

                switch(_renderingVisibility) {
                    case Visibility.Tracked:
                        cam.cullingMask = cam.cullingMask | (1 << trackedLayerIndex);
                    break;

                    case Visibility.Virtual:
                        cam.cullingMask = cam.cullingMask | (1 << virtualLayerIndex);
                    break;

                    case Visibility.Both:
                        cam.cullingMask = cam.cullingMask | (1 << trackedLayerIndex)  | (1 << virtualLayerIndex);
                    break;

                    case Visibility.None:
                    break;

                }
            }
        }

        private void OnValidate() {
            if (cam == null) cam = GetComponent<Camera>();
            RenderingVisibility = _renderingVisibility;
        }

        bool LayerInMask(int mask, int layer) {
            return mask == (mask | (1 << layer));
        }

        public void SetVisibility(int visibility) {
            SetVisibility((Visibility) visibility);
        }

        public void SetVisibility(Visibility visibility) {
            RenderingVisibility = visibility;
        }
    }
}
