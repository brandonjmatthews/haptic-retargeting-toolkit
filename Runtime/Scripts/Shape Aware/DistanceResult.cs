/**
Haptic Retargeting Took-kit Prototype
DistanceResult.cs

Struct to contain the shortest distance between shapes and nearest points.
@author: Brandon Matthews, 2021
*/

using UnityEngine;

namespace HRTK
{
    [System.Serializable]
    public struct DistanceResult
    {
        public Vector3 pointA;
        public Vector3 pointB;
        public float distance;
        public int intersecting;

        public float Distance => Intersecting ? 0.0f : distance;

        public bool Intersecting
        {
            get => intersecting == 1;
            set
            {
                intersecting = value ? 1 : 0;
            }
        }

        public DistanceResult Swap()
        {
            Vector3 t = pointA;
            pointA = pointB;
            pointB = t;
            return this;
        }
    }
}
