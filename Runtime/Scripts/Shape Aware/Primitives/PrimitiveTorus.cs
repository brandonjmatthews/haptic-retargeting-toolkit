using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting 
{
    public class PrimitiveTorus : Primitive
    {
        public float Radius;
        public float Thickness;
        public override DistanceResult Distance(Primitive other) {
            DistanceResult result = new DistanceResult();

            if (other is PrimitiveCapsule) result = PDQ.CapsuleToTorus(other as PrimitiveCapsule, this).Swap();
            else if (other is PrimitiveSphere) result = PDQ.SphereToTorus(other as PrimitiveSphere, this).Swap();
            // else if (other is PrimitiveBox) return PDQ.BoxToTorus(other as PrimitiveBox, this).Swap();
            // else if (other is PrimitivePlane) return PDQ.TorusToPlane(this, other as PrimitivePlane);
            else if (other is PrimitivePoint) result = Distance(other.transform.position);
            else Debug.LogWarningFormat("Distance between {0} and {1} is not implemented.", this.GetType().ToString(), other.GetType().ToString());
            
            if (invert) result.intersecting = result.intersecting == 0 ? 1 : 0;
            return result;
        }
  
        public override DistanceResult Distance(Vector3 other)
        {
            return PDQ.PointToTorus(other, this).Swap();
        }

        private void OnDrawGizmos() {
            Color oldColor = Gizmos.color;
            Gizmos.color = Color.cyan;

            // DrawGizmoCircle(transform.position, transform.up, Radius);
            DrawGizmoCircle(transform.position, transform.up, Radius + Thickness);
            DrawGizmoCircle(transform.position, transform.up, Radius - Thickness);
            DrawGizmoCircle(transform.position + (transform.up * Thickness), transform.up, Radius);
            DrawGizmoCircle(transform.position - (transform.up * Thickness), transform.up, Radius);

            DrawGizmoCircle(transform.position + transform.right * -Radius, transform.forward, Thickness);
            DrawGizmoCircle(transform.position + transform.right * Radius, transform.forward, Thickness);
            DrawGizmoCircle(transform.position + transform.forward * -Radius, transform.right, Thickness);
            DrawGizmoCircle(transform.position + transform.forward * Radius, transform.right, Thickness);

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
            // 
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
            // return SDF.Torus(RelativePoint(position), LocalStartPoint, LocalEndPoint, Radius);
            return 0.0f;
        }
    }
}