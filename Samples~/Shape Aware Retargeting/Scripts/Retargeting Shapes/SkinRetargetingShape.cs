/**
Haptic Retargeting Took-kit Prototype
SkinRetargetingShape.cs

@author: Brandon Matthews, 2021
*/


using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting
{
    public class SkinRetargetingShape : RetargetingShape
    {
        public bool ShowTriangles;
        public Material TrianglesMaterial;
        public float TrianglesThickness;
        GameObject showTrianglesObject; 
        int MAX_POINTS = 128;

        [SerializeField]
        SkinnedMeshRenderer _skin;
        [SerializeField]
        ComputeShader _compute;
        ComputeShader _computeInstance;

        Mesh _mesh;

        int _triIndexes;
        ComputeBuffer _vertBuffer;
        ComputeBuffer _triBuffer;
        ComputeBuffer _weightBuffer;
        ComputeBuffer _computeResultBuffer;
        ComputeBuffer _pointBuffer;
        DistanceResult[] _triResults;

        Matrix4x4[] _boneMatrices;

        ComputeBuffer _primitiveBuffer;


        Dictionary<PointRetargetingShape, ComputeBuffer> _pointBufferCache;

        private void Start()
        {
            _mesh = _skin.sharedMesh;
            _computeInstance = Instantiate(_compute);

            if (_skin == null || _mesh == null || _compute == null || _computeInstance == null)
            {
                Debug.LogErrorFormat("Skin Shape {0} requires skin and distance compute shader to calculate distance", gameObject.name);
                return;
            }

            _pointBufferCache = new Dictionary<PointRetargetingShape, ComputeBuffer>();

            _vertBuffer = new ComputeBuffer(_mesh.vertexCount, Marshal.SizeOf(typeof(Vector3)));
            _vertBuffer.SetData(_mesh.vertices);

            _triBuffer = new ComputeBuffer(_mesh.triangles.Length, Marshal.SizeOf(typeof(int)));
            _triBuffer.SetData(_mesh.triangles);

            Weight[] weights = new Weight[_mesh.boneWeights.Length];
            for (int i = 0; i < _mesh.boneWeights.Length; i++)
            {
                BoneWeight bw = _mesh.boneWeights[i];
                Weight w = new Weight();
                w.Weight0 = bw.weight0;
                w.Weight1 = bw.weight1;
                w.Weight2 = bw.weight2;
                w.Weight3 = bw.weight3;

                w.BoneIndex0 = bw.boneIndex0;
                w.BoneIndex1 = bw.boneIndex1;
                w.BoneIndex2 = bw.boneIndex2;
                w.BoneIndex3 = bw.boneIndex3;
                weights[i] = w;
            }

            _weightBuffer = new ComputeBuffer(_mesh.boneWeights.Length, Marshal.SizeOf(typeof(BoneWeight)));
            _weightBuffer.SetData(weights);
            _triIndexes = _triBuffer.count / 3;

            _computeResultBuffer = new ComputeBuffer(_triIndexes, Marshal.SizeOf(typeof(DistanceResult)));

            if (ShowTriangles) DrawTriangles();
        }

        public override DistanceResult ClosestPoints(Vector3[] positions) {
            ComputeKernel kernel;
            kernel = new ComputeKernel(_computeInstance, DistanceShaderParameters.SkinToPointsKernel);
            ConfigureKernel(_computeInstance, kernel, WhichMesh.MeshA);
            _computeInstance.SetVectorArray(DistanceShaderParameters.PointsArray, positions.toVector4Array());
            _computeInstance.SetInt(DistanceShaderParameters.PointsCount, positions.Length);

            return ComputeDistanceResult(kernel);
        }

        public override DistanceResult ClosestPoints(RetargetingShape otherShape)
        {
            if (_skin == null || _mesh == null || _compute == null || _computeInstance == null)
            {
                Debug.LogErrorFormat("Skin Shape {0} requires skin and distance compute shader to calculate distance", gameObject.name);
                return new DistanceResult();
            }

            ComputeKernel kernel;

            if (otherShape is PointRetargetingShape)
            {
                PointRetargetingShape pointShape = otherShape as PointRetargetingShape;

                Vector3[] points = new Vector3[MAX_POINTS];
                int pointCount = Mathf.Min(pointShape.Points.Count, MAX_POINTS);
                for (int i = 0; i < pointCount; i++) {
                    points[i] = pointShape.Points[i].position;
                }

                return ClosestPoints(points);
            }
            else if (otherShape is MeshRetargetingShape)
            {
                MeshRetargetingShape meshShape = otherShape as MeshRetargetingShape;
                kernel = new ComputeKernel(_computeInstance, DistanceShaderParameters.SkinToMeshKernel);
                ConfigureKernel(_computeInstance, kernel, WhichMesh.MeshA);
                meshShape.ConfigureKernel(_computeInstance, kernel, WhichMesh.MeshB);
            }
            else if (otherShape is SkinRetargetingShape)
            {
                SkinRetargetingShape skinShape = otherShape as SkinRetargetingShape;
                kernel = new ComputeKernel(_computeInstance, DistanceShaderParameters.SkinToSkinKernel);
                ConfigureKernel(_computeInstance, kernel, WhichMesh.MeshA);
                skinShape.ConfigureKernel(_computeInstance, kernel, WhichMesh.MeshB);
            }            
            else if (otherShape is PrimitiveRetargetingShape) {
                PrimitiveRetargetingShape primitiveShape = otherShape as PrimitiveRetargetingShape;
                
                kernel = new ComputeKernel(_computeInstance, DistanceShaderParameters.SkinToPrimitivesKernel);
                ConfigureKernel(_computeInstance, kernel, WhichMesh.MeshA);
                ComputeBuffer primitiveBuffer;
                int bufferSize = primitiveShape.GetPrimitiveBuffer(out primitiveBuffer);

                if (bufferSize > 0)
                {
                    _computeInstance.SetBuffer(kernel.Index, DistanceShaderParameters.PrimitiveBuffer, primitiveBuffer);
                    _computeInstance.SetInt(DistanceShaderParameters.PrimitiveCount, bufferSize);
                } else {
                    Debug.LogWarning("No Valid Primitives, Distance could not be computed");
                    return new DistanceResult();
                }
            }
            else
            {
                //Debug.LogErrorFormat("Invalid Shape Type {0}. Skinned Mesh Shape is only compatible with Point, Mesh and Skin Shapes", otherShape.GetType().ToString());
                return new DistanceResult();
            }

            DistanceResult nearResult = ComputeDistanceResult(kernel);
            return nearResult;       
        }

        public DistanceResult ComputeDistanceResult(ComputeKernel kernel) {
            _computeInstance.SetBuffer(kernel.Index, "_ResultBuffer", _computeResultBuffer);
        
            _computeInstance.Dispatch(kernel.Index, _triIndexes / (int)kernel.ThreadX + 1, (int)kernel.ThreadY, (int)kernel.ThreadZ);

            _triResults = new DistanceResult[_triIndexes];
            _computeResultBuffer.GetData(_triResults);

            DistanceResult nearResult = _triResults[0];

            for (int i = 1; i < _triResults.Length; i++)
            {
                if (_triResults[i].Distance < nearResult.Distance)
                {
                    nearResult = _triResults[i];

                }
            }

            return nearResult;
        }

        public void ConfigureKernel(ComputeShader shader, ComputeKernel kernel, WhichMesh whichMesh)
        {
            if (_skin == null || _mesh == null)
            {
                Debug.LogErrorFormat("Skin Shape {0} requires skin to configure compute kernel", gameObject.name);
                return;
            }

            shader.SetBuffer(kernel.Index, DistanceShaderParameters.MeshVertexBuffer(whichMesh), _vertBuffer);
            shader.SetBuffer(kernel.Index, DistanceShaderParameters.MeshTriangleBuffer(whichMesh), _triBuffer);
            shader.SetInt(DistanceShaderParameters.MeshTriangleCount(whichMesh), _triBuffer.count);
            shader.SetInt(DistanceShaderParameters.MeshTriangleIndexes(whichMesh), _triIndexes);

            Matrix4x4[] boneMatrices = new Matrix4x4[_skin.bones.Length];
            for (int i = 0; i < boneMatrices.Length; i++)
                boneMatrices[i] = _skin.bones[i].localToWorldMatrix * _mesh.bindposes[i];

            shader.SetMatrixArray(DistanceShaderParameters.SkinnedBoneMatrices(whichMesh), boneMatrices);
            shader.SetBuffer(kernel.Index, DistanceShaderParameters.SkinnedWeights(whichMesh), _weightBuffer);
        }



        public void DrawTriangles() {
            if (showTrianglesObject == null) {
                showTrianglesObject = Instantiate(_skin.gameObject, transform);
            }

            Material temp = Instantiate(TrianglesMaterial);
            temp.SetFloat("_WireframeVal", TrianglesThickness);
            SkinnedMeshRenderer smr = showTrianglesObject.GetComponent<SkinnedMeshRenderer>();
            Material[] materials = smr.materials;
            materials[0] = temp;
            smr.materials = materials;
        }
    }
}