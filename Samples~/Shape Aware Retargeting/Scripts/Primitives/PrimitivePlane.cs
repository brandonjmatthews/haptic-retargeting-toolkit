using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting 
{
    public enum Direction {
        Forward,
        Back,
        Up,
        Down,
        Right,
        Left
    }

    public class PrimitivePlane : Primitive
    {
        public Direction NormalDirection;
        public float Height;
        
        public float OriginDistance => 0.0f + Height;
        public Vector3 OriginPosition => transform.position - (Normal * Height);

        public Vector3 Normal {
            get {
                Vector3 normalDirection = Vector3.forward;
                if (NormalDirection == Direction.Forward) normalDirection = Vector3.forward;
                if (NormalDirection == Direction.Up)      normalDirection = Vector3.up;
                if (NormalDirection == Direction.Right)   normalDirection = Vector3.right;
                if (NormalDirection == Direction.Back)    normalDirection = Vector3.back;
                if (NormalDirection == Direction.Down)    normalDirection = Vector3.down;
                if (NormalDirection == Direction.Left)    normalDirection = Vector3.left;
                return normalDirection;
            }
        }

        public Vector3 TransformedNormal {
            get {
                return transform.TransformDirection(Normal).normalized;
            }
        }

        public override DistanceResult Distance(Primitive other) {
            DistanceResult result = new DistanceResult();

            if (other is PrimitivePlane) result = PDQ.PlaneToPlane(this, other as PrimitivePlane);
            else if (other is PrimitiveBox) result = PDQ.BoxToPlane(other as PrimitiveBox, this).Swap();
            else if (other is PrimitiveSphere) result = PDQ.SphereToPlane(other as PrimitiveSphere, this).Swap();
            else if (other is PrimitiveCapsule) result = PDQ.CapsuleToPlane(other as PrimitiveCapsule, this).Swap();
            else if (other is PrimitivePoint) result = Distance(other.transform.position);
            else Debug.LogWarningFormat("Distance between {0} and {1} is not implemented.", this.GetType().ToString(), other.GetType().ToString());

            if (invert) result.intersecting = result.intersecting == 0 ? 1 : 0;
            return result;
        }

        public override DistanceResult Distance(Vector3 other)
        {
            return PDQ.PointToPlane(other, this).Swap();
        }

        public override float SignedDistance(Vector3 position)
        {
            return SDF.Plane(RelativePoint(position), Normal, OriginDistance);
        }

        // protected override void Draw()
        // {
            
        // }

        private void OnDrawGizmos() {
            Color oldColor = Gizmos.color;
            Gizmos.color = Color.cyan;

            Vector3 position = transform.position - (TransformedNormal * Height);
            
            Gizmos.DrawRay(position, TransformedNormal * 0.5f);
            
            if (NormalDirection == Direction.Forward || NormalDirection == Direction.Back)
            {
                Gizmos.DrawLine(position + transform.right   * -0.1f, position + transform.right * 0.1f);
                Gizmos.DrawLine(position + transform.up      * -0.1f, position + transform.up    * 0.1f);
            }
            else if (NormalDirection == Direction.Up || NormalDirection == Direction.Down)
            {
                Gizmos.DrawLine(position + transform.right   * -0.1f, position + transform.right   * 0.1f);
                Gizmos.DrawLine(position + transform.forward * -0.1f, position + transform.forward * 0.1f);
            }
            else
            {
                Gizmos.DrawLine(position + transform.forward * -0.1f, position + transform.forward * 0.1f);
                Gizmos.DrawLine(position + transform.up      * -0.1f, position + transform.up      * 0.1f);
            }

            Gizmos.color = oldColor;
        }
    }
}