#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HRTK
{
    public class RetargetingHandRig : MonoBehaviour
    {
        public GameObject LeftHandPrefab;
        public GameObject RightHandPrefab;
        public GameObject OriginPrefab;

        protected RetargetingController leftHandController;
        protected RetargetingController rightHandController;

        public virtual void BuildRig()
        {
            if (leftHandController == null && LeftHandPrefab != null)
            {
                leftHandController = BuildHand(LeftHandPrefab, Chirality.Left);
                leftHandController.gameObject.name = "Left Retargeting Hand";

                if (OriginPrefab != null)
                {
                    GameObject originPrefab = PrefabUtility.InstantiatePrefab(OriginPrefab, leftHandController.transform) as GameObject;
                    leftHandController.Origin = originPrefab.GetComponent<RetargetingOrigin>();
                    leftHandController.Origin.gameObject.name = "Left Hand Origin";
                }
            }

            if (rightHandController == null && RightHandPrefab != null)
            {
                rightHandController = BuildHand(RightHandPrefab, Chirality.Right);
                rightHandController.gameObject.name = "Right Retargeting Hand";

                if (OriginPrefab != null)
                {
                    GameObject originPrefab = PrefabUtility.InstantiatePrefab(OriginPrefab, rightHandController.transform) as GameObject;
                    rightHandController.Origin = originPrefab.GetComponent<RetargetingOrigin>();
                    rightHandController.Origin.gameObject.name = "Right Hand Origin";
                }
            }


        }

        protected RetargetingController BuildHand(GameObject handPrefab, Chirality whichHand)
        {
            GameObject handRoot = new GameObject();
            handRoot.transform.parent = this.transform;
            handRoot.transform.localPosition = Vector3.zero;
            handRoot.transform.localRotation = Quaternion.identity;
            RetargetingController controller = handRoot.AddComponent<RetargetingController>();
            controller.whichHand = whichHand;

            GameObject trackedHandObject = PrefabUtility.InstantiatePrefab(handPrefab, handRoot.transform) as GameObject;
            RetargetingHand trackedHand = trackedHandObject.GetComponent<RetargetingHand>();
            if (trackedHand != null)
            {
                trackedHand.HandType = HandType.Tracked;
                trackedHand.gameObject.name = handPrefab.name + " (Tracked)";
                trackedHand.transform.localPosition = Vector3.zero;
                trackedHand.transform.localRotation = Quaternion.identity;
                trackedHand.gameObject.layer = LayerMask.NameToLayer(Constants.TrackedEnvironmentLayer);
            }

            GameObject virtualHandObject = PrefabUtility.InstantiatePrefab(handPrefab, handRoot.transform) as GameObject;
            RetargetingHand virtualHand = virtualHandObject.GetComponent<RetargetingHand>();

            if (virtualHand != null)
            {
                virtualHand.HandType = HandType.Virtual;
                virtualHand.gameObject.name = handPrefab.name + " (Virtual)";
                virtualHand.transform.localPosition = Vector3.zero;
                virtualHand.transform.localRotation = Quaternion.identity;
                virtualHand.gameObject.layer = LayerMask.NameToLayer(Constants.VirtualEnvironmentLayer);
            }

            controller.TrackedHand = trackedHand;
            controller.VirtualHand = virtualHand;

            return controller;
        }

        public void ClearRig() {
            if (leftHandController != null) {
                DestroyImmediate(leftHandController.TrackedHand.gameObject);
                DestroyImmediate(leftHandController.VirtualHand.gameObject);
                DestroyImmediate(leftHandController.Origin.gameObject);
                DestroyImmediate(leftHandController.gameObject);
            }

            if (rightHandController != null) {
                DestroyImmediate(rightHandController.TrackedHand.gameObject);
                DestroyImmediate(rightHandController.VirtualHand.gameObject);
                DestroyImmediate(rightHandController.Origin.gameObject);
                DestroyImmediate(rightHandController.gameObject);
            }
        }
    }
}
#endif