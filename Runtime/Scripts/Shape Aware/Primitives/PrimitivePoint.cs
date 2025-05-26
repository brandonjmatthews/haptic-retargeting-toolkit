using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting 
{
    public class PrimitivePoint : Primitive
    {
        public override DistanceResult Distance(Primitive other) {
            DistanceResult result = new DistanceResult();

            if (other is PrimitiveCapsule) return PDQ.PointToCapsule(this.transform.position, other as PrimitiveCapsule);
            else if (other is PrimitiveSphere) return PDQ.PointToSphere(this.transform.position, other as PrimitiveSphere);
            else if (other is PrimitiveBox) return PDQ.PointToBox(this.transform.position, other as PrimitiveBox);
            else if (other is PrimitivePlane) return PDQ.PointToPlane(this.transform.position, other as PrimitivePlane);
            // else if (other is PrimitiveTorus) return PDQ.PointToTorus(this.transform.position, other as PrimitiveTorus);
            else if (other is PrimitivePoint) return Distance(other.transform.position);
            else Debug.LogWarningFormat("Distance between {0} and {1} is not implemented.", this.GetType().ToString(), other.GetType().ToString());
            
            if (invert) result.intersecting = result.intersecting == 0 ? 1 : 0;
            return result;
        }

        public override DistanceResult Distance(Vector3 other)
        {
            DistanceResult result;
            result.pointA = transform.position;
            result.pointB = other;
            result.distance = Vector3.Distance(result.pointA, result.pointB);
            result.intersecting = 0;
            return result;
        }

        // protected override void Draw()
        // {
            
        // }

        private void OnDrawGizmos() {
            Color oldColor = Gizmos.color;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.01f);
            Gizmos.color = oldColor;
        }

        public override float SignedDistance(Vector3 position)
        {
            return Vector3.Distance(position, transform.position);
        }
    }
}