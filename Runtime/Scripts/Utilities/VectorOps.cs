/*
 * HRTK: VectorOps.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using UnityEngine;

namespace HRTK
{
    public static class VectorOps
    {
        public static Vector3 Abs(Vector3 vec)
        {
            return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
        }

        public static Vector3 Min(Vector3 vecA, Vector3 vecB)
        {
            return new Vector3(Mathf.Min(vecA.x, vecB.x), Mathf.Min(vecA.y, vecB.y), Mathf.Min(vecA.z, vecB.z));
        }

        public static Vector3 Max(Vector3 vecA, Vector3 vecB)
        {
            return new Vector3(Mathf.Max(vecA.x, vecB.x), Mathf.Max(vecA.y, vecB.y), Mathf.Max(vecA.z, vecB.z));
        }


        // Maximum/minumum elements of a vector
        public static float MaxElement(Vector3 v)
        {
            return Mathf.Max(Mathf.Max(v.x, v.y), v.z);
        }

        public static float MinElement(Vector3 v)
        {
            return Mathf.Min(Mathf.Min(v.x, v.y), v.z);
        }

        public static float Saturate(float x) {
            return Mathf.Max(0, Mathf.Min(1, x));
        }

        public static Vector3 Saturate(Vector3 v) {
            return new Vector3(Saturate(v.x), Saturate(v.y), Saturate(v.z));
        }

        public static Vector3 Mul(Vector3 a, Vector3 b) {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
    }
}