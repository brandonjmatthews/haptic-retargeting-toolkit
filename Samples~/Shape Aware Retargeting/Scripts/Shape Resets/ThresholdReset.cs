using System.Collections.Generic;
using HRTK.Modules.ShapeRetargeting;
using UnityEngine;

namespace HRTK
{
    public abstract class ThresholdReset : RetargetingReset
    {
        [SerializeField] protected PrimitiveRetargetingShape ThresholdShape;
        float currentDistance;
        [SerializeField] DistanceResult result;
        protected override bool CheckResetComplete() {
            result = ThresholdShape.ClosestPoints(Hand.VirtualHand.Shape);
            currentDistance = result.distance;
            
            if (result.Intersecting) {
                return true;
            } else {
                return false;
            }
        }
    }
}
