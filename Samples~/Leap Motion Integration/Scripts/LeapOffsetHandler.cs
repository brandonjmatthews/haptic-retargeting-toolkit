using UnityEngine;
using Leap;
using Leap.Unity;

namespace HRTK.LeapMotion
{    
    public class LeapOffsetHandler : RetargetingOffsetHandler {
        // [Header("LeapMotion Hands")]
        protected HandModelBase trackedLeapHand;
        protected HandModelBase virtualLeapHand;

        protected Hand sourceHand;
        protected Hand mimicHand;
        protected bool mimicInitialized = false;
        protected bool mimicBegun = false;

        public static Leap.Unity.Chirality ConvertChirality(HRTK.Chirality cvChirality)
        {
            if (cvChirality == HRTK.Chirality.Left)
            {
                return Leap.Unity.Chirality.Left;
            }
            else
            {
                return Leap.Unity.Chirality.Right;
            }
        }

        public override bool Initialize(RetargetingHand trackedHand, RetargetingHand virtualHand) {
            if (!base.Initialize(trackedHand, virtualHand)) return false;
            trackedLeapHand = trackedHand.GetComponent<HandModelBase>();
            virtualLeapHand = virtualHand.GetComponent<HandModelBase>();

            if (trackedLeapHand == null) {
                Debug.LogWarning("Tracked Hand is missing Leap HandModelBase");
                return initialized = false && initialized;
            } else if (virtualLeapHand == null) {
                Debug.LogWarning("Virtual Hand is missing Leap HandModelBase");
                return initialized = false && initialized;
            } else {
                return initialized;
            }
        }

        public override void ApplyRetargetingOffset(float ratio, RetargetingOrigin origin, TrackedTarget trackedTarget, VirtualTarget virtualTarget, RetargetingHand trackedHand, RetargetingHand virtualHand)
        {
             if (!initialized) return;
            _positionOffset = virtualTarget.Target.position - trackedTarget.Target.position;
            _rotationOffset = Quaternion.identity;

            if (handleRotationOffset) {
                (_positionOffset, _rotationOffset) = ComputeOffsetWithRotation(ratio, origin, trackedTarget, virtualTarget, trackedHand, virtualHand);
            } else {
                // Adjust by ratio and apply the retargeting offset 
                _positionOffset = ComputeOffset(ratio, origin, trackedTarget, virtualTarget, trackedHand, virtualHand);
            }

            MimicHand(_positionOffset, _rotationOffset);
        }

        protected override void CopyTrackedHand() {
            base.CopyTrackedHand();
            if (virtualLeapHand == null) virtualLeapHand = virtualHand.GetComponent<HandModelBase>();

            virtualLeapHand.leapProvider = null;
        }

        void MimicHand(Vector3 positionOffset, Quaternion rotationOffset) {
            sourceHand = trackedLeapHand.GetLeapHand();

            if (sourceHand != null)
            {
                if (mimicHand == null) { mimicHand = new Hand(); }
                // Copy data from the tracked hand into the mimic hand.

                mimicHand.CopyFrom(sourceHand); //copy the stored pose in the mimic hand
                mimicHand.Arm.CopyFrom(sourceHand.Arm); // copy the stored pose's arm into the mimic hand

                // Use the rotation from the live data
                var handRotation = sourceHand.Rotation;

                // Transform the copied hand so that it's centered on the current hands position and matches it's rotation.
                mimicHand.SetTransform(sourceHand.PalmPosition + positionOffset, handRotation  * rotationOffset);
            }

            // Drive the attached HandModel.
            if (mimicHand != null && virtualLeapHand != null)
            {
                // Initialize the handModel if it hasn't already been initialized.
                if (!mimicInitialized)
                {
                    virtualLeapHand.SetLeapHand(mimicHand); //Prevents an error with null reference exception when creating the spheres from
                                                     //the init hand call
                    virtualLeapHand.InitHand();
                    mimicInitialized = true;
                }

                // Set the HandModel's hand data.
                virtualLeapHand.SetLeapHand(mimicHand);

                // "Begin" the HandModel to represent a 'newly tracked' hand.
                if (!mimicBegun)
                {
                    virtualLeapHand.BeginHand();
                    mimicBegun = true;
                }

                virtualLeapHand.UpdateHand();
            }

            virtualLeapHand.gameObject.SetActive(trackedLeapHand.gameObject.activeSelf);
        }
    }
}