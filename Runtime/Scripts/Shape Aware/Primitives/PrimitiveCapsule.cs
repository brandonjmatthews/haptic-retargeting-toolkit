using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting 
{
    public class PrimitiveCapsule : Primitive
    {
        public float Radius;
        public Vector3 LocalStartPoint;
        public Vector3 LocalEndPoint;

        public Vector3 StartPoint => transform.TransformPoint(LocalStartPoint);
        public Vector3 EndPoint => transform.TransformPoint(LocalEndPoint);

        // LineRenderer[] drawnLines = new LineRenderer[14];


        public override DistanceResult Distance(Primitive other) {
            DistanceResult result = new DistanceResult();
            
            if (other is PrimitiveCapsule) result = PDQ.CapsuleToCapsule(this, other as PrimitiveCapsule);
            else if (other is PrimitiveSphere) result = PDQ.SphereToCapsule(other as PrimitiveSphere, this).Swap();
            else if (other is PrimitiveBox) result = PDQ.BoxToCapsule(other as PrimitiveBox, this).Swap();
            else if (other is PrimitivePlane) result = PDQ.CapsuleToPlane(this, other as PrimitivePlane);
            // else if (other is PrimitiveTorus) result = PDQ.CapsuleToTorus(this, other as PrimitiveTorus);
            else if (other is PrimitivePoint) result = Distance(other.transform.position);
            else Debug.LogWarningFormat("Distance between {0} and {1} is not implemented.", this.GetType().ToString(), other.GetType().ToString());

           
            if (invert) result.intersecting = result.intersecting == 0 ? 1 : 0;

            return result;
        }
  
        public override DistanceResult Distance(Vector3 other)
        {
            return PDQ.PointToCapsule(other, this).Swap();
        }

        // protected override void Draw()
        // {
        //     // Modified from "DebugExtension"
        //     Vector3 start = LocalStartPoint;
        //     Vector3 end = LocalEndPoint;

        //     Vector3 up = (end - start).normalized * Radius;

        //     Vector3 trueEnd = end + up;
        //     Vector3 trueStart = start + (-1 * up);

        //     Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
        //     Vector3 right = Vector3.Cross(up, forward).normalized * Radius;
        
            
        //     float height = (trueStart - trueEnd).magnitude;
        //     float sideLength = Mathf.Max(0, (height * 0.5f) - Radius);
        //     Vector3 middle = (trueEnd + trueStart) * 0.5f;
            
        //     trueStart = middle + ((trueStart - middle).normalized * sideLength);
        //     trueEnd = middle + ((trueEnd - middle).normalized * sideLength);

        //     for (int i = 0; i < drawnLines.Length; i++) {
        //         if (drawnLines[i] == null) drawnLines[i] = Instantiate(LinePrefab, transform).GetComponent<LineRenderer>();
        //         drawnLines[i].transform.localPosition = Vector3.zero;
        //         drawnLines[i].transform.localRotation = Quaternion.identity;
        //         drawnLines[i].useWorldSpace = false;
        //         drawnLines[i].startWidth = LineWidth;
        //         drawnLines[i].endWidth = LineWidth;
        //     }

        //     //Radial circles
        //     DrawRendererCircle(drawnLines[0], trueStart, up, Radius);	
        //     DrawRendererCircle(drawnLines[1], trueEnd, -up, Radius);
            
        //     //Side lines
        //     drawnLines[2].positionCount = 2;
        //     drawnLines[2].SetPositions(new Vector3[2] {trueStart + right, trueEnd + right});
        //     drawnLines[3].positionCount = 2;
        //     drawnLines[3].SetPositions(new Vector3[2] {trueStart - right, trueEnd - right});
        //     drawnLines[4].positionCount = 2;
        //     drawnLines[4].SetPositions(new Vector3[2] {trueStart + forward, trueEnd + forward});
        //     drawnLines[5].positionCount = 2;
        //     drawnLines[5].SetPositions(new Vector3[2] {trueStart - forward, trueEnd - forward});
            
        //     DrawRendererCap(drawnLines[6], right, -up, trueStart);
        //     DrawRendererCap(drawnLines[7], -right, -up, trueStart);
        //     DrawRendererCap(drawnLines[8], forward, -up, trueStart);
        //     DrawRendererCap(drawnLines[9], -forward, -up, trueStart);

        //     DrawRendererCap(drawnLines[10], right, up, trueEnd);
        //     DrawRendererCap(drawnLines[11], -right, up, trueEnd);
        //     DrawRendererCap(drawnLines[12], forward, up, trueEnd);
        //     DrawRendererCap(drawnLines[13], -forward, up, trueEnd);
        // }

        // private void DrawRendererCap(LineRenderer line, Vector3 direction, Vector3 up, Vector3 position) {
        //     line.positionCount = 26;
        //     Vector3[] positions = new Vector3[26];

        //     for(int i = 0; i < 26; i++){
        //         positions[i] = Vector3.Slerp(direction, up, i / 25.0f) + position;
        //     }
        //     line.SetPositions(positions);
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
            
            for(int i = 1; i < 26; i++){
                
                //Start endcap
                Gizmos.DrawLine(Vector3.Slerp(right, -up, i/25.0f)+trueStart, Vector3.Slerp(right, -up, (i-1)/25.0f)+trueStart);
                Gizmos.DrawLine(Vector3.Slerp(-right, -up, i/25.0f)+trueStart, Vector3.Slerp(-right, -up, (i-1)/25.0f)+trueStart);
                Gizmos.DrawLine(Vector3.Slerp(forward, -up, i/25.0f)+trueStart, Vector3.Slerp(forward, -up, (i-1)/25.0f)+trueStart);
                Gizmos.DrawLine(Vector3.Slerp(-forward, -up, i/25.0f)+trueStart, Vector3.Slerp(-forward, -up, (i-1)/25.0f)+trueStart);
                
                //End endcap
                Gizmos.DrawLine(Vector3.Slerp(right, up, i/25.0f)+trueEnd, Vector3.Slerp(right, up, (i-1)/25.0f)+trueEnd);
                Gizmos.DrawLine(Vector3.Slerp(-right, up, i/25.0f)+trueEnd, Vector3.Slerp(-right, up, (i-1)/25.0f)+trueEnd);
                Gizmos.DrawLine(Vector3.Slerp(forward, up, i/25.0f)+trueEnd, Vector3.Slerp(forward, up, (i-1)/25.0f)+trueEnd);
                Gizmos.DrawLine(Vector3.Slerp(-forward, up, i/25.0f)+trueEnd, Vector3.Slerp(-forward, up, (i-1)/25.0f)+trueEnd);
            }
            
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