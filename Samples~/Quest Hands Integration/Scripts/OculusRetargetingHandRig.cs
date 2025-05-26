#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HRTK.OculusQuest
{
    public class OculusRetargetingHandRig : RetargetingHandRig
    {
        public GameObject LeftHandAnchor;
        public GameObject RightHandAnchor;

        public override void BuildRig()
        {
            if (leftHandController == null && LeftHandPrefab != null && LeftHandAnchor != null)
            {
                leftHandController = BuildHand(LeftHandPrefab, Chirality.Left);
                leftHandController.transform.parent = LeftHandAnchor.transform;
                leftHandController.gameObject.name = "Left Retargeting Hand";
                if (OriginPrefab != null)
                {
                    GameObject origin = PrefabUtility.InstantiatePrefab(OriginPrefab) as GameObject;
                    origin.transform.parent = this.transform;
                    leftHandController.Origin = origin.GetComponent<RetargetingOrigin>();
                    leftHandController.Origin.gameObject.name = "Left Hand Origin";
                }
            }

            if (rightHandController == null && RightHandPrefab != null && RightHandAnchor != null)
            {
                rightHandController = BuildHand(RightHandPrefab, Chirality.Right);
                rightHandController.transform.parent = RightHandAnchor.transform;
                rightHandController.gameObject.name = "Right Retargeting Hand";
                if (OriginPrefab != null)
                {
                    GameObject origin = PrefabUtility.InstantiatePrefab(OriginPrefab) as GameObject;
                    origin.transform.parent = this.transform;
                    rightHandController.Origin = origin.GetComponent<RetargetingOrigin>();
                    rightHandController.Origin.gameObject.name = "Right Hand Origin";
                }
            }
        }
    }
}
#endif