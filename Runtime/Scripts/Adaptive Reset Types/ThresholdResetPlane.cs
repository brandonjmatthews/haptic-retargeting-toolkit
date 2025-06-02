/*
 * HRTK: ThresholdResetPlane.cs
 *
 * Copyright (c) 2021 Brandon Matthews
 */

using HRTK.Modules.ShapeRetargeting;
using UnityEngine;

namespace HRTK
{
    [RequireComponent(typeof(PrimitivePlane))]
    public class ThresholdResetPlane : ThresholdReset
    {
        [SerializeField] PrimitivePlane ThresholdPlane;
        
        Vector3 initialLocalPosition;

        private void Start() {
            if (ThresholdPlane == null) ThresholdPlane = GetComponent<PrimitivePlane>();
            initialLocalPosition = transform.localPosition;
 
            ConfigureInitialReset();
        }

        public override void ConfigureInitialReset()
        {
            transform.localPosition = initialLocalPosition - (ThresholdPlane.Normal * StaticDistance);
        }

        public override void ConfigureStaticReset(TrackedTarget currentTracked, VirtualTarget currentVirtual, TrackedTarget nextTracked = null, VirtualTarget nextVirtual = null)
        {   
            Vector3 virtualMidpoint = Vector3.zero;
            Vector3 trackedMidpoint = Vector3.zero;

            if (nextTracked == null || nextVirtual == null) {
                virtualMidpoint = currentVirtual.Target.position;
            } else {
                virtualMidpoint = Util.GetMidpoint(nextVirtual.Target.position, currentVirtual.Target.position);
            }


            transform.localPosition = initialLocalPosition - (ThresholdPlane.Normal * StaticDistance);
        }

        public override void ConfigureAdaptiveReset(TrackedTarget currentTracked, VirtualTarget currentVirtual, TrackedTarget nextTracked, VirtualTarget nextVirtual)
        {
            Vector3 virtualMidpoint = Util.GetMidpoint(nextVirtual.Target.position, currentVirtual.Target.position);
            Vector3 trackedMidpoint = Util.GetMidpoint(nextTracked.Target.position, currentTracked.Target.position);


            Vector3 direction = Head.transform.position - virtualMidpoint;

            float angleHeight = 0.0f;
            float translateHeight = 0.0f;
            // Get midpoint between both virtual targets

            if (nextTracked == currentTracked)
            {
                angleHeight = AdaptiveReset.AdaptiveAngleHeightShared(nextTracked, nextVirtual, currentTracked, currentVirtual, AdaptiveMaxAngle);
                translateHeight = AdaptiveReset.AdaptiveTransHeightShared(nextTracked, nextVirtual, currentTracked, currentVirtual, AdaptiveMaxTranslation);
            }
            else
            {
                if (Util.CalculateAngle(virtualMidpoint, trackedMidpoint, currentVirtual.Target.position, currentTracked.Target.position) > AdaptiveMaxAngle)
                {
                    angleHeight = AdaptiveReset.AdaptiveAngleHeight(nextTracked, nextVirtual, currentTracked, currentVirtual, direction, AdaptiveMaxAngle, AdaptiveMaxDistance, AdaptiveTolerance);
                }

                if (Util.CalculateTranslation(virtualMidpoint, trackedMidpoint, currentVirtual.Target.position, currentTracked.Target.position) > AdaptiveMaxTranslation)
                {
                    translateHeight = AdaptiveReset.AdaptiveTransHeight(nextTracked, nextVirtual, currentTracked, currentVirtual, direction, AdaptiveMaxTranslation, AdaptiveMaxDistance, AdaptiveTolerance);
                }
            }

            float distance = Mathf.Max(Mathf.Abs(angleHeight), Mathf.Abs(translateHeight));

            transform.position = virtualMidpoint - (direction * distance);
        }
    }
}
