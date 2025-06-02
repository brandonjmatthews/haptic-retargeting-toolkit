/*
 * HRTK: TargetOffsetHandler.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK {
public class TargetOffsetHandler : RetargetingOffsetHandler
{
        public Transform virtualParent;
        public Transform trackedParent;
        public float combinationRatio = 0f;
        public override bool Initialize(RetargetingHand trackedHand, RetargetingHand virtualHand) {
            if (!base.Initialize(trackedHand, virtualHand)) return false;
            return initialized = true;
        }

        TrackedTarget currentTracked;
        VirtualTarget currentVirtual;
        Vector3 initialOffset;

        public override void ApplyRetargetingOffset(float ratio, RetargetingOrigin origin, TrackedTarget trackedTarget, VirtualTarget virtualTarget, RetargetingHand trackedHand, RetargetingHand virtualHand)
        {
            if (!initialized) return;

            if (trackedTarget != currentTracked || virtualTarget != currentVirtual) {
                currentTracked = trackedTarget;
                currentVirtual = virtualTarget;
                initialOffset = currentVirtual.transform.position - currentTracked.transform.position;
            }

            combinationRatio = Mathf.Clamp01(combinationRatio);

            _positionOffset = virtualTarget.Target.position - trackedTarget.Target.position;
            // _rotationOffset = Quaternion.identity;

            if (handleRotationOffset) {
                // (_positionOffset, _rotationOffset) = ComputeOffsetWithRotation(ratio, origin, trackedTarget, virtualTarget, trackedHand, virtualHand);
                // _positionOffset = -_positionOffset;
                // _rotationOffset = Quaternion.Inverse(_rotationOffset);

                // if (virtualParent != null && trackedParent != null) {
                //     virtualParent.transform.position = trackedParent.transform.position + _positionOffset;
                //     virtualParent.transform.rotation = trackedParent.transform.rotation * _rotationOffset;
                // } else {
                //     virtualTarget.transform.position = trackedTarget.transform.position + _positionOffset;
                //     virtualTarget.transform.rotation = trackedTarget.transform.rotation;
                // }
            } else {
                Vector3 interfaceOffset =  Vector3.Lerp(Vector3.zero, initialOffset, ratio) * combinationRatio;


                if (virtualParent != null && trackedParent != null) {
                    // Adjust by ratio and apply the retargeting offset 
                    virtualParent.transform.position = trackedParent.transform.position - interfaceOffset;
                    virtualParent.transform.rotation = trackedParent.transform.rotation;
                } else {
                    // Adjust by ratio and apply the retargeting offset 
                    virtualTarget.transform.position = trackedTarget.transform.position - interfaceOffset;
                    virtualTarget.transform.rotation = trackedTarget.transform.rotation;
                }
                
                Vector3 handOffset = ComputeOffset(ratio, origin, trackedTarget, virtualTarget, trackedHand, virtualHand);
                _positionOffset = handOffset;

                virtualHand.transform.position = trackedHand.transform.position + handOffset;
                virtualHand.transform.rotation = trackedHand.transform.rotation;

            }
        }
    }
}
