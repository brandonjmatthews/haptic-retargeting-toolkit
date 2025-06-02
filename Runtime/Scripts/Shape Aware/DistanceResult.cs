/**
 * HRTK: DistanceResult.cs
 *
 * Copyright (c) 2021 Brandon Matthews
 * Struct to contain the shortest distance between shapes and nearest points.
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
