/*
 * HRTK: Proximity.cs
 *
 * Copyright (c) 2021 Brandon Matthews
 */

using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting
{
    public static class Proximity
    {
        public struct Triangle
        {
            public Vector3 V0 { get; }
            public Vector3 V1 { get; }
            public Vector3 V2 { get; }

            public Triangle(Vector3 V0, Vector3 V1, Vector3 V2)
            {
                this.V0 = V0;
                this.V1 = V1;
                this.V2 = V2;
            }
        }

        public struct Edge
        {
            public Vector3 Start { get; }
            public Vector3 End { get; }

            public Edge(Vector3 start, Vector3 end)
            {
                this.Start = start;
                this.End = end;
            }
        }

        public static DistanceResult ClosestPointBetweenSegments(Vector3 a0, Vector3 a1, Vector3 b0, Vector3 b1)
        {
            Vector3 A = a1 - a0;
            Vector3 B = b1 - b0;

            Vector3 lineDiff = a0 - b0;

            float a = Vector3.Dot(A, A);
            float e = Vector3.Dot(B, B);
            float f = Vector3.Dot(B, lineDiff);
            float c = Vector3.Dot(A, lineDiff);
            float b = Vector3.Dot(A, B);

            // s and t are the parameter values from iLine1and iLine2.
            float denom = a * e - b * b;
            // denom = max(denom, eps(e))?
            float s = Mathf.Clamp((b * f - c * e) / denom, 0, 1);
            // e = max(e, eps(e));
            float t = (b * s + f) / e;

            // If t in [0,1] done. Else clamp t, recompute s for the new value of t and
            // clamp s to [0, 1]
            float newT = Mathf.Clamp(t, 0, 1);

            if (newT != t)
            {
                s = Mathf.Clamp((newT * b - c) / a, 0, 1);
            }

            // Compute closest points and return distance
            DistanceResult result = new DistanceResult()
            {
                intersecting = 0,
                pointA = a0 + A * s,
                pointB = b0 + B * newT
            };
            result.distance = Vector3.Distance(result.pointA, result.pointB);

            return result;
        }

        public static DistanceResult ClosestPointsTriToTri(Triangle triABC, Triangle triDEF)
        {
            // Find the minimum distance between each pair of edges (9 total combinations)

            // float3 edgeAB[2] = {B, A};
            // float3 edgeCB[2] = {C, B};
            // float3 edgeAC[2] = {A, C};

            // float3 edgeED[2] = {E, D};
            // float3 edgeFE[2] = {F, E};
            // float3 edgeDF[2] = {D, F};

            // float3 triABCEdges[3][2] = {edgeAB, edgeCB, edgeAC};

            // float3 triDEFEdges[3][2] = {edgeED, edgeFE, edgeDF};

            Edge edgeBA = new Edge(triABC.V1, triABC.V0);
            Edge edgeCB = new Edge(triABC.V2, triABC.V1);
            Edge edgeAC = new Edge(triABC.V0, triABC.V2);

            Edge edgeED = new Edge(triDEF.V1, triDEF.V0);
            Edge edgeFE = new Edge(triDEF.V2, triDEF.V1);
            Edge edgeDF = new Edge(triDEF.V0, triDEF.V2);

            Edge[] triABCEdges = new Edge[] {
                edgeBA,
                edgeCB,
                edgeAC
            };

            Edge[] triDEFEdges = new Edge[] {
                edgeED,
                edgeFE,
                edgeDF
            };

            bool finished;

            DistanceResult result = new DistanceResult();
            bool contact = TriContact(triABC, triDEF);

            if (contact)
            {
                result.pointA = Vector3.zero;
                result.pointB = Vector3.zero;
                result.distance = 0.0f;
                result.intersecting = 1;
                return result;
            }

            (finished, result) = ClosestEdgeToEdge(triABCEdges, edgeED, triDEF.V2);

            if (finished)
            {
                return result;
            }

            DistanceResult tempResult;
            (finished, tempResult) = ClosestEdgeToEdge(triABCEdges, edgeFE, triDEF.V0);

            if (tempResult.distance < result.distance)
            {
                result = tempResult;
            }

            if (finished)
            {
                return result;
            }

            (finished, tempResult) = ClosestEdgeToEdge(triABCEdges, edgeDF, triDEF.V1);

            if (tempResult.distance < result.distance)
            {
                result = tempResult;
            }

            if (finished)
            {
                return result;
            }

            tempResult = ClosestVertsBetweenTris(triDEF, triABC);

            if (tempResult.distance < result.distance)
            {
                result = tempResult.Swap();
            }

            tempResult = ClosestVertsBetweenTris(triABC, triDEF);

            if (tempResult.distance < result.distance)
            {
                result = tempResult;
            }

            return result;
        }

        public static Vector3 ClosestPointOnRect(Vector3 point, Vector3 TL, Vector3 TR, Vector3 BL)
        {
            Vector3 accr = TR - TL;
            Vector3 down = BL - TL;
            Vector3 d = point - TL;

            Vector3 q = TL;

            float dist = Vector3.Dot(d, accr);
            float maxDist = Vector3.Dot(accr, accr);

            if (dist >= maxDist)
            {
                q += accr;
            }
            else if (dist > 0.0f)
            {
                q += (dist / maxDist) * accr;
            }

            dist = Vector3.Dot(d, down);
            maxDist = Vector3.Dot(down, down);
            if (dist >= maxDist)
            {
                q += down;
            }
            else if (dist > 0.0f)
            {
                q += (dist / maxDist) * down;
            }

            return q;
        }

        public static Vector3 ClosestPointOnTriangle(Vector3 point, Vector3 A, Vector3 B, Vector3 C)
        {
            Vector3 ab = B - A;
            Vector3 ac = C - A;
            Vector3 bc = C - B;

            float snom = Vector3.Dot(point - A, ab);
            float sdenom = Vector3.Dot(point - B, A - B);

            float tnom = Vector3.Dot(point - A, ac);
            float tdenom = Vector3.Dot(point - C, A - C);

            if (snom <= 0.0f && tnom <= 0.0f) return A;

            float unom = Vector3.Dot(point - B, bc);
            float udenom = Vector3.Dot(point - C, B - C);

            if (sdenom <= 0.0f && unom <= 0.0f) return B;
            if (tdenom <= 0.0f && udenom <= 0.0f) return C;

            Vector3 n = Vector3.Cross(ab, ac);
            float vc = Vector3.Dot(n, Vector3.Cross(A - point, B - point));

            if (vc <= 0.0f && snom >= 0.0f && sdenom >= 0.0f)
            {
                return ab + snom / (snom + sdenom) * ab;
            }

            float va = Vector3.Dot(n, Vector3.Cross(B - point, C - point));
            if (va <= 0.0f && unom >= 0.0f && udenom >= 0.0f)
            {
                return B + unom / (unom + udenom) * bc;
            }

            float vb = Vector3.Dot(n, Vector3.Cross(C - point, A - point));
            if (vb <= 0.0f && tnom >= 0.0f && tdenom >= 0.0f)
            {
                return A + tnom / (tnom / tdenom) * ac;
            }

            float u = va / (va + vb + vc);
            float v = vb / (va + vb + vc);
            float w = 1.0f - u - v;

            Vector3 pos = u * A + v * B + w * C;
            return pos;
        }

        // Find a direction that demonstrates that the current side is closest and
        // separates the triangles.
        private static bool ClosestEdgePoints(Vector3 triPoint1, Vector3 triClosePoint1, Vector3 triPoint2, Vector3 triClosePoint2, Vector3 sepDir)
        {
            Vector3 awayDir = triPoint1 - triClosePoint1;
            float diffDir = Vector3.Dot(awayDir, sepDir);

            awayDir = triPoint2 - triClosePoint2;
            float sameDir = Vector3.Dot(awayDir, sepDir);

            return diffDir <= 0 && sameDir >= 0;
        }

        // Compute the distance between a triangle edge and another triangle’s edges
        private static (bool, DistanceResult) ClosestEdgeToEdge(Edge[] triEdges, Edge edge, Vector3 lastPoint)
        {
            bool finished = false;
            DistanceResult result = new DistanceResult();

            DistanceResult segAB = ClosestPointBetweenSegments(triEdges[0].Start, triEdges[0].End, edge.Start, edge.End);

            Vector3 sepDir = segAB.pointB - segAB.pointA;
            finished = finished || ClosestEdgePoints(triEdges[0].Start, segAB.pointA, lastPoint, segAB.pointB, sepDir);

            result.pointA = segAB.pointA;
            result.pointB = segAB.pointB;
            result.intersecting = 0;

            if (finished)
            {
                result.distance = segAB.distance;
                return (finished, result);
            }

            DistanceResult segBC = ClosestPointBetweenSegments(triEdges[1].Start, triEdges[1].End, edge.Start, edge.End);
            sepDir = segBC.pointB - segBC.pointA;

            finished = finished || ClosestEdgePoints(
                triEdges[2].Start, segBC.pointA, lastPoint, segBC.pointB, sepDir);

            float ABdist;

            if (segAB.distance < segBC.distance)
            {
                ABdist = segAB.distance;
            }
            else
            {
                ABdist = segBC.distance;
                result.pointA = segBC.pointA;
                result.pointB = segBC.pointB;
            }

            if (finished)
            {
                result.distance = ABdist;
                return (finished, result);
            }

            DistanceResult segCA = ClosestPointBetweenSegments(triEdges[2].Start, triEdges[2].End, edge.Start, edge.End);
            sepDir = segCA.pointB - segCA.pointB;

            finished = finished || ClosestEdgePoints(
                triEdges[0].Start, segCA.pointA, lastPoint, segCA.pointB, sepDir);

            if (ABdist < segCA.distance)
            {
                result.distance = ABdist;
            }
            else
            {
                result = segCA;
            }

            return (finished, result);
        }

        // Compute the distance between iTriB vertexes and another triangle iTriA
        static DistanceResult ClosestVertsBetweenTris(Triangle triABC, Triangle triDEF)
        {
            DistanceResult result = new DistanceResult();
            Vector3 Ap, Bp, Cp;

            Ap = ClosestPointOnTriangle(triDEF.V0, triABC.V0, triABC.V1, triABC.V2);
            Bp = ClosestPointOnTriangle(triDEF.V1, triABC.V0, triABC.V1, triABC.V2);
            Cp = ClosestPointOnTriangle(triDEF.V2, triABC.V0, triABC.V1, triABC.V2);

            float dA = Vector3.Distance(triDEF.V0, Ap);
            float dB = Vector3.Distance(triDEF.V1, Bp);
            float dC = Vector3.Distance(triDEF.V2, Cp);

            float ABdist;
            Vector3 ABp;
            if (dA < dB)
            {
                ABdist = dA;
                ABp = Ap;
            }
            else
            {
                ABdist = dB;
                ABp = Bp;
            }

            if (ABdist < dC)
            {
                result.pointA = ABp;

                if (dA < dB)
                {
                    result.pointB = triDEF.V0;
                }
                else
                {
                    result.pointB = triDEF.V1;
                }

                result.distance = ABdist;
                result.intersecting = 0;
            }
            else
            {
                result.pointA = Cp;
                result.pointB = triDEF.V2;
                result.distance = dC;
                result.intersecting = 0;
            }

            return result;
        }

        static bool TriContact(Triangle triABC, Triangle triDEF)
        {
            Vector3 P1 = triABC.V0;
            Vector3 P2 = triABC.V1;
            Vector3 P3 = triABC.V2;

            Vector3 Q1 = triDEF.V0;
            Vector3 Q2 = triDEF.V1;
            Vector3 Q3 = triDEF.V2;

            Vector3 p1 = new Vector3(0, 0, 0);
            Vector3 p2 = P2 - P1;
            Vector3 p3 = P3 - P1;

            Vector3 q1 = Q1 - P1;
            Vector3 q2 = Q2 - P1;
            Vector3 q3 = Q3 - P1;

            Vector3 e1 = P2 - P1;
            Vector3 e2 = P3 - P2;

            Vector3 f1 = Q2 - Q1;
            Vector3 f2 = Q3 - Q2;

            Vector3 n1 = Vector3.Cross(e1, e2);
            bool x0 = Project(n1, p1, p2, p3, q1, q2, q3);
            Vector3 m1 = Vector3.Cross(f1, f2);
            bool x1 = Project(m1, p1, p2, p3, q1, q2, q3);

            Vector3 t;
            t = Vector3.Cross(e1, f1);
            bool x2 = Project(t, p1, p2, p3, q1, q2, q3);
            t = Vector3.Cross(e1, f2);
            bool x3 = Project(t, p1, p2, p3, q1, q2, q3);
            Vector3 f3 = q1 - q3;
            t = Vector3.Cross(e1, f3);
            bool x4 = Project(t, p1, p2, p3, q1, q2, q3);

            t = Vector3.Cross(e2, f1);
            bool x5 = Project(t, p1, p2, p3, q1, q2, q3);
            t = Vector3.Cross(e2, f2);
            bool x6 = Project(t, p1, p2, p3, q1, q2, q3);
            t = Vector3.Cross(e2, f3);
            bool x7 = Project(t, p1, p2, p3, q1, q2, q3);

            Vector3 e3 = p1 - p3;
            t = Vector3.Cross(e3, f1);
            bool x8 = Project(t, p1, p2, p3, q1, q2, q3);
            t = Vector3.Cross(e3, f2);
            bool x9 = Project(t, p1, p2, p3, q1, q2, q3);
            t = Vector3.Cross(e3, f3);
            bool x10 = Project(t, p1, p2, p3, q1, q2, q3);

            t = Vector3.Cross(e1, n1);
            bool x11 = Project(t, p1, p2, p3, q1, q2, q3);
            t = Vector3.Cross(e2, n1);
            bool x12 = Project(t, p1, p2, p3, q1, q2, q3);
            t = Vector3.Cross(e3, n1);
            bool x13 = Project(t, p1, p2, p3, q1, q2, q3);

            t = Vector3.Cross(f1, m1);
            bool x14 = Project(t, p1, p2, p3, q1, q2, q3);
            t = Vector3.Cross(f2, m1);
            bool x15 = Project(t, p1, p2, p3, q1, q2, q3);
            t = Vector3.Cross(f3, m1);
            bool x16 = Project(t, p1, p2, p3, q1, q2, q3);

            return x1 && x2 && x3 && x4 && x5 && x6 && x7 && x8 && x9 && x10 && x11 &&
                   x12 && x13 && x14 && x15 && x16;
        }

        // A common subroutine for each separating direction
        static bool Project(Vector3 ax, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 q1, Vector3 q2, Vector3 q3)
        {

            float P1 = Vector3.Dot(ax, p1);
            float P2 = Vector3.Dot(ax, p2);
            float P3 = Vector3.Dot(ax, p3);

            float Q1 = Vector3.Dot(ax, q1);
            float Q2 = Vector3.Dot(ax, q2);
            float Q3 = Vector3.Dot(ax, q3);

            float mx1 = Mathf.Max(P1, Mathf.Max(P2, P3));
            float mn1 = Mathf.Min(P1, Mathf.Min(P2, P3));
            float mx2 = Mathf.Max(Q1, Mathf.Max(Q2, Q3));
            float mn2 = Mathf.Min(Q1, Mathf.Min(Q2, Q3));

            return (mn1 <= mx2) && (mn2 <= mx1);
        }

        /* Returns whether a ray intersects a triangle. Developed by Möller–Trumbore. */
        static int RayTriangleIntersection(Vector3 origin, Vector3 direction, Vector3 v0, Vector3 v1, Vector3 v2) {
            Vector3 e1, e2, h, s, q;
            float a, f, u, v, t;

            e1 = v1 - v0;
            e2 = v2 - v0;

            h = Vector3.Cross(direction, e2);
            a = Vector3.Dot(e1, h);

            if (Mathf.Abs(a) < float.Epsilon) {
                return 0;  // ray is parallel to triangle
            }

            f = 1.0f / a;
            s = origin - v0;
            u = f * Vector3.Dot(s, h);

            if (u < 0.0f || u > 1.0f) {
                return 0;
            }

            q = Vector3.Cross(s, e1);
            v = f * Vector3.Dot(direction, q);

            if (v < 0.0f || u + v > 1.0f) {
                return 0;
            }

            t = f * Vector3.Dot(e2, q);

            return (t >= float.Epsilon) ? 1 : 0;
        }



        public static int RayMeshIntersectionCount(Vector3 origin, Vector3 direction, Mesh mesh) {
            int count = 0;

            for (int t = 0; t < mesh.triangles.Length; t+=3) {
                Vector3 v0 = mesh.vertices[mesh.triangles[t]];
                Vector3 v1 = mesh.vertices[mesh.triangles[t+1]];
                Vector3 v2 = mesh.vertices[mesh.triangles[t+2]];
                count += RayTriangleIntersection(origin, direction, v0, v1, v2);
            }

            return count;
        }
    }
}
