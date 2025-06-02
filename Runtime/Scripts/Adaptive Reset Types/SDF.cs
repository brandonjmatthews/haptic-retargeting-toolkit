/*
 * HRTK: SDF.cs
 *
 * Copyright (c) 2021 Brandon Matthews
 */

using UnityEngine;
using System.Collections.Generic;
using VectorSwizzling;

namespace HRTK
{
    public static class SDF
    {
        public static float Sphere(Vector3 p, float radius)
        {
            return p.magnitude - radius;
        }

        public static float Box(Vector3 p, Vector3 b)
        {
            b = b / 2.0f;
            Vector3 d = VectorOps.Abs(p) - b;
            return VectorOps.Max(d, Vector3.zero).magnitude + VectorOps.MaxElement(VectorOps.Min(d, Vector3.zero));
        }

        public static float Cylinder(Vector3 p, float r, float h)
        {
            float d = new Vector2(p.x, p.z).magnitude - r;
            d = Mathf.Max(d, Mathf.Abs(p.y) - h);
            return d;
        }

        public static float Cone(Vector3 p, float r, float h)
        {
            Vector2 q = new Vector2(p.xz().magnitude, p.y);
            Vector2 tip = q - new Vector2(0, h);
            Vector2 mantleDir = new Vector2(h, r).normalized;
            float mantle = Vector2.Dot(tip, mantleDir);
            float d = Mathf.Max(mantle, -q.y);
            float projected = Vector2.Dot(tip, new Vector2(mantleDir.y, -mantleDir.x));

            // distance to tip
            if ((q.y > h) && (projected < 0))
            {
                d = Mathf.Max(d, tip.magnitude);
            }

            // distance to base ring
            if ((q.x > r) && (projected > new Vector2(h, r).magnitude))
            {
                d = Mathf.Max(d, (q - new Vector2(r, 0)).magnitude);
            }
            return d;
        }

        public static float HexagonalPrismCircumcircle(Vector3 p, Vector2 h)
        {
            Vector3 q = VectorOps.Abs(p);
            return Mathf.Max(q.y - h.y, Mathf.Max(q.x * Mathf.Sqrt(3.0f) * 0.5f + q.z * 0.5f, q.z) - h.x);
        }

        public static float Dodecahedron(Vector3 p)
        {
            float phi = (1.0f + Mathf.Sqrt(5.0f)) * 0.5f;
            Vector3 n = Vector3.Normalize(new Vector3(phi, 1, 0));

            p = new Vector3(Mathf.Abs(p.x), Mathf.Abs(p.y), Mathf.Abs(p.z));
            float a = Vector3.Dot(p, n.xyz());
            float b = Vector3.Dot(p, n.zxy());
            float c = Vector3.Dot(p, n.yzx());
            return Mathf.Max(Mathf.Max(a, b), c) - phi * n.y;
        }

        public static float Plane(Vector3 p, Vector3 n, float distanceFromOrigin)
        {
            return Vector3.Dot(p, n) + distanceFromOrigin;
        }

        public static float Capsule(Vector3 p, Vector3 a, Vector3 b, float r)
        {
            return LineSegment(p, a, b) - r;
        }

        static float LineSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float t = VectorOps.Saturate(Vector3.Dot(p - a, ab) / Vector3.Dot(ab, ab));
            return Vector3.Magnitude((ab * t + a) - p);
        }
    }
}