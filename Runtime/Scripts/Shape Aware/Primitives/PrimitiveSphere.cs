/*
 * HRTK: PrimitiveSphere.cs
 *
 * Copyright (c) 2021 Brandon Matthews
 */

using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting 
{
   public class PrimitiveSphere : Primitive
    {
        public float Radius;

        // LineRenderer[] lines = new LineRenderer[3];

        public override DistanceResult Distance(Primitive other) {
            DistanceResult result = new DistanceResult();

            if (other is PrimitiveSphere) result = PDQ.SphereToSphere(this, other as PrimitiveSphere);
            else if (other is PrimitiveBox) result = PDQ.SphereToBox(this, other as PrimitiveBox);
            else if (other is PrimitivePlane) result = PDQ.SphereToPlane(this, other as PrimitivePlane);
            else if (other is PrimitiveCapsule) result = PDQ.SphereToCapsule(this, other as PrimitiveCapsule);
            else if (other is PrimitiveTorus) result = PDQ.SphereToTorus(this, other as PrimitiveTorus);
            else if (other is PrimitivePoint) return Distance(other.transform.position);
            else Debug.LogWarningFormat("Distance between {0} and {1} is not implemented.", this.GetType().ToString(), other.GetType().ToString());
            
            if (invert) {
                result.intersecting = result.intersecting == 0 ? 1 : 0;
                result.distance = -result.distance;
            }
            return result;
        }

        public override DistanceResult Distance(Vector3 other)
        {
            DistanceResult result = PDQ.PointToSphere(other, this).Swap();
            if (invert) {
                result.intersecting = result.intersecting == 0 ? 1 : 0;
                result.distance = -result.distance;
            }
            return result;
        }

        // protected override void Draw()
        // {
        //     if (lines[0] == null) lines[0] = MakeLine();
        //     if (lines[1] == null) lines[1] = MakeLine();
        //     if (lines[2] == null) lines[2] = MakeLine();

        //     DrawRendererCircle(lines[0], Vector3.zero, transform.up, Radius);
        //     DrawRendererCircle(lines[1], Vector3.zero, transform.right, Radius);
        //     DrawRendererCircle(lines[2], Vector3.zero, transform.forward, Radius);
        // }

        // private LineRenderer MakeLine() {
        //     LineRenderer lr = Instantiate(LinePrefab, transform).GetComponent<LineRenderer>();
        //     lr.transform.localPosition = Vector3.zero;
        //     lr.transform.localRotation = Quaternion.identity;
        //     lr.useWorldSpace = false;
        //     lr.startWidth = LineWidth;
        //     lr.endWidth = LineWidth;
            
        //     return lr;
        // }

        // private void DrawRendererCircle(LineRenderer line, Vector3 position, Vector3 up, float radius)
        // {
        //     line.positionCount = 92;
        //     // Modified from "DebugExtension"
        //  	up = ((up == Vector3.zero) ? Vector3.up : up).normalized * radius;
        //     Vector3 _forward = Vector3.Slerp(up, -up, 0.5f);
        //     Vector3 _right = Vector3.Cross(up, _forward).normalized*radius;
            
        //     Matrix4x4 matrix = new Matrix4x4();
            
        //     matrix[0] = _right.x;
        //     matrix[1] = _right.y;
        //     matrix[2] = _right.z;
            
        //     matrix[4] = up.x;
        //     matrix[5] = up.y;
        //     matrix[6] = up.z;
            
        //     matrix[8] = _forward.x;
        //     matrix[9] = _forward.y;
        //     matrix[10] = _forward.z;
            
        //     Vector3[] positions = new Vector3[92];
        //     positions[0] = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            
        //     for(var i = 0; i < 91; i++){
        //         Vector3 newPos = new Vector3(Mathf.Cos((i*4)*Mathf.Deg2Rad), 0.0f, Mathf.Sin((i*4)*Mathf.Deg2Rad));
        //         newPos = position + matrix.MultiplyPoint3x4(newPos);
        //         positions[i + 1] = newPos;
        //     }    

        //     line.SetPositions(positions);
        // }

        private void OnDrawGizmos() {
            Color oldColor = Gizmos.color;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, Radius);
            Gizmos.color = oldColor;
        }

        public override float SignedDistance(Vector3 position)
        {
            return SDF.Sphere(RelativePoint(position), Radius);
        }
    }
}