/*
 * HRTK: PrimitiveDistanceQueries.cs
 *
 * Copyright (c) 2021 Brandon Matthews
 */

using System.Collections.Generic;
using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting 
{
    public static class PDQ
    {
        /// <summary> 
        /// Measures the shortest distance between a Point and a Sphere
        /// </summary>
        /// <param name="point"> The Point in world space. </param>
        /// <param name="sphere"> The Primitive Sphere object. </param>
        /// <returns> A DistanceResult object with:
        ///         - int intersecting : 1 when the point is within the sphere, 0 otherwise
        ///         - Vector3 nearA: the point
        ///         - Vector3 nearB: nearest point on the sphere
        ///         - float distnace: absolute distance between nearA and nearB
        /// </returns>
        public static DistanceResult PointToSphere(Vector3 point, PrimitiveSphere sphere)
        {
            Vector3 direction = Vector3.Normalize(sphere.transform.position - point);
            Vector3 spherePoint = sphere.transform.position - (direction * sphere.Radius);

            DistanceResult r;
            r.intersecting = Vector3.Distance(sphere.transform.position, point) < sphere.Radius ? 1 : 0;
            r.pointA = point;
            r.pointB = spherePoint;
            r.distance = Vector3.Distance(point, spherePoint);
            if (r.intersecting == 1) r.distance = -1 * r.distance;
            return r;
        }

        
        /// <summary> 
        /// Measures the shortest distance between a Point and a Capsule
        /// </summary>
        /// <param name="point"> The Point in world space. </param>
        /// <param name="capsule"> The Primitive Capsule object. </param>
        /// <returns> A DistanceResult object with:
        ///         - int intersecting : 1 when the point is within the capsule, 0 otherwise
        ///         - Vector3 nearA: the point
        ///         - Vector3 nearB: nearest point on the capsule
        ///         - float distnace: absolute distance between nearA and nearB
        /// </returns>
        public static DistanceResult PointToCapsule(Vector3 point, PrimitiveCapsule capsule)
        {
            Vector3 nearCapsuleSeg = SegmentToPointNearestPoint(capsule.StartPoint, capsule.EndPoint, point, out int end);

            Vector3 direction = (point - nearCapsuleSeg).normalized;

            Vector3 capsulePoint = nearCapsuleSeg + (direction * capsule.Radius);

            DistanceResult r;
            r.intersecting = Vector3.Distance(nearCapsuleSeg, point) < capsule.Radius ? 1 : 0;
            r.pointA = point;
            r.pointB = capsulePoint;
            r.distance = Vector3.Distance(point, capsulePoint);
            return r;
        }

        public static DistanceResult PointToBox(Vector3 point, PrimitiveBox box)
        {

            float minDistance = float.MaxValue;
            Vector3 minBox = Vector3.zero;

            Vector3[] quads = new Vector3[24] {
                box.LUF, box.RUF, box.RDF, box.LDF, // F
                box.RDB, box.RUB, box.LUB, box.LDB, // B
                box.LUB, box.RUB, box.RUF, box.LUF, // U
                box.LDF, box.LDB, box.RDB, box.RDF, //D
                box.RUB, box.RUF, box.RDF, box.RDB, //R
                box.LUF, box.LUB, box.LDB, box.LDF, //L
            };

            for (int i = 0; i < quads.Length; i += 4)
            {
                Vector3 projBox = PointToQuadNearestPoints(point, quads[i], quads[i+1], quads[i+2], quads[i+3]);
                
                float dist = Vector3.Distance(point, projBox);

                if (Mathf.Abs(dist) < minDistance)
                {
                    minDistance = dist;
                    minBox = projBox;
                }
            } 
        
            float distance = SDF.Box(box.transform.InverseTransformPoint(point), box.Size);

            DistanceResult result;
            result.pointA = point;
            result.pointB = minBox;
            result.distance = minDistance;
            result.intersecting = distance < 0.0f ? 1 : 0;

            return result;
            // Vector3 origin = box.LDF;
            // Vector3 v100 = box.LDB;
            // Vector3 v010 = box.LUF;
            // Vector3 v001 = box.RDF;

            // var px = v100;
            // var py = v010;
            // var pz = v001;

            // var vx = (px - origin);
            // var vy = (py - origin);
            // var vz = (pz - origin);

            // var tx = Vector3.Dot(point - origin, vx) / Dot2(vx);
            // var ty = Vector3.Dot(point - origin, vy) / Dot2(vy);
            // var tz = Vector3.Dot(point - origin, vz) / Dot2(vz);

            // tx = tx < 0 ? 0 : tx > 1 ? 1 : tx;
            // ty = ty < 0 ? 0 : ty > 1 ? 1 : ty;
            // tz = tz < 0 ? 0 : tz > 1 ? 1 : tz;

            // Vector3 projectedPoint = tx * vx + ty * vy + tz * vz + origin;


            // float distance = SDF.Box(box.transform.InverseTransformPoint(point), box.Size);
            // Debug.Log(distance);
            // DistanceResult r;
            // r.intersecting = distance < 0 ? 1 : 0;
            // r.pointA = point;
            // r.pointB = projectedPoint;
            // r.distance = Vector3.Distance(point, projectedPoint);
            // return r;
        }

        public static DistanceResult PointToPlane(Vector3 point, PrimitivePlane plane)
        {
            Vector3 projectedPoint = ProjectPointOnPlane(point, plane.TransformedNormal, plane.OriginPosition);

            DistanceResult r;
            r.intersecting = SDF.Plane(plane.RelativePoint(point), plane.Normal, plane.OriginDistance) < 0 ? 1 : 0;
            r.pointA = point;
            r.pointB = projectedPoint;
            r.distance = Vector3.Distance(r.pointA, r.pointB);
            return r;
        }

        public static Vector3 ProjectPointOnPlane(Vector3 point, Vector3 normal, Vector3 origin) {
            Vector3 v = point - origin;
            float normalDist = Vector3.Dot(v, normal);
            return point - (normal * normalDist);

        }

        public static DistanceResult PointToTorus(Vector3 point, PrimitiveTorus torus) {
            Vector3 pointOnCircle = PointToCircleNearestPoint(point, torus.transform.position, torus.transform.up, torus.Radius);
            Vector3 ringToPoint = point - pointOnCircle;

            pointOnCircle = pointOnCircle + (ringToPoint.normalized * torus.Thickness);

            DistanceResult r;
            r.pointA = point;
            r.pointB = pointOnCircle;
            r.intersecting = ringToPoint.magnitude < torus.Thickness ? 1 : 0;
            r.distance = Vector3.Distance(r.pointA, r.pointB);
            return r;
        }

        public static DistanceResult SphereToSphere(PrimitiveSphere sphereA, PrimitiveSphere sphereB)
        {
            Vector3 direction = Vector3.Normalize(sphereB.transform.position - sphereA.transform.position);

            DistanceResult r;

            if (sphereA.transform.position == sphereB.transform.position) {
                direction = Vector3.forward;
                r.pointA = sphereA.transform.position - (direction * sphereA.Radius);
                r.pointB = sphereB.transform.position - (direction * sphereB.Radius);
            } else {
                r.pointA = sphereA.transform.position + (direction * sphereA.Radius);
                r.pointB = sphereB.transform.position - (direction * sphereB.Radius);
            }


            bool interectingA = Vector3.Distance(sphereA.transform.position, r.pointB) < sphereA.Radius;
            bool interectingB = Vector3.Distance(sphereB.transform.position, r.pointA) < sphereB.Radius;
            r.intersecting = interectingA || interectingB ? 1 : 0;

            r.distance = Vector3.Distance(r.pointA, r.pointB);
            return r;
        }

        public static DistanceResult SphereToCapsule(PrimitiveSphere sphere, PrimitiveCapsule capsule)
        {
            Vector3 nearCapsuleSeg = SegmentToPointNearestPoint(capsule.StartPoint, capsule.EndPoint, sphere.transform.position, out int end);

            Vector3 direction = (sphere.transform.position - nearCapsuleSeg).normalized;

            DistanceResult r;
            r.pointA = sphere.transform.position - (direction * sphere.Radius);
            r.pointB = nearCapsuleSeg + (direction * capsule.Radius);

            bool interectingSphere = Vector3.Distance(sphere.transform.position, r.pointB) < sphere.Radius;
            bool interectingCapsule = Vector3.Distance(nearCapsuleSeg, r.pointA) < capsule.Radius;
            r.intersecting = interectingCapsule || interectingSphere ? 1 : 0;

            r.distance = Vector3.Distance(r.pointA, r.pointB);
            return r;
        }

        public static DistanceResult PointToCylinder(Vector3 point, PrimitiveCylinder cylinder)
        {
            Vector3 nearCylinderSeg = SegmentToPointNearestPoint(cylinder.StartPoint, cylinder.EndPoint, point, out int end);
            DistanceResult r = new DistanceResult();
            
            if (end == 0) {
                Vector3 direction = (point - nearCylinderSeg).normalized;
                r.pointA = point;
                r.pointB = nearCylinderSeg + (direction * cylinder.Radius);
                bool intersectingCylinder = Vector3.Distance(nearCylinderSeg, r.pointA) < cylinder.Radius;
                r.intersecting = intersectingCylinder ? 1 : 0;
            } else {
                Vector3 normal = Vector3.forward;
                Vector3 endPoint = cylinder.StartPoint;
                if (end == 1) {
                    normal = Vector3.Normalize(cylinder.StartPoint - cylinder.EndPoint);
                    endPoint = cylinder.StartPoint;
                }
                if (end == 2) {
                    normal = Vector3.Normalize(cylinder.EndPoint - cylinder.StartPoint);
                    endPoint = cylinder.EndPoint;
                }

                Vector3 pointOnEndPlane = ProjectPointOnPlane(point, normal, endPoint);
                Vector3 toPoint = pointOnEndPlane - endPoint;
                Vector3 clampedEndPoint = endPoint + Vector3.ClampMagnitude(toPoint, cylinder.Radius);
                Debug.DrawLine(endPoint, clampedEndPoint, Color.red);

                Vector3 direction = (point - clampedEndPoint).normalized;

                r.pointB = clampedEndPoint;
                r.pointA = point;
            }

            r.distance = Vector3.Distance(r.pointA, r.pointB);
            return r;
        }

        public static DistanceResult SphereToCylinder(PrimitiveSphere sphere, PrimitiveCylinder cylinder)
        {
            Vector3 nearCylinderSeg = SegmentToPointNearestPoint(cylinder.StartPoint, cylinder.EndPoint, sphere.transform.position, out int end);
            DistanceResult r = new DistanceResult();
            
            if (end == 0) {
                Vector3 direction = (sphere.transform.position - nearCylinderSeg).normalized;
                r.pointA = sphere.transform.position - (direction * sphere.Radius);
                r.pointB = nearCylinderSeg + (direction * cylinder.Radius);

                bool intersectingSphere = Vector3.Distance(sphere.transform.position, r.pointB) < sphere.Radius;
                bool intersectingCylinder = Vector3.Distance(nearCylinderSeg, r.pointA) < cylinder.Radius;
                r.intersecting = intersectingCylinder || intersectingSphere ? 1 : 0;
            } else {
                Vector3 normal = Vector3.forward;
                Vector3 endPoint = cylinder.StartPoint;
                if (end == 1) {
                    normal = Vector3.Normalize(cylinder.StartPoint - cylinder.EndPoint);
                    endPoint = cylinder.StartPoint;
                }
                if (end == 2) {
                    normal = Vector3.Normalize(cylinder.EndPoint - cylinder.StartPoint);
                    endPoint = cylinder.EndPoint;
                }

                Vector3 pointOnEndPlane = ProjectPointOnPlane(sphere.transform.position, normal, endPoint);
                Vector3 toPoint = pointOnEndPlane - endPoint;
                Vector3 clampedEndPoint = endPoint + Vector3.ClampMagnitude(toPoint, cylinder.Radius);
                Debug.DrawLine(endPoint, clampedEndPoint, Color.red);

                Vector3 direction = (sphere.transform.position - clampedEndPoint).normalized;

                r.pointB = clampedEndPoint;
                r.pointA = sphere.transform.position - (direction * sphere.Radius);
                r.intersecting = 0;
            }

            r.distance = Vector3.Distance(r.pointA, r.pointB);
            return r;
        }

        public static DistanceResult SphereToBox(PrimitiveSphere sphere, PrimitiveBox box)
        {
            DistanceResult r = PointToBox(sphere.transform.position, box);

            Vector3 direction = Vector3.Normalize(r.pointB - sphere.transform.position);
            r.pointA = sphere.transform.position + (direction * sphere.Radius);
            r.distance = Vector3.Distance(r.pointA, r.pointB);

            bool intersectingSphere = Vector3.Distance(sphere.transform.position, r.pointB) < sphere.Radius;
            bool intersectingBox = SDF.Box(box.transform.InverseTransformPoint(r.pointA), box.Size) < 0;
            r.intersecting = (intersectingBox || intersectingSphere) ? 1 : 0;

            return r;
        }

        public static DistanceResult SphereToPlane(PrimitiveSphere sphere, PrimitivePlane plane)
        {
            DistanceResult r = PointToPlane(sphere.transform.position, plane);
            r.distance = Mathf.Abs(r.distance);

            if (r.distance < sphere.Radius)
            {
                r.pointA = r.pointB;
                r.distance = 0.0f;
            }

            Vector3 direction = Vector3.Normalize(r.pointB - sphere.transform.position);
            r.pointA = sphere.transform.position + (direction * sphere.Radius);

            return r;
        }

        public static DistanceResult SphereToTorus(PrimitiveSphere sphere, PrimitiveTorus torus) {
            Vector3 torusPoint = PointToCircleNearestPoint(sphere.transform.position, torus.transform.position, torus.transform.up, torus.Radius);

            Vector3 toTorus = torusPoint - sphere.transform.position;
            Vector3 spherePoint = sphere.transform.position + toTorus.normalized * sphere.Radius;
            Vector3 torusSurfacePoint = torusPoint - toTorus.normalized * torus.Thickness;
            DistanceResult result;
            result.pointA = spherePoint;
            result.pointB = torusSurfacePoint;
            result.distance = Vector3.Distance(torusSurfacePoint, spherePoint);
            result.intersecting = Vector3.Distance(sphere.transform.position, torusSurfacePoint) < sphere.Radius ? 1 : 0;
            return result;
        }

        // public static DistanceResult BoxToBox(PrimitiveBox boxA, PrimitiveBox boxB)
        // {
        //     float minDistance = float.MaxValue;
        //     Vector3 minBoxA = Vector3.zero;
        //     Vector3 minBoxB = Vector3.zero;

        //     Vector3[] vertices = new Vector3[8] {
        //         boxA.LDF,
        //         boxA.LDB,
        //         boxA.RDF,
        //         boxA.RDB,

        //         boxA.LUF,
        //         boxA.LUB,
        //         boxA.RUF,
        //         boxA.RUB
        //     };

        //     int intersecting = 0;
        //     for (int i = 0; i < vertices.Length; i++)
        //     {
        //         DistanceResult r = PointToBox(vertices[i], boxB);

        //         if (r.distance < minDistance)
        //         {
        //             minDistance = r.distance;
        //             minBoxB = r.pointB;
        //             minBoxA = vertices[i];
        //         }
        //     }

        //     DistanceResult result;
        //     result.pointA = minBoxA;
        //     result.pointB = minBoxB;
        //     result.distance = minDistance;
        //     result.intersecting = 0;
        //     return result;
        // }


        // public bool LineIntersectsQuad() {

        // }


        public static DistanceResult BoxToCapsule(PrimitiveBox box, PrimitiveCapsule capsule)
        {
            float minDistance = float.MaxValue;
            Vector3 minBox = Vector3.zero;
            Vector3 minCapsule = Vector3.zero;

            Vector3[] quads = new Vector3[24] {
                box.LUF, box.RUF, box.RDF, box.LDF, // F
                box.RDB, box.RUB, box.LUB, box.LDB, // B
                box.LUB, box.RUB, box.RUF, box.LUF, // U
                box.LDF, box.LDB, box.RDB, box.RDF, //D
                box.RUB, box.RUF, box.RDF, box.RDB, //R
                box.LUF, box.LUB, box.LDB, box.LDF, //L
            };

            for (int i = 0; i < quads.Length; i += 4)
            {
                Vector3 projBox;
                Vector3 projCapsule;
                SegmentToQuadNearestPoints(capsule.StartPoint, capsule.EndPoint, quads[i], quads[i+1], quads[i+2], quads[i+3], out projCapsule, out projBox);
                
                float dist = Vector3.Distance(projCapsule, projBox);

                if (Mathf.Abs(dist) < minDistance)
                {
                    minDistance = dist;
                    minCapsule = projCapsule;
                    minBox = projBox;
                }
            }

            DistanceResult result;

            Vector3 direction = Vector3.Normalize(minBox - minCapsule);
            result.pointA = minBox;
            result.pointB = minCapsule + (direction * capsule.Radius);
            result.distance = Vector3.Distance(result.pointA, result.pointB);

            result.intersecting = minDistance < capsule.Radius ? 1 : 0;

            return result;
        }

        public static DistanceResult BoxToPlane(PrimitiveBox box, PrimitivePlane plane)
        {
            float minDistance = float.MaxValue;
            Vector3 minProjected = Vector3.zero;
            Vector3 minSegment = Vector3.zero;

            Vector3[] segments = new Vector3[24] {
                box.LDB, box.RDB,
                box.RDF, box.RDB,
                box.LDF, box.RDF,
                box.LDF, box.LDB,

                box.LUB, box.RUB,
                box.RUF, box.RUB,
                box.LUF, box.RUF,
                box.LUF, box.LUB,


                box.LUB, box.LDF,
                box.RUB, box.RDB,
                box.LDF, box.LUF,
                box.RDF, box.RUF,
            };

            for (int i = 0; i < segments.Length; i += 2)
            {
                DistanceResult r = PlaneToSegment(plane, segments[i], segments[i + 1]);
                r.distance = Mathf.Abs(r.distance);
                if (r.intersecting == 1)
                {
                    r.distance = 0;
                    return r;
                }
                if (r.distance < minDistance)
                {
                    minDistance = r.distance;
                    minProjected = r.pointA;
                    minSegment = r.pointB;
                }
            }

            DistanceResult result;
            result.pointA = minSegment;
            result.pointB = minProjected;
            result.distance = minDistance;
            result.intersecting = 0;
            return result;
        }

        public static DistanceResult CapsuleToCapsule(PrimitiveCapsule capsuleA, PrimitiveCapsule capsuleB)
        {
            Vector3 nearASeg, nearBSeg;
            SegmentToSegmentNearestPoints(capsuleA.StartPoint, capsuleA.EndPoint, capsuleB.StartPoint, capsuleB.EndPoint, out nearASeg, out nearBSeg);

            Vector3 direction = (nearBSeg - nearASeg).normalized;
            DistanceResult result;
            result.pointA = nearASeg + (direction * capsuleA.Radius);
            result.pointB = nearBSeg - (direction * capsuleB.Radius);
            result.distance = Vector3.Distance(result.pointA, result.pointB);

            bool intersectingA = Vector3.Distance(nearASeg, result.pointB) < capsuleA.Radius;
            bool intersectingB = Vector3.Distance(nearBSeg, result.pointA) < capsuleB.Radius;
            result.intersecting = (intersectingA || intersectingB) ? 1 : 0;


            return result;
        }

        public static DistanceResult CapsuleToPlane(PrimitiveCapsule capsule, PrimitivePlane plane)
        {
            DistanceResult result = PlaneToSegment(plane, capsule.StartPoint, capsule.EndPoint);

            if (result.intersecting == 1 || Mathf.Abs(result.distance) < capsule.Radius)
            {
                result.pointA = result.pointB;
                result.distance = 0.0f;
                result.intersecting = 1;
                return result;
            }
            else
            {
                Vector3 direction = (result.pointA - result.pointB).normalized;
                result.pointB = result.pointB + (direction * capsule.Radius);
                result.distance = Vector3.Distance(result.pointA, result.pointB);
                result.intersecting = 0;
                return result;
            }
        }

        public static DistanceResult CapsuleToTorus(PrimitiveCapsule capsule, PrimitiveTorus torus) {
            Vector3 capsulePoint, torusPoint;
            SegmentToCircleNearestPoints(capsule.StartPoint, capsule.EndPoint, torus.transform.position, torus.transform.up, torus.Radius, out capsulePoint, out torusPoint);

            Vector3 toTorusPoint = torusPoint - capsulePoint;

            torusPoint = torusPoint - toTorusPoint.normalized * torus.Thickness;
            capsulePoint = capsulePoint + toTorusPoint.normalized * capsule.Radius;

            DistanceResult r;
            r.pointA = capsulePoint;
            r.pointB = torusPoint;
            r.distance = Vector3.Distance(capsulePoint, torusPoint);
            r.intersecting = toTorusPoint.magnitude < (torus.Thickness + capsule.Radius) ? 1 : 0;
            return r;
        }

        public static DistanceResult PlaneToPlane(PrimitivePlane planeA, PrimitivePlane planeB)
        {
            DistanceResult result;
            if (planeA.Normal * -1 == planeB.Normal)
            {
                result.pointA = planeA.OriginPosition;
                result.pointB = planeB.OriginPosition;
                result.distance = Vector3.Distance(result.pointA, result.pointB);
                result.intersecting = 0;
            }
            else
            {
                result.pointA = Vector3.zero;
                result.pointB = Vector3.zero;
                result.intersecting = 1;
                result.distance = 0.0f;
            }

            return result;
        }


        static DistanceResult PlaneToSegment(PrimitivePlane plane, Vector3 p0, Vector3 p1)
        {
            Vector3 segment = p1 - p0;

            DistanceResult p0ToPlane = PointToPlane(p0, plane);
            DistanceResult p1ToPlane = PointToPlane(p1, plane);


            // Segment is perpendicular to plane
            if (Vector3.Dot(plane.TransformedNormal, segment.normalized) == 0)
            {
                p0ToPlane.intersecting = 0;
                return p0ToPlane.Swap();
            }

            float t = (Vector3.Dot(plane.TransformedNormal, plane.OriginPosition) - Vector3.Dot(plane.TransformedNormal, p0)) / Vector3.Dot(plane.TransformedNormal, segment.normalized);


            Vector3 intersect = p0 + (segment.normalized * t);
            Vector3 toIntersect = intersect - p0;

            if (Vector3.Dot(segment, toIntersect) < 0)
            {
                // p0 side,
                p0ToPlane.intersecting = 0;
                return p0ToPlane.Swap();
            }
            else
            {
                // p1 side, could be intersecting
                if (toIntersect.magnitude <= segment.magnitude)
                {
                    DistanceResult result;
                    result.pointA = intersect;
                    result.pointB = intersect;
                    result.distance = 0.0f;
                    result.intersecting = 1;
                    return result;
                }
                else
                {
                    p1ToPlane.intersecting = 0;
                    return p1ToPlane.Swap();
                }
            }
        }
    
        static float Dot2(Vector3 v)
        {
            return Vector3.Dot(v, v);
        }

        static Vector3 PointToQuadNearestPoints(Vector3 a0, Vector3 b0, Vector3 b1, Vector3 b2, Vector3 b3) {
            Vector3 p0ABC = PointToTriangle(a0, b0, b1, b2);
            Vector3 p0ACD = PointToTriangle(a0, b0, b2, b3);

            float d0ABC = Vector3.Distance(a0, p0ABC);
            float d0ACD = Vector3.Distance(a0, p0ACD);

            float min = Mathf.Min(d0ABC, d0ACD);

            if (d0ABC <= d0ACD) {
                return p0ABC;
            } else {
                return p0ACD;
            }
        }


        static Vector3 PointToQuadNearestPoint(Vector3 point, Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
            Vector3 DA = A - D;
            Vector3 DC = C - D;
            Vector3 planeNormal = Vector3.Cross(C - D, A - D).normalized;
            Vector3 toPoint = point - D;
            Vector3 projection = point - (Vector3.Dot(toPoint, planeNormal)) * planeNormal;

            Vector3 toProjection = projection - D;
            float x = Vector3.Dot(toProjection, DC) / DC.sqrMagnitude;
            float y = Vector3.Dot(toProjection, DA) / DA.sqrMagnitude;

            x = Mathf.Clamp01(x);
            y = Mathf.Clamp01(y);

            return D + (x * DC) + (y * DA);
        }

        static void SegmentToQuadNearestPoints(Vector3 a0, Vector3 a1, Vector3 b0, Vector3 b1, Vector3 b2, Vector3 b3, out Vector3 nearA, out Vector3 nearB) {

            // Vector3 p0ABC = PointToTriangle(a0, b0, b1, b2);
            // Vector3 p0ACD = PointToTriangle(a0, b0, b2, b3);
            
            // Vector3 p1ABC = PointToTriangle(a1, b0, b1, b2);
            // Vector3 p1ACD = PointToTriangle(a1, b0, b2, b3);

            // float d0ABC = Vector3.Distance(a0, p0ABC);
            // float d0ACD = Vector3.Distance(a0, p0ACD);
            
            // float d1ABC = Vector3.Distance(a1, p1ABC);
            // float d1ACD = Vector3.Distance(a1, p1ACD);

            // float min = Mathf.Min(Mathf.Min(d0ABC, d0ACD), Mathf.Min(d1ABC, d1ACD));

            // if (min == d0ABC) {
            //     nearA = a0;
            //     nearB = p0ABC;
            // } else if (min == d0ACD) {
            //     nearA = a0;
            //     nearB = p0ACD;
            // } else if (min == d1ABC) {
            //     nearA = a1;
            //     nearB = p1ABC;
            // } else {
            //     nearA = a1;
            //     nearB = p1ACD;
            // }           

            Vector3 p0 = PointToQuadNearestPoint(a0, b0, b1, b2, b3);
            Vector3 p1 = PointToQuadNearestPoint(a1, b0, b1, b2, b3);

            Vector3 toP0 = p0 - a0;
            Vector3 toP1 = p1 - a1;

            if (toP0.sqrMagnitude < toP1.sqrMagnitude) {
                nearA = a0;
                nearB = p0;
            } else {
                nearA = a1;
                nearB = p1;
            }
        }

        public static Vector3 PointToTriangle(Vector3 p0, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 u = b - a; //P2P1
            Vector3 v = c - a; //P3P1
            Vector3 w = c - b; //P3P2
            Vector3 n = Vector3.Cross(u, v);
            Vector3 pa = p0 - a;

            // Compute barycentric coordinates
            float gamma = Vector3.Dot(Vector3.Cross(u, pa), n) / Vector3.Dot(n, n);
            float beta = Vector3.Dot(Vector3.Cross(pa, v), n) / Vector3.Dot(n, n);
            float alpha = 1 - gamma - beta;

            // Convert barycentric coordinates into world coordinates
            Vector3 p01 = alpha * a + beta * b + gamma * c;

            // float d = 1 / 3;
            // // Get barycentric center
            // Vector3 center = d * a + d * b + d * c;

            bool alphaInside = alpha >= 0 && alpha <= 1;
            bool betaInside = beta >= 0 && beta <= 1;
            bool gammaInside = gamma >= 0 && gamma <= 1;

            bool inside = alphaInside && betaInside && gammaInside;

            if (inside)
            {
                return p01;
            }

            List<Vector3> projectedPoints = new List<Vector3>();

            int end;
            projectedPoints.Add(SegmentToPointNearestPoint(a, b, p01, out end));
            projectedPoints.Add(SegmentToPointNearestPoint(b, c, p01, out end));
            projectedPoints.Add(SegmentToPointNearestPoint(c, a, p01, out end));

            projectedPoints.Sort(delegate (Vector3 x, Vector3 y)
            {
                float xDist = Vector3.Distance(p01, x);
                float yDist = Vector3.Distance(p01, y);

                if (xDist == yDist) return 0;
                if (xDist < yDist) return -1;
                else return 1;
            });

            Vector3 p02 = projectedPoints[0];
            return p02;
        }

        static void SegmentToCircleNearestPoints(Vector3 segStart, Vector3 segEnd, Vector3 center, Vector3 up, float radius, out Vector3 nearA, out Vector3 nearB) {
            up = ((up == Vector3.zero) ? Vector3.up : up).normalized * radius;
            Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * radius;
          
            Vector3 segment = segEnd - segStart;
            Vector3 startToCenter = center - segStart;
            Vector3 centerToEnd = segEnd - center;
            Vector3 cross = Vector3.Cross(segment.normalized, startToCenter.normalized);

            float dot = Vector3.Dot(up.normalized, segment.normalized);
            if (cross.magnitude <  0.00001f) {
                // Line passes through center 
                if (Mathf.Abs(dot) >= 1.0f - 0.00001f) {
                // Line is perpendicular to the circle's plane
                    float magnitudeDifference = Mathf.Abs(startToCenter.magnitude + centerToEnd.magnitude - segment.magnitude);
                    Debug.Log(magnitudeDifference);
                    if (magnitudeDifference < 0.00001f) {
                        // Center is on the line
                        nearA = center;
                        nearB = center + (forward.normalized * radius);
                    } else {
                        // Center is not on the line
                        nearB = center + (forward.normalized * radius);
                        float startDistance = Vector3.Distance(segStart, nearB);
                        float endDistance = Vector3.Distance(segEnd, nearB);

                        if (startDistance < endDistance) {
                            nearA = segStart;
                        } else {
                            nearA = segEnd;
                        }
                    }
                } else {
                    Vector3 startNearest = PointToCircleNearestPoint(segStart, center, up, radius);
                    Vector3 endNearest = PointToCircleNearestPoint(segEnd, center, up, radius);

                    float startDistance = Vector3.Distance(segStart, startNearest);
                    float endDistance = Vector3.Distance(segEnd, endNearest);

                    float centerDistance = radius;

                    if (startDistance < centerDistance && startDistance <= endDistance) {
                        nearA = segStart;
                        nearB = startNearest;
                    } else if (endDistance < centerDistance && endDistance <= startDistance) {
                        nearA = segEnd;
                        nearB = endNearest;
                    } else {
                        nearA = center;
                        nearB = center + (Vector3.Cross(segment.normalized, up.normalized).normalized * radius);  
                    }
                }
            } else {

                // Line does not pass through center or is not perpendicular
                Vector3 segmentPoint = SegmentToPointNearestPoint(segStart, segEnd, center, out int end);
                Vector3 toSegment = segmentPoint - center;
                
                Vector3 segmentNearest = PointToCircleNearestPoint(segmentPoint, center, up.normalized, radius);
                Vector3 startNearest = PointToCircleNearestPoint(segStart, center, up.normalized, radius);
                Vector3 endNearest = PointToCircleNearestPoint(segEnd, center, up.normalized, radius);

                float startDistance = Vector3.Distance(segStart, startNearest);
                float endDistance = Vector3.Distance(segEnd, endNearest);
                float segmentDistance = Vector3.Distance(segmentPoint, segmentNearest);
                if (endDistance < segmentDistance && endDistance <= startDistance) {
                    nearA = segEnd;
                    nearB = endNearest;
                } else if (startDistance < segmentDistance && startDistance <= endDistance) {
                    nearA = segStart;
                    nearB = startNearest;
                } else {
                    nearA = segmentPoint;
                    nearB = segmentNearest;
                }
            }
        }

        static Vector3 PointToCircleNearestPoint(Vector3 point, Vector3 center, Vector3 up, float radius) {
            up = ((up == Vector3.zero) ? Vector3.up : up).normalized * radius;
            Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * radius;

            Vector3 v = point - center;

            Vector3 projectedPoint;
            if (v.magnitude == 0.0f) {
                projectedPoint = center + forward.normalized * radius;
            } else {
                float normalDist = Vector3.Dot(v, up.normalized);
                projectedPoint = point - (up.normalized * normalDist);

                Vector3 toProjPoint = projectedPoint - center;
                projectedPoint = center + (toProjPoint.normalized * radius);
            }

            return projectedPoint;
        }

        static void SegmentToSegmentNearestPoints(Vector3 a0, Vector3 a1, Vector3 b0, Vector3 b1, out Vector3 nearA, out Vector3 nearB)
        {
            Vector3 dir0 = a1 - a0;
            Vector3 dir1 = b1 - b0;

            Vector3 lineDiff = a0 - b0;

            float a = Vector3.Dot(dir0, dir0);
            float e = Vector3.Dot(dir1, dir1);
            float f = Vector3.Dot(dir1, lineDiff);
            float c = Vector3.Dot(dir0, lineDiff);
            float b = Vector3.Dot(dir0, dir1);

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
            nearA = a0 + dir0 * s;
            nearB = b0 + dir1 * newT;
        }

        static Vector3 SegmentToPointNearestPoint(Vector3 a0, Vector3 a1, Vector3 b, out int end)
        {
            Vector3 lineVec = a1 - a0;
            Vector3 pointVec = b - a0;

            Vector3 proj = a0 + Vector3.Dot(pointVec, lineVec) / Vector3.Dot(lineVec, lineVec) * lineVec;


            float dot = Vector3.Dot(pointVec.normalized, lineVec.normalized);

            //Point is not on side of linePoint2, compared to lineStart.
            //Point is not on the line segment and it is on the side of lineStart.

            Vector3 projVec = a0 - proj;
            //point is on side of linePoint2, compared to lineStart
            if (dot > 0)
            {

                //point is on the line segment
                if (projVec.magnitude <= lineVec.magnitude)
                {
                    end = 0;
                    return proj;
                }

                //point is not on the line segment and it is on the side of linePoint2
                else
                {
                    end = 2;
                    return a1;
                }
            } else {
                end = 1;
                return a0;
            }
        }
    }
}
