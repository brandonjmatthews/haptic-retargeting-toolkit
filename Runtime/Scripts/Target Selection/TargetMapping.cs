/*
 * HRTK: TargetMapping.cs
 *
 * Copyright (c) 2019 Brandon Matthews
 */

using UnityEngine;
using System.Collections.Generic;

namespace HRTK
{
    public static class TargetMapping
    {
        public enum Method {
            Nearest,
            Optimized
        }
        public enum Source {
            Hand,
            Origin,
        }
        public enum Factor
        {
            Angle,
            Translation,
            Both,
        }


        public static TrackedTarget GetNearestTarget(VirtualTarget virtualTarget, List<TrackedTarget> options)
        {

            if (options == null || options.Count == 0) return null;

            float minDistance = float.MaxValue;
            TrackedTarget nearestTarget = null;
            
            for (int i = 0; i < options.Count; i++) {
                // if (options[i].Selected) continue;
                if (options[i] == null) continue;
                float distance = Vector3.Distance(virtualTarget.Target.position, options[i].Target.position);

                if (distance < minDistance) {
                    minDistance = distance;
                    nearestTarget = options[i];
                }
            }

            return nearestTarget;
        }

        public static TrackedTarget GetOptimalTrackedTarget(VirtualTarget virtualTarget, List<TrackedTarget> options, Factor optimizationFactor, Vector3 vO, Vector3 tO)
        {
            if (options == null || options.Count == 0) return null;

            float optimalVal = float.MaxValue;
            TrackedTarget optimalTarget = null;

            for (int i = 0; i < options.Count; i++) {
                // if (options[i].Selected) continue;

                float testVal = GetOptimizerResult(virtualTarget, options[i], optimizationFactor, vO, tO);

                if (testVal < optimalVal) {
                    optimalVal = testVal;
                    optimalTarget = options[i];
                }
            }

            return optimalTarget;
        }

        
        public static TrackedTarget GetOptimalTrackedTarget(VirtualTarget virtualTarget, List<TrackedTarget> options, Factor optimizationFactor, Vector3 origin)
        {
          if (options == null || options.Count == 0) return null;

            TrackedTarget optimalTarget = null;
            float optimalVal = float.MaxValue;

            for (int i = 0; i < options.Count; i++) {
                // if (options[i].Selected) continue;
                float testVal = GetOptimizerResult(virtualTarget, options[i], optimizationFactor, origin);

                if (testVal < optimalVal) {
                    optimalVal = testVal;
                    optimalTarget = options[i];
                }
            }

            return optimalTarget;
        }


        static float GetOptimizerResult(VirtualTarget virtualTarget, TrackedTarget trackedTarget, Factor optimizationFactor, Vector3 vO, Vector3 tO)
        {
            float optimalVal = float.MaxValue;
            switch (optimizationFactor)
            {
                case Factor.Angle:
                    optimalVal = AngleOptimizerVal(virtualTarget, trackedTarget, vO, tO);
                    break;
                case Factor.Translation:
                    optimalVal = TranslationOptimizerVal(virtualTarget, trackedTarget, vO, tO);
                    break;
                case Factor.Both:
                    optimalVal = TranslationOptimizerVal(virtualTarget, trackedTarget, vO, tO);
                    optimalVal = optimalVal * AngleOptimizerVal(virtualTarget, trackedTarget, vO, tO);
                    break;
            }
            return optimalVal;
        }

        static float GetOptimizerResult(VirtualTarget virtualTarget, TrackedTarget trackedTarget, Factor optimizationFactor, Vector3 origin)
        {
            float optimalVal = float.MaxValue;
            switch (optimizationFactor)
            {
                 case Factor.Angle:
                    optimalVal = AngleOptimizerVal(virtualTarget, trackedTarget, origin);
                    break;
                case Factor.Translation:
                    optimalVal = TranslationOptimizerVal(virtualTarget, trackedTarget, origin);
                    break;
                case Factor.Both:
                    optimalVal = TranslationOptimizerVal(virtualTarget, trackedTarget, origin);
                    optimalVal = optimalVal * AngleOptimizerVal(virtualTarget, trackedTarget, origin);
                    break;
            }
            return optimalVal;
        }

        static float TranslationOptimizerVal(VirtualTarget virtualTarget, TrackedTarget trackedTarget, Vector3 vO, Vector3 tO) {
            // CalculateTranslation is in percent scale, convert back to proportion
            return Util.CalculateTranslation(virtualTarget.Target.position, trackedTarget.Target.position, vO, tO);
        }

        static float TranslationOptimizerVal(VirtualTarget virtualTarget, TrackedTarget trackedTarget, Vector3 origin) {
            // CalculateTranslation is in percent scale, convert back to proportion
            return Util.CalculateTranslation(origin, virtualTarget.Target.position, trackedTarget.Target.position);
        }

        static float AngleOptimizerVal(VirtualTarget virtualTarget, TrackedTarget trackedTarget, Vector3 vO, Vector3 tO) {
            float dotProduct = Util.Dot(virtualTarget.Target.position, trackedTarget.Target.position, vO, tO);
            return Util.MapValue(-1.0f, 1.0f, 0.0f, 2.0f, dotProduct);
        }

        static float AngleOptimizerVal(VirtualTarget virtualTarget, TrackedTarget trackedTarget, Vector3 origin) {
            float dotProduct = Util.Dot(origin, virtualTarget.Target.position, trackedTarget.Target.position);
            return Util.MapValue(-1.0f, 1.0f, 0.0f, 2.0f, dotProduct);
        }
    }
}