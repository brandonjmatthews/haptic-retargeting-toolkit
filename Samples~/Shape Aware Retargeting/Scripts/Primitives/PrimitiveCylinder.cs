using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting 
{
    public class PrimitiveCylinder : Primitive
    {
        public float Radius;
        public Vector3 LocalStartPoint;
        public Vector3 LocalEndPoint;

        public Vector3 StartPoint => transform.TransformPoint(LocalStartPoint);
        public Vector3 EndPoint => transform.TransformPoint(LocalEndPoint);

        // LineRenderer[] drawnLines = new LineRenderer[14];


        public override DistanceResult Distance(Primitive other) {
            DistanceResult result = new DistanceResult();

            // if (other is PrimitiveCapsule) return PDQ.CylinderToCylinder(this, other as PrimitiveCapsule);
            // else if (other is PrimitiveCapsule) return PDQ.CylidnerToCapsule(this, other as PrimitiveCapsule);
            if (other is PrimitiveSphere) result = PDQ.SphereToCylinder(other as PrimitiveSphere, this).Swap();
            // else if (other is PrimitiveBox) return PDQ.BoxToCylinder(other as PrimitiveBox, this).Swap();
            // else if (other is PrimitivePlane) return PDQ.CylinderToPlane(this, other as PrimitivePlane);
            // else if (other is PrimitiveTorus) return PDQ.CapsuleToTorus(this, other as PrimitiveTorus);
            else if (other is PrimitivePoint) result = Distance(other.transform.position);
            else Debug.LogWarningFormat("Distance between {0} and {1} is not implemented.", this.GetType().ToString(), other.GetType().ToString());
            
            if (invert) result.intersecting = result.intersecting == 0 ? 1 : 0;
            return result;
        }
  
        public override DistanceResult Distance(Vector3 other)
        {
            return PDQ.PointToCylinder(other, this).Swap();
        }


        private void OnDrawGizmos() {
            // Modified from "DebugExtension"
            Vector3 start = StartPoint;
            Vector3 end = EndPoint;

            Vector3 up = (end - start).normalized * Radius;

            Vector3 trueEnd = end + up;
            Vector3 trueStart = start + (-1 * up);

            Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 right = Vector3.Cross(up, forward).normalized * Radius;
            
            Color oldColor = Gizmos.color;
            Gizmos.color = Color.cyan;
            
            float height = (trueStart - trueEnd).magnitude;
            float sideLength = Mathf.Max(0, (height * 0.5f) - Radius);
            Vector3 middle = (trueEnd + trueStart) * 0.5f;
            
            trueStart = middle + ((trueStart - middle).normalized * sideLength);
            trueEnd = middle + ((trueEnd - middle).normalized * sideLength);
            
            // Gizmos.DrawLine(start, end);

            //Radial circles
            DrawGizmoCircle(trueStart, up, Radius);	
            DrawGizmoCircle(trueEnd, -up, Radius);
            
            //Side lines
            Gizmos.DrawLine(trueStart+right, trueEnd+right);
            Gizmos.DrawLine(trueStart-right, trueEnd-right);
            
            Gizmos.DrawLine(trueStart+forward, trueEnd+forward);
            Gizmos.DrawLine(trueStart-forward, trueEnd-forward);
            
            // for(int i = 1; i < 26; i++){
                
            //     //Start endcap
            //     Gizmos.DrawLine(Vector3.Slerp(right, -up, i/25.0f)+trueStart, Vector3.Slerp(right, -up, (i-1)/25.0f)+trueStart);
            //     Gizmos.DrawLine(Vector3.Slerp(-right, -up, i/25.0f)+trueStart, Vector3.Slerp(-right, -up, (i-1)/25.0f)+trueStart);
            //     Gizmos.DrawLine(Vector3.Slerp(forward, -up, i/25.0f)+trueStart, Vector3.Slerp(forward, -up, (i-1)/25.0f)+trueStart);
            //     Gizmos.DrawLine(Vector3.Slerp(-forward, -up, i/25.0f)+trueStart, Vector3.Slerp(-forward, -up, (i-1)/25.0f)+trueStart);
                
            //     //End endcap
            //     Gizmos.DrawLine(Vector3.Slerp(right, up, i/25.0f)+trueEnd, Vector3.Slerp(right, up, (i-1)/25.0f)+trueEnd);
            //     Gizmos.DrawLine(Vector3.Slerp(-right, up, i/25.0f)+trueEnd, Vector3.Slerp(-right, up, (i-1)/25.0f)+trueEnd);
            //     Gizmos.DrawLine(Vector3.Slerp(forward, up, i/25.0f)+trueEnd, Vector3.Slerp(forward, up, (i-1)/25.0f)+trueEnd);
            //     Gizmos.DrawLine(Vector3.Slerp(-forward, up, i/25.0f)+trueEnd, Vector3.Slerp(-forward, up, (i-1)/25.0f)+trueEnd);
            // }
            
            Gizmos.color = oldColor;
        }

        void DrawGizmoCircle(Vector3 position, Vector3 up, float radius) {
            // Modified from "DebugExtension"
         	up = ((up == Vector3.zero) ? Vector3.up : up).normalized * radius;
            Vector3 _forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 _right = Vector3.Cross(up, _forward).normalized*radius;
            
            Matrix4x4 matrix = new Matrix4x4();
            
            matrix[0] = _right.x;
            matrix[1] = _right.y;
            matrix[2] = _right.z;
            
            matrix[4] = up.x;
            matrix[5] = up.y;
            matrix[6] = up.z;
            
            matrix[8] = _forward.x;
            matrix[9] = _forward.y;
            matrix[10] = _forward.z;
            
            Vector3 _lastPoint = position + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
            Vector3 _nextPoint = Vector3.zero;
            
            for(var i = 0; i < 91; i++){
                _nextPoint.x = Mathf.Cos((i*4)*Mathf.Deg2Rad);
                _nextPoint.z = Mathf.Sin((i*4)*Mathf.Deg2Rad);
                _nextPoint.y = 0;
                
                _nextPoint = position + matrix.MultiplyPoint3x4(_nextPoint);
                
                Gizmos.DrawLine(_lastPoint, _nextPoint);
                _lastPoint = _nextPoint;
            }    
        }

        public override float SignedDistance(Vector3 position)
        {
            return SDF.Capsule(RelativePoint(position), LocalStartPoint, LocalEndPoint, Radius);
        }
    }
}