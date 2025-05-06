using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting {
    public class MeshGroupRetargetingShape : RetargetingShape
    {
        public List<MeshRetargetingShape> MeshShapes;
        public override DistanceResult ClosestPoints(RetargetingShape otherShape)
        {
            DistanceResult minDistance = MeshShapes[0].ClosestPoints(otherShape);
            
            for (int i = 1; i < MeshShapes.Count; i++) {
                DistanceResult tempDistance = MeshShapes[i].ClosestPoints(otherShape);
                if (tempDistance.distance < minDistance.distance) {
                    minDistance = tempDistance;
                }
            }

            return minDistance;
        }

        public override DistanceResult ClosestPoints(Vector3[] positions)
        {
            DistanceResult minDistance = MeshShapes[0].ClosestPoints(positions);
            
            for (int i = 1; i < MeshShapes.Count; i++) {
                DistanceResult tempDistance = MeshShapes[i].ClosestPoints(positions);
                if (tempDistance.distance < minDistance.distance) {
                    minDistance = tempDistance;
                }
            }

            return minDistance;
        }
    }
}
