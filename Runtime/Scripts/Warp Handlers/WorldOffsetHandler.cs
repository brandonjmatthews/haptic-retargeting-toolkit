using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK {
public class WorldOffsetHandler : RetargetingOffsetHandler
{
        
        public Transform worldPivot;
        public Transform worldTransform;

        public override bool Initialize(RetargetingHand trackedHand, RetargetingHand virtualHand) {
                        if (!base.Initialize(trackedHand, virtualHand)) return false;

            if (worldPivot == null) {
                Debug.LogWarning("World pivot is missing");
                return initialized = false;
            } else {
                return initialized = true;
            }
        }

        TrackedTarget currentTracked;
        VirtualTarget currentVirtual;
        float initialAngle;

        public override void ApplyRetargetingOffset(float ratio, RetargetingOrigin origin, TrackedTarget trackedTarget, VirtualTarget virtualTarget, RetargetingHand trackedHand, RetargetingHand virtualHand)
        {
            if (!initialized) return;

            if (trackedTarget != currentTracked || virtualTarget != currentVirtual) {
                Vector3 toTrackedTarget = trackedTarget.transform.position - worldPivot.transform.position;
                Vector3 toVirtualTarget = virtualTarget.transform.position - worldPivot.transform.position;

                currentTracked = trackedTarget;
                currentVirtual = virtualTarget;
                initialAngle = Vector3.SignedAngle(toTrackedTarget, toVirtualTarget, worldPivot.up);
            }
            Debug.Log(initialAngle);
            worldTransform.rotation = Quaternion.Euler(new Vector3(0.0f, Mathf.Lerp(0.0f, -initialAngle, ratio), 0.0f));
            // _positionOffset = virtualTarget.Target.position - trackedTarget.Target.position;
            // _rotationOffset = Quaternion.identity;
            



            // if (handleRotationOffset) {
   
            // }


        }
    }
}
