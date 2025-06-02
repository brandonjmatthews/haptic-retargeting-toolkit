/*
 * HRTK: PrimitiveBox.cs
 *
 * Copyright (c) 2021 Brandon Matthews
 */

using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting
{
    public class PrimitiveBox : Primitive
    {
        [SerializeField] private Vector3 _size;
        private Vector3[] verts;

        public Vector3 LUF => transform.TransformPoint(Vertices[0]);
        public Vector3 LUB => transform.TransformPoint(Vertices[1]);
        public Vector3 RUF => transform.TransformPoint(Vertices[2]);
        public Vector3 RUB => transform.TransformPoint(Vertices[3]);
        public Vector3 LDF => transform.TransformPoint(Vertices[4]);
        public Vector3 LDB => transform.TransformPoint(Vertices[5]);
        public Vector3 RDF => transform.TransformPoint(Vertices[6]);
        public Vector3 RDB => transform.TransformPoint(Vertices[7]);

        public Vector3[] Vertices
        {
            get
            {
                if (verts == null || verts.Length == 0)
                {
                    verts = new Vector3[8] {
                        new Vector3( Size.x,  Size.y,  Size.z) * 0.5f, // LUF
                        new Vector3( Size.x,  Size.y, -Size.z) * 0.5f, // LUB
                        new Vector3(-Size.x,  Size.y,  Size.z) * 0.5f, // RUF
                        new Vector3(-Size.x,  Size.y, -Size.z) * 0.5f, // RUB
                        new Vector3( Size.x, -Size.y,  Size.z) * 0.5f, // LDF
                        new Vector3( Size.x, -Size.y, -Size.z) * 0.5f, // LDB
                        new Vector3(-Size.x, -Size.y,  Size.z) * 0.5f, // RDF
                        new Vector3(-Size.x, -Size.y, -Size.z) * 0.5f  // RDB 
                    };
                } 
            
                return verts;
            }
        }

        int[] triangles = new int[36] {
                    // Top
                    1, 2, 0,
                    1, 2, 3,

                    // Bottom
                    4, 7, 6,
                    4, 7, 5,

                    // Left
                    0, 5, 4,
                    0, 5, 1,

                    // Back
                    1, 7, 3,
                    1, 7, 5,

                    // Front
                    4, 2, 0,
                    4, 2, 6,

                    // Right
                    3, 6, 2,
                    3, 6, 7
                };

        public Vector3 Size
        {
            get => _size;
            set
            {
                _size = value;
                verts = null;
            }
        }

        public override DistanceResult Distance(Primitive other)
        {
            DistanceResult result = new DistanceResult();
            // if (other is PrimitiveBox) result = PDQ.BoxToBox(this, other as PrimitiveBox);
            if (other is PrimitiveSphere) result = PDQ.SphereToBox(other as PrimitiveSphere, this).Swap();
            else if (other is PrimitiveCapsule) result = PDQ.BoxToCapsule(this, other as PrimitiveCapsule);
            else if (other is PrimitivePlane) result = PDQ.BoxToPlane(this, other as PrimitivePlane);
            else if (other is PrimitivePoint) result = Distance(other.transform.position);
            else Debug.LogWarningFormat("Distance between {0} and {1} is not implemented.", this.GetType().ToString(), other.GetType().ToString());

            if (invert) result.intersecting = result.intersecting == 0 ? 1 : 0;


            return result;
        }

        public override DistanceResult Distance(Vector3 other)
        {
            return PDQ.PointToBox(other, this).Swap();
        }

        public override float SignedDistance(Vector3 position)
        {
            return SDF.Box(RelativePoint(position), Size);
        }

        public Vector3 InitialPosition()
        {
            return LUF;
        }

        public Vector3 Support(Vector3 worldDirection)
        {
            // Vector3 localDirection = transform.InverseTransformDirection(worldDirection);

            Vector3 supportVert = transform.TransformPoint(Vertices[0]);
            float highestDot = Vector3.Dot(worldDirection, supportVert);

            for (int i = 0; i < Vertices.Length; i++)
            {
                Vector3 vert = transform.TransformPoint(Vertices[i]);
                float dot = Vector3.Dot(worldDirection, vert);
                if (dot > highestDot)
                {
                    highestDot = dot;
                    supportVert = vert;
                }
            }

            return supportVert;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        // protected override void Draw()
        // {
        //     DrawLine(0, LDB, RDB);
        //     DrawLine(1, RDB, RDF);
        //     DrawLine(2, LDF, RDF);
        //     DrawLine(3, LDF, LDB);

        //     DrawLine(4, LUB, RUB);
        //     DrawLine(5, RUB, RUF);
        //     DrawLine(6, LUF, RUF);
        //     DrawLine(7, LUF, LUB);

        //     DrawLine(8, LDB, LUB);
        //     DrawLine(9, RDB, RUB);
        //     DrawLine(10, LDF, LUF);
        //     DrawLine(11, RDF, RUF);
        // }

        // private void DrawLine(int index, Vector3 start, Vector3 end) {
        //     if (LinePrefab == null) return;
        //     if (drawnLines[index] == null) {
        //         drawnLines[index] = Instantiate(LinePrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<LineRenderer>();
        //         drawnLines[index].useWorldSpace = true;
        //         drawnLines[index].positionCount = 2;
        //         if (drawnLines[index] == null) return;
        //     }

        //     drawnLines[index].SetPosition(0, start);
        //     drawnLines[index].SetPosition(1, end);
        //     drawnLines[index].startWidth = LineWidth;
        //     drawnLines[index].endWidth = LineWidth;
        // }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Color oldColor = Gizmos.color;
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireCube(Vector3.zero, _size);

            Gizmos.color = oldColor;
        }
    }
}