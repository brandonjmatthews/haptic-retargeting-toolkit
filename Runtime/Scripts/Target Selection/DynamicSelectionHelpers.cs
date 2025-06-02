/*
 * HRTK: DynamicSelectionHelpers.cs
 *
 * Copyright (c) 2019 Brandon Matthews
 */

using UnityEngine;

namespace HRTK {
    public static class DynamicSelectionHelpers {
        public static bool WithinSelectableAngle(RetargetingController hand, TrackedTarget targetTracked, VirtualTarget targetVirtual, float angleLimit) {
            float angle = Util.CalculateAngle(hand.TrackedHand.transform.position, hand.VirtualHand.transform.position, targetTracked.Target.position, targetVirtual.Target.position);
            return (angle < angleLimit);
        }

        public static bool WithinSelectableTranslation(RetargetingController hand, TrackedTarget targetTracked, VirtualTarget targetVirtual, float translationLimit) {
            float translation = Util.CalculateTranslation(hand.TrackedHand.transform.position, hand.VirtualHand.transform.position, targetTracked.Target.position, targetVirtual.Target.position);
            return (translation < translationLimit);
        }

        public static bool WithinSelectableAngleAndTranslation(RetargetingController hand, TrackedTarget targetTracked, VirtualTarget targetVirtual, float angleLimit, float translationLimit) {
            return WithinSelectableAngle(hand, targetTracked, targetVirtual, angleLimit) && WithinSelectableTranslation(hand, targetTracked, targetVirtual, translationLimit);
        }

        public static bool WithinSelectableAngle(RetargetingOrigin origin, TrackedTarget targetTracked, VirtualTarget targetVirtual, float angleLimit) {
            float angle = Util.CalculateAngle(origin.Position, origin.VirtualPosition, targetTracked.Target.position, targetVirtual.Target.position);
            return (angle < angleLimit);
        }

        public static bool WithinSelectableTranslation(RetargetingOrigin origin, TrackedTarget targetTracked, VirtualTarget targetVirtual, float translationLimit) {
            float translation = Util.CalculateTranslation(origin.Position, origin.VirtualPosition, targetTracked.Target.position, targetVirtual.Target.position);
            return (translation < translationLimit);
        }

        public static bool WithinSelectableAngleAndTranslation(RetargetingOrigin origin, TrackedTarget targetTracked, VirtualTarget targetVirtual, float angleLimit, float translationLimit) {
            return WithinSelectableAngle(origin, targetTracked, targetVirtual, angleLimit) && WithinSelectableTranslation(origin, targetTracked, targetVirtual, translationLimit);
        }

        public static bool BeyondDynamicSelectableThreshold(RetargetingController hand, TrackedTarget previousTracked, VirtualTarget previousVirtual, TrackedTarget targetTracked, VirtualTarget targetVirtual, Vector3 normal, float angleLimit, float translationLimit, float maxHeight = 10.0f, float tolerance = 0.01f) {
            normal = normal.normalized;
            float handDistance =  Util.DistanceAlongVector(targetTracked.Target.position, hand.TrackedHand.transform.position, normal);
            float minimumDistance = AdaptiveReset.AdaptiveResetHeight(previousTracked, previousVirtual, targetTracked, targetVirtual, normal, angleLimit, translationLimit, maxHeight, tolerance);
            return (handDistance > minimumDistance);
        }

        public static bool BeyondStaticSelectableThreshold(RetargetingController hand, TrackedTarget targetTracked, VirtualTarget targetVirtual, Vector3 normal, float thresholdDistance) {
            normal = normal.normalized;
            Vector3 targetToHand = hand.TrackedHand.Palm.position - targetTracked.transform.position;
            Vector3 projection = Vector3.Project(targetToHand, normal);

            float handDistance = projection.magnitude;
            return (handDistance > thresholdDistance);
        }
    }
}