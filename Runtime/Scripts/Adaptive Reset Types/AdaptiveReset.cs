using UnityEngine;
using System.Collections.Generic;

namespace HRTK {
    public static class AdaptiveReset {

        public static float AdaptiveResetHeight(TrackedTarget previousTracked, VirtualTarget previousVirtual, TrackedTarget targetTracked, VirtualTarget targetVirtual, Vector3 normal, float maxAngle, float maxTrans, float maxHeight, float tolerance) {
            float angleHeight = 0.0f;
            float translateHeight = 0.0f;
            // Get midpoint between both virtual targets
            Vector3 virtualMidpoint = Util.GetMidpoint(previousVirtual.Target.position, targetVirtual.Target.position);
            Vector3 trackedMidpoint = Util.GetMidpoint(previousTracked.Target.position, targetTracked.Target.position);

            if (previousTracked == targetTracked) {
                angleHeight = AdaptiveAngleHeightShared(previousTracked, previousVirtual, targetTracked, targetVirtual, maxAngle);
                translateHeight = AdaptiveTransHeightShared(previousTracked, previousVirtual, targetTracked, targetVirtual, maxTrans);
            } else {
                if (Util.CalculateAngle(virtualMidpoint, trackedMidpoint, targetVirtual.Target.position, targetTracked.Target.position) > maxAngle) {
                    angleHeight = AdaptiveAngleHeight(previousTracked, previousVirtual, targetTracked, targetVirtual, normal, maxAngle, maxHeight, tolerance);
                }

                if (Util.CalculateTranslation(virtualMidpoint, trackedMidpoint, targetVirtual.Target.position, targetTracked.Target.position) > maxTrans) {
                    translateHeight = AdaptiveTransHeight(previousTracked, previousVirtual, targetTracked, targetVirtual, normal, maxTrans, maxHeight, tolerance);
                }
            }


            return Mathf.Max(Mathf.Abs(angleHeight), Mathf.Abs(translateHeight));
        }
        
        // public static TargetPair AdaptiveInteractionTarget(List<TargetPair> suitableResetTargets, TargetPair currentTargetPair, TargetPair nextTargetPair) {
        //     // For each suitable target that is not the target pair
        //     TargetPair optimalResetPair = suitableResetTargets[0];
        //     float optimalAngle = 999;
        //     foreach(TargetPair tp in suitableResetTargets) {
        //         if (tp.TrackedTarget != nextTargetPair.TrackedTarget && tp.TrackedTarget != currentTargetPair.TrackedTarget) {
        //             // Get the angle around the target to the next tracked and virtual targets
        //             Vector3 toVirtual = tp.TrackedTarget.Target.position - nextTargetPair.VirtualTarget.Target.position;
        //             Vector3 toTracked = tp.TrackedTarget.Target.position - nextTargetPair.TrackedTarget.Target.position;
        //             float angle = Vector3.Angle(toVirtual, toTracked);
        //             if (angle < optimalAngle) {
        //                 // get the suitable target with the lowest angle
        //                 optimalAngle = angle;
        //                 optimalResetPair = tp;
        //             }
        //         }
        //     }
        //     return optimalResetPair;
        // }

        public static float AdaptiveAngleHeight(TrackedTarget previousTracked, VirtualTarget previousVirtual, TrackedTarget targetTracked, VirtualTarget targetVirtual, Vector3 normal, float targetAngle, float upperLimit, float tolerance) {
            // Get midpoint between both virtual targets
            Vector3 virtualMidpoint = Util.GetMidpoint(previousVirtual.Target.position, targetVirtual.Target.position);
            Vector3 trackedMidpoint  = Util.GetMidpoint(previousTracked.Target.position, targetTracked.Target.position);

            float height = 0.0f;

            if (!FindTargetAngleHeight(previousTracked, previousVirtual, targetTracked, targetVirtual, normal, targetAngle, upperLimit, tolerance, out height)) {
                // Fallback to calculating RA triangle around the tracked target
                Debug.Log("No suitable angle found (angle)");
            }

            return height;
        }

        public static float AdaptiveTransHeight(TrackedTarget previousTracked, VirtualTarget previousVirtual, TrackedTarget targetTracked, VirtualTarget targetVirtual, Vector3 normal, float targetTranslation, float upperLimit, float tolerance) {
            float height = 0.0f;

            if (!FindTargetTransHeight(previousTracked, previousVirtual, targetTracked, targetVirtual, normal, targetTranslation, upperLimit, tolerance, out height)) {
                // Fallback to calculating RA triangle around the tracked target
                Debug.Log("No suitable angle found (translation)");
            }

            return height;
        }

        public static float TargetAngle(TrackedTarget previousTracked, VirtualTarget previousVirtual, TrackedTarget targetTracked, VirtualTarget targetVirtual) {
            Vector3 currVT = previousVirtual.Target.position;
            Vector3 currPT = previousTracked.Target.position;

            Vector3 nextVT = targetVirtual.Target.position;
            Vector3 nextPT = targetTracked.Target.position;

            return Util.CalculateAngle(currVT, currPT, nextVT, nextPT);
        }

        public static float TargetTranslationGain(TrackedTarget previousTracked, VirtualTarget previousVirtual, TrackedTarget targetTracked, VirtualTarget targetVirtual) {
            Vector3 currVT = previousVirtual.Target.position;
            Vector3 currPT = previousTracked.Target.position;
            Vector3 nextVT = targetVirtual.Target.position;
            Vector3 nextPT = targetTracked.Target.position;

            return Util.CalculateTranslation(currVT, currPT, nextVT, nextPT);
        }

        static bool FindTargetAngleHeight(TrackedTarget previousTracked, VirtualTarget previousVirtual, TrackedTarget targetTracked, VirtualTarget targetVirtual, Vector3 normal, float targetAngle, float upperLimit, float tolerance, out float height) {
            float upperTolerance = targetAngle + tolerance;
            float lowerTolerance = targetAngle - tolerance;

            // Get midpoint between both virtual targets
            Vector3 virtualMidpoint = Util.GetMidpoint(previousVirtual.Target.position, targetVirtual.Target.position);
            Vector3 trackedMidpoint  = Util.GetMidpoint(previousTracked.Target.position, targetTracked.Target.position);

            // Perform binary search on real numbers to find solution
            float L = 0.0f;
            float R = upperLimit;
            float V = 0.0f;
            bool found = false;

            float A = 0.0f;
            while ((L < R || !Mathf.Approximately(L, R)) && !found) {
                float m = (L + R) / 2.0f;
                Vector3 MV = virtualMidpoint + (normal * m);
                Vector3 MT = trackedMidpoint + (normal * m);
                A = Util.CalculateAngle(MV, MT, targetVirtual.Target.position, targetTracked.Target.position);

                if (A < lowerTolerance) {
                    R = m;
                } else if (A > upperTolerance) {
                    L = m;
                } else {
                    V = m;
                    found = true;
                }
            }

            height = V;
            return found;
        }

        static bool FindTargetTransHeight(TrackedTarget previousTracked, VirtualTarget previousVirtual, TrackedTarget targetTracked, VirtualTarget targetVirtual, Vector3 normal, float targetTrans, float upperLimit, float tolerance, out float height) {
            float upperTolerance = targetTrans + tolerance;
            float lowerTolerance = targetTrans - tolerance;

            // Get midpoint between both virtual targets
            Vector3 virtualMidpoint = Util.GetMidpoint(previousVirtual.Target.position, targetVirtual.Target.position);
            Vector3 trackedMidpoint  = Util.GetMidpoint(previousVirtual.Target.position, targetVirtual.Target.position);

            // Perform binary search on real numbers to find solution
            float L = 0.0f;
            float R = upperLimit;
            float V = 0.0f;
            bool found = false;

            float A = 0.0f;
            while ((L < R || !Mathf.Approximately(L, R)) && !found) {
                float m = (L + R) / 2.0f;
                Vector3 MV = virtualMidpoint + (normal * m);
                Vector3 MT = trackedMidpoint + (normal * m);
                A = Util.CalculateTranslation(MV, MT, targetVirtual.Target.position, targetTracked.Target.position);

                if (A < lowerTolerance) {
                    R = m;
                } else if (A > upperTolerance) {
                    L = m;
                } else {
                    V = m;
                    found = true;
                }
            }
        
            height = V;
            return found;
        }


        public static Vector3 GetMidpointOffset(TrackedTarget previousTracked, VirtualTarget previousVirtual, TrackedTarget targetTracked, VirtualTarget targetVirtual) {
            Vector3 virtualMidpoint = Util.GetMidpoint(previousVirtual.Target.position, targetVirtual.Target.position);
            Vector3 trackedMidpoint = Util.GetMidpoint(previousTracked.Target.position, targetTracked.Target.position);
            Vector3 midpointOffset = trackedMidpoint - virtualMidpoint;
            return midpointOffset;
        }



        public static float AdaptiveAngleHeightShared(TrackedTarget previousTracked, VirtualTarget previousVirtual, TrackedTarget targetTracked, VirtualTarget targetVirtual, float targetAngle) {
            Vector3 midpoint = Util.GetMidpoint(previousVirtual.Target.position, targetVirtual.Target.position);
            float height = GetRATriangleHeightAngle(targetVirtual.Target.position, midpoint, targetAngle);
            return height;
        }

        static float GetRATriangleHeightAngle(Vector3 A, Vector3 B, float angC) {
            /*  
                C
                |\
                |  \
              b |    \ a
                |      \
                |________\
               A    c     B
            */

            float c = Mathf.Abs(Vector3.Magnitude(A - B));
            float tanC = Mathf.Tan(Mathf.Deg2Rad * angC);
            float b = c / tanC;
            return b;
        }

        public static float AdaptiveTransHeightShared(TrackedTarget previousTracked, VirtualTarget previousVirtual, TrackedTarget targetTracked, VirtualTarget targetVirtual, float targetTranslation) {
            Vector3 midpoint = Util.GetMidpoint(previousVirtual.Target.position, targetVirtual.Target.position);
            float height = GetRATriangleHeightTranslation(targetVirtual.Target.position, midpoint, targetTranslation);
            return height;
        }


        static float GetRATriangleHeightTranslation(Vector3 BaseA, Vector3 BaseB, float translationDifference) {

            /*  
                |\
                |  \
              b |    \ c
                |      \
                |________\
                    a
            */
            float a = Mathf.Abs(Vector3.Magnitude(BaseA - BaseB));
            float d = translationDifference + 1;
            // d = (1 + translationDifference)
            // b = c / d;
            // c = b * d; 
            // a2 + b2 = c2
            // a2 + b2 = (b * d)2
            // a2 = (b * d)2 - b2
            // a2 = ((b * d) * (b * d)) - b2
            // a2 = b2 + bd + db + d2 - b2
            // a2 = bd + db + d2
            // a2 / d = b + b + d
            // (a2 / d) - d = 2b
            // b = 2((a2 / d) - d))
            float b = 2 * (((a * a) / d) - d);
            return 0.0f;
            // return b;
        }

        public static float FingerTargetAngleDifference(RetargetingController hand, TrackedTarget trackedTarget, VirtualTarget virtualTarget) {
            return Util.CalculateAngle(hand.VirtualHand.transform.position, hand.TrackedHand.transform.position, virtualTarget.Target.position, trackedTarget.Target.position);
        }

        public static float FingerTargetTranslationDifference(RetargetingController hand, TrackedTarget trackedTarget, VirtualTarget virtualTarget) {
            return Util.CalculateTranslation(hand.VirtualHand.transform.position, hand.TrackedHand.transform.position, virtualTarget.Target.position, trackedTarget.Target.position);
        }

        public static float FingerTargetAngleDifference(RetargetingController hand, VirtualTarget virtualTarget, TrackedTarget trackedTarget) {
            return Util.CalculateAngle(hand.VirtualHand.transform.position, hand.TrackedHand.transform.position, virtualTarget.Target.position, trackedTarget.Target.position);
        }

        public static float FingerTargetTranslationDifference(RetargetingController hand, VirtualTarget virtualTarget, TrackedTarget trackedTarget) {
            return Util.CalculateTranslation(hand.VirtualHand.transform.position, hand.TrackedHand.transform.position, virtualTarget.Target.position, trackedTarget.Target.position);
        }
    }
}