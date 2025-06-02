/*
 * HRTK: PrimitiveRetargetingShape.cs
 *
 * Copyright (c) 2021 Brandon Matthews
 */

using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace HRTK.Modules.ShapeRetargeting
{
    public class PrimitiveRetargetingShape : RetargetingShape
    {
        [SerializeField] bool _autoDetectChildPrimitives = true;
        [SerializeField] List<Primitive> _primitives;

        ComputeBuffer _primitiveBuffer;

        int _bufferSize;
        public int BufferSize => _bufferSize;

        public List<Primitive> Primitives {
            get {
                return _primitives;
            }

            set {
                _primitives = value;
            }
        }

        private void Start() {
            if (_autoDetectChildPrimitives) {
                LoadChildPrimitives();
            }
        }

        void LoadChildPrimitives() {
            Primitive[] childPrimitives = transform.GetComponentsInChildren<Primitive>();
            Primitives = new List<Primitive>(childPrimitives);
        }

        public int GetPrimitiveBuffer(out ComputeBuffer buffer)
        {
            List<ComputePrimitive> computePrimitives = new List<ComputePrimitive>();
            for (int i = 0; i < Primitives.Count; i++)
            {
                if (Primitives[i] == null) continue;
                
                if (Primitives[i] is PrimitiveSphere)
                {
                    PrimitiveSphere sphere = Primitives[i] as PrimitiveSphere;
                    computePrimitives.Add(new ComputePrimitive
                    {
                        type = 0,
                        radius = sphere.Radius,
                        position = sphere.transform.position
                    });
                }
                else if (Primitives[i] is PrimitiveCapsule)
                {
                    PrimitiveCapsule capsule = Primitives[i] as PrimitiveCapsule;
                    computePrimitives.Add(new ComputePrimitive
                    {
                        type = 1,
                        radius = capsule.Radius,
                        position = capsule.StartPoint,
                        extraVector = capsule.EndPoint
                    });
                }
                else if (Primitives[i] is PrimitiveBox)
                {
                    PrimitiveBox box = Primitives[i] as PrimitiveBox;
                    computePrimitives.Add(new ComputePrimitive
                    {
                        type = 2,
                        position = box.Size,
                        transform = Matrix4x4.TRS(box.transform.position, box.transform.rotation, box.transform.lossyScale)
                    });
                }
            }


            if (computePrimitives.Count > 0)
            {

                if (_primitiveBuffer == null || computePrimitives.Count != _bufferSize)
                {
                    _primitiveBuffer = new ComputeBuffer(computePrimitives.Count, Marshal.SizeOf(typeof(ComputePrimitive)));
                    _bufferSize = computePrimitives.Count;
                }

                _primitiveBuffer.SetData(computePrimitives.ToArray());
            }
            else
            {
                _bufferSize = 0;
            }

            buffer = _primitiveBuffer;
            return _bufferSize;
        }

        public override DistanceResult ClosestPoints(Vector3[] positions)
        {
            if (positions.Length < 1) return new DistanceResult();

            DistanceResult minResult = Primitives[0].Distance(positions[0]);

            for (int i = 0; i < Primitives.Count; i++)
            {
                if (Primitives[i] == null) continue;

                for (int j = 0; j < positions.Length; j++)
                {
                    DistanceResult result = Primitives[i].Distance(positions[j]);

                    if (result.Distance < minResult.Distance)
                    {
                        minResult = result;
                    }
                }
            }

            return minResult;
        }

        public override DistanceResult ClosestPoints(RetargetingShape otherShape)
        {
            if (Primitives.Count == 0)
            {
                Debug.LogFormat("Primitive shape: {0} must have at least one Primitive to calculate distance", gameObject.name);
                return new DistanceResult();
            }

            if (otherShape is PointRetargetingShape)
            {
                PointRetargetingShape pointShape = otherShape as PointRetargetingShape;

                DistanceResult minResult = Primitives[0].Distance(pointShape.Points[0].position);


                if (pointShape.Points.Count == 0)
                {
                    Debug.LogFormat("Point shape: {0} must have at least one Point to calculate distance", gameObject.name);
                    return new DistanceResult();
                }

                for (int i = 0; i < Primitives.Count; i++)
                {
                    for (int j = 0; j < pointShape.Points.Count; j++)
                    {
                        DistanceResult result = Primitives[i].Distance(pointShape.Points[j].position);

                        if (result.Distance < minResult.Distance)
                        {
                            minResult = result;
                        }
                    }
                }

                return minResult;
            }
            else if (otherShape is PrimitiveRetargetingShape)
            {
                PrimitiveRetargetingShape primitiveShape = otherShape as PrimitiveRetargetingShape;

                DistanceResult minResult = Primitives[0].Distance(primitiveShape.Primitives[0]);
                if (Primitives != null && Primitives.Count > 0)
                {
                    if (primitiveShape.Primitives != null && primitiveShape.Primitives.Count > 0)
                    {
                        for (int i = 0; i < Primitives.Count; i++)
                        {
                            for (int j = 0; j < primitiveShape.Primitives.Count; j++)
                            {
                                DistanceResult result = Primitives[i].Distance(primitiveShape.Primitives[j]);

                                if (result.Distance < minResult.Distance)
                                {
                                    minResult = result;
                                }
                            }
                        }
                    }
                }

                return minResult;
            }
            else if (otherShape is MeshRetargetingShape)
            {
                MeshRetargetingShape meshShape = otherShape as MeshRetargetingShape;
                return meshShape.ClosestPoints(this);
            }
            else if (otherShape is SkinRetargetingShape)
            {
                SkinRetargetingShape skinShape = otherShape as SkinRetargetingShape;
                return skinShape.ClosestPoints(this);
            }
            else
            {
                // Debug.LogErrorFormat("Invalid Shape Type {0}. Primitive Shape is only compatible with Point and Primitive Shapes", otherShape.GetType().ToString());
                return new DistanceResult();
            }
        }
    }
}