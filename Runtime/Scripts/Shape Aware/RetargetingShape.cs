/*
 * HRTK: RetargetingShape.cs
 *
 * Copyright (c) 2021 Brandon Matthews
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