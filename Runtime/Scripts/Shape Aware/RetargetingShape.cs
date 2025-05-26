/**
Haptic Retargeting Took-kit Prototype
MeshRetargetingShape.cs

@author: Brandon Matthews, 2021
*/

using UnityEngine;

namespace HRTK
{
    public abstract class RetargetingShape : MonoBehaviour
    {
        public abstract DistanceResult ClosestPoints(RetargetingShape otherShape);

        public abstract DistanceResult ClosestPoints(Vector3[] positions);
    }
}