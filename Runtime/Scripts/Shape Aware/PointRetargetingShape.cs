using System.Collections.Generic;
using UnityEngine;

namespace HRTK
{
    public class PointRetargetingShape : RetargetingShape
    {
        public List<Transform> Points;

        public override DistanceResult ClosestPoints(Vector3[] positions) {
            DistanceResult minimumDistance = new DistanceResult();
            minimumDistance.pointA = Points[0].position;
            minimumDistance.pointB = positions[0];
            minimumDistance.distance = Vector3.Distance(Points[0].position, positions[0]);

            for (int i = 0; i < Points.Count; i++)
            {
                for (int j = 0; j < positions.Length; j++)
                {
                    float distance = Vector3.Distance(Points[i].position, Points[j].position);

                    if (distance < minimumDistance.distance) {
                        minimumDistance.pointA = Points[i].position;
                        minimumDistance.pointB = positions[0];
                        minimumDistance.distance = distance;
                    }
                }
            }

            return minimumDistance;
        }

        public override DistanceResult ClosestPoints(RetargetingShape otherShape)
        {
            if (Points.Count == 0)
            {
                Debug.LogFormat("Point shape: {0} must have at least one Point to calculate distance", gameObject.name);
                return new DistanceResult();
            }

            if (otherShape is PointRetargetingShape)
            {
                PointRetargetingShape pointShape = otherShape as PointRetargetingShape;

                if (pointShape.Points.Count == 0)
                {
                    Debug.LogFormat("Point shape: {0} must have at least one Point to calculate distance", gameObject.name);
                    return new DistanceResult();
                }
                
                DistanceResult minimumDistance = new DistanceResult();
                minimumDistance.pointA = Points[0].position;
                minimumDistance.pointB = pointShape.Points[0].position;
                minimumDistance.distance = Vector3.Distance(Points[0].position, pointShape.Points[0].position);

                for (int i = 0; i < Points.Count; i++)
                {
                    for (int j = 0; j < pointShape.Points.Count; j++)
                    {
                        float distance = Vector3.Distance(Points[i].position, pointShape.Points[j].position);

                        if (distance < minimumDistance.distance) {
                            minimumDistance.pointA = Points[i].position;
                            minimumDistance.pointB = pointShape.Points[j].position;
                            minimumDistance.distance = distance;
                        }
                    }
                }

                return minimumDistance;
            }
            else
            {
                // Reverse distance calculation for extended shapes
                DistanceResult result = otherShape.ClosestPoints(this).Swap();
                return result;
            }
        }
    }
}