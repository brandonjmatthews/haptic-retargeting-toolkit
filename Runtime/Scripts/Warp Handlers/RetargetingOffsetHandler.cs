/*
 * HRTK: RetargetingOffsetHandler.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using System;
using UnityEngine;

namespace HRTK
{
    public class RetargetingOffsetHandler : MonoBehaviour {
        public bool handleRotationOffset;
        public float maxRotationOffset;
        
        protected RetargetingHand trackedHand;
        protected RetargetingHand virtualHand;
        protected bool initialized;
        public bool Initialized => initialized;

        protected Vector3 _positionOffset;
        public Vector3 PositionOffset => _positionOffset;
        protected Quaternion _rotationOffset;
        public Quaternion RotationOffset => _rotationOffset;

        // bool _offsetLocked;
        // Vector3 _lockedPositionOffset;
        // Quaternion _lockedRotationOffset;

        // public virtual void LockOffset(bool lockOffset) {
        //     _offsetLocked = lockOffset;

        //     if (_offsetLocked) {
        //         _lockedPositionOffset = _positionOffset;
        //         _lockedRotationOffset = _rotationOffset;
        //     }
        // }

        public virtual bool Initialize(RetargetingHand trackedHand, RetargetingHand virtualHand) {
            if (trackedHand == null) {
                Debug.LogWarning("No Tracked Hand Set, Disabling Offset Handler");
                return false;
            } else {
                this.trackedHand = trackedHand;
            }

            if (virtualHand == null) {
                Debug.LogWarning("No Virtual Hand Set, Copying Tracked Hand");
                CopyTrackedHand();
            } else {
                this.virtualHand = virtualHand;
            }
            
            return initialized = true;
        }

        protected virtual void CopyTrackedHand() {
            GameObject handCopy = Instantiate(trackedHand.gameObject, trackedHand.transform.position, trackedHand.transform.rotation);
            virtualHand = handCopy.GetComponent<RetargetingHand>();
            virtualHand.HandType = HandType.Virtual;
        }

        public virtual void ApplyRetargetingOffset(float ratio, RetargetingOrigin origin, TrackedTarget trackedTarget, VirtualTarget virtualTarget, RetargetingHand trackedHand, RetargetingHand virtualHand)
        {
            if (!initialized) return;
            _positionOffset = virtualTarget.Target.position - trackedTarget.Target.position;
            _rotationOffset = Quaternion.identity;

            if (handleRotationOffset) {
                (_positionOffset, _rotationOffset) = ComputeOffsetWithRotation(ratio, origin, trackedTarget, virtualTarget, trackedHand, virtualHand);

                virtualHand.transform.position = trackedHand.transform.position + _positionOffset;
                virtualHand.transform.rotation = _rotationOffset * trackedHand.transform.rotation;
            } else {
                // Adjust by ratio and apply the retargeting offset 
                _positionOffset = ComputeOffset(ratio, origin, trackedTarget, virtualTarget, trackedHand, virtualHand);

                virtualHand.transform.position = trackedHand.transform.position + _positionOffset;
                virtualHand.transform.rotation = trackedHand.transform.rotation;
            }
        }

        protected virtual (Vector3, Quaternion) ComputeOffsetWithRotation(float ratio, RetargetingOrigin origin, TrackedTarget trackedTarget, VirtualTarget virtualTarget, RetargetingHand trackedHand, RetargetingHand virtualHand) {
            // if (_offsetLocked) return (_lockedPositionOffset, _lockedRotationOffset);

            Vector3 positionOffset = virtualTarget.Target.position - trackedTarget.Target.position;
            Quaternion rotationOffset = Quaternion.identity;
            // Get the world space difference in position and rotation
            Vector3 worldPositionDifference = virtualTarget.transform.position - trackedTarget.transform.position;
            Quaternion worldRotationDifference = trackedTarget.transform.rotation * Quaternion.Inverse(virtualTarget.transform.rotation);


            // Get the transform the tracked hand into the local space of the targets
            Vector3 localTrackedPosition = trackedTarget.transform.InverseTransformPoint(trackedHand.transform.position);
            Quaternion localTrackedRotation = Quaternion.Inverse(trackedTarget.transform.rotation) * trackedHand.transform.rotation;

            // Compute the equivalent world position of the virtual hand
            Vector3 worldVirtualPosition = virtualTarget.transform.TransformPoint(localTrackedPosition);
            Quaternion worldVirtualRotation = virtualTarget.transform.rotation;

            // Compute the adjusted position offset to compensate for the rotation
            positionOffset = Vector3.Lerp(trackedHand.transform.position, worldVirtualPosition, ratio) - trackedHand.transform.position;

            Quaternion targetRotationDifference = virtualTarget.transform.rotation * Quaternion.Inverse(trackedTarget.transform.rotation);
            targetRotationDifference = Util.ClampQuaternion(targetRotationDifference, 0.0f, maxRotationOffset);
            rotationOffset = Quaternion.Inverse(trackedHand.transform.rotation) * Quaternion.Lerp(trackedHand.transform.rotation, targetRotationDifference * trackedHand.transform.rotation, ratio);
            
            return (positionOffset, rotationOffset);
        }

        protected virtual Vector3 ComputeOffset(float ratio, RetargetingOrigin origin, TrackedTarget trackedTarget, VirtualTarget virtualTarget, RetargetingHand trackedHand, RetargetingHand virtualHand) {
            // if (_offsetLocked) return _lockedPositionOffset;

            Vector3 positionOffset = virtualTarget.Target.position - trackedTarget.Target.position;
            positionOffset = Vector3.Lerp(origin.PositionOffset, positionOffset, ratio);
            return positionOffset;
        }

    }
}