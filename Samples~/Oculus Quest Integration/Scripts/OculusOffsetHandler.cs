using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK.OculusQuest {
    public class OculusOffsetHandler : RetargetingOffsetHandler
    {
        protected OVRCustomSkeleton trackedHandSkeleton;
        protected OVRCustomSkeleton virtualHandSkeleton;
        public override bool Initialize(RetargetingHand trackedHand, RetargetingHand virtualHand) {
            if (!base.Initialize(trackedHand, virtualHand)) return false;
            
            trackedHandSkeleton = trackedHand.GetComponent<OVRCustomSkeleton>();
            virtualHandSkeleton = virtualHand.GetComponent<OVRCustomSkeleton>();

            if (trackedHandSkeleton == null) {
                Debug.LogWarning("Tracked Hand is missing OVR Skeleton");
                return initialized = false && initialized;
            } else if (virtualHandSkeleton == null) {
                Debug.LogWarning("Virtual Hand is missing OVR Skeleton");
                return initialized = false && initialized;
            } else {
                return initialized = true;
            }
        }

        public override void ApplyRetargetingOffset(float ratio, RetargetingOrigin origin, TrackedTarget trackedTarget, VirtualTarget virtualTarget, RetargetingHand trackedHand, RetargetingHand virtualHand)
        {

             if (!initialized) return;
            _positionOffset = virtualTarget.Target.position - trackedTarget.Target.position;
            _rotationOffset = Quaternion.identity;

            if (handleRotationOffset) {
                (_positionOffset, _rotationOffset) = ComputeOffsetWithRotation(ratio, origin, trackedTarget, virtualTarget, trackedHand, virtualHand);
                virtualHandSkeleton.transform.position = trackedHandSkeleton.transform.position + _positionOffset;
                virtualHandSkeleton.transform.rotation = trackedHandSkeleton.transform.rotation * _rotationOffset;
            } else {
                // Adjust by ratio and apply the retargeting offset 
                _positionOffset = ComputeOffset(ratio, origin, trackedTarget, virtualTarget, trackedHand, virtualHand);
                virtualHandSkeleton.transform.position = trackedHandSkeleton.transform.position + _positionOffset;
                virtualHandSkeleton.transform.rotation = trackedHandSkeleton.transform.rotation;
            }
        }

    }
}