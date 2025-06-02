/*
 * HRTK: MeshRetargetingShape.cs
 *
 * Copyright (c) 2021 Brandon Matthews
 */



#if !MESH_RETARGETING
#define MESH_RETARGETING
#endif

using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting
{
    public class MeshRetargetingShape : RetargetingShape
    {
        public bool ShowTriangles;
        public Material TrianglesMaterial;
        public float TrianglesThickness;
        GameObject showTrianglesObject; 

        int MAX_POINTS = 128;

        [SerializeField]
        MeshFilter _meshFilter;
        public MeshFilter MeshFilter => _meshFilter;
        [SerializeField]
        ComputeShader _compute;
        ComputeShader _computeInstance;

        Mesh _mesh;
        int _triIndexes;
        ComputeBuffer _vertBuffer;
        ComputeBuffer _triBuffer;
        ComputeBuffer _computeResultBuffer;
        DistanceResult[] _triResults;



        private void Start()
        {

            _mesh = _meshFilter.sharedMesh;
            _computeInstance = Instantiate(_compute);

            if (_meshFilter == null || _mesh == null || _compute == null || _computeInstance == null)
            {
                Debug.LogErrorFormat("Mesh Shape {0} requires mesh and distance compute shader to calculate distance", gameObject.name);
                return;
            }

            _vertBuffer = new ComputeBuffer(_mesh.vertexCount, Marshal.SizeOf(typeof(Vector3)));
            _vertBuffer.SetData(_mesh.vertices);

            _triBuffer = new ComputeBuffer(_mesh.triangles.Length, Marshal.SizeOf(typeof(int)));
            _triBuffer.SetData(_mesh.triangles);

            _triIndexes = _triBuffer.count / 3;

            _computeResultBuffer = new ComputeBuffer(_triIndexes, Marshal.SizeOf(typeof(DistanceResult)));

            if (ShowTriangles) DrawTriangles();
        }

        public override DistanceResult ClosestPoints(Vector3[] positions) {
            ComputeKernel kernel;
            kernel = new ComputeKernel(_computeInstance, DistanceShaderParameters.MeshToPointsKernel);
            ConfigureKernel(_computeInstance, kernel, WhichMesh.MeshA);
            _computeInstance.SetVectorArray(DistanceShaderParameters.PointsArray, positions.toVector4Array());
            _computeInstance.SetInt(DistanceShaderParameters.PointsCount, positions.Length);
            return ComputeDistanceResult(kernel);
        }

        public override DistanceResult ClosestPoints(RetargetingShape otherShape)
        {
            if (_meshFilter == null || _mesh == null || _compute == null || _computeInstance == null)
            {
                Debug.LogErrorFormat("Mesh Shape {0} requires mesh and distance compute shader to calculate distance", gameObject.name);
                return new DistanceResult();
            }

            ComputeKernel kernel;

            bool swapResults = false;

            if (otherShape is PointRetargetingShape)
            {
                PointRetargetingShape pointShape = otherShape as PointRetargetingShape;

                int pointCount = Mathf.Min(pointShape.Points.Count, MAX_POINTS);
                Vector3[] points = new Vector3[pointCount];
                for (int i = 0; i < pointCount; i++) {
                    points[i] = pointShape.Points[i].position;
                }
                
                return ClosestPoints(points);
            }
            else if (otherShape is MeshRetargetingShape)
            {
                MeshRetargetingShape meshShape = otherShape as MeshRetargetingShape;
                kernel = new ComputeKernel(_computeInstance, DistanceShaderParameters.MeshToMeshKernel);
                ConfigureKernel(_computeInstance, kernel, WhichMesh.MeshA);
                meshShape.ConfigureKernel(_computeInstance, kernel, WhichMesh.MeshB);
            }
            else if (otherShape is SkinRetargetingShape)
            {
                SkinRetargetingShape skinShape = otherShape as SkinRetargetingShape;
                kernel = new ComputeKernel(_computeInstance, DistanceShaderParameters.SkinToMeshKernel);
                skinShape.ConfigureKernel(_computeInstance, kernel, WhichMesh.MeshA);
                ConfigureKernel(_computeInstance, kernel, WhichMesh.MeshB);
                swapResults = true;
            }
            else if (otherShape is PrimitiveRetargetingShape) {
                PrimitiveRetargetingShape primitiveShape = otherShape as PrimitiveRetargetingShape;
                
                kernel = new ComputeKernel(_computeInstance, DistanceShaderParameters.MeshToPrimitivesKernel);
                ConfigureKernel(_computeInstance, kernel, WhichMesh.MeshA);

                ComputeBuffer primitiveBuffer;
                int bufferSize = primitiveShape.GetPrimitiveBuffer(out primitiveBuffer);

                if (bufferSize > 0) {
                    
                    _computeInstance.SetBuffer(kernel.Index, DistanceShaderParameters.PrimitiveBuffer, primitiveBuffer);
                    _computeInstance.SetInt(DistanceShaderParameters.PrimitiveCount, bufferSize);
                } else {
                    Debug.LogWarning("No Valid Primitives, Distance could not be computed");
                    return new DistanceResult();
                }
            }
            else
            {
                //Debug.LogErrorFormat("Invalid Shape Type {0}. Mesh Shape is only compatible with Point, Mesh and Skin Shapes", otherShape.GetType().ToString());
                return new DistanceResult();
            }

            DistanceResult nearResult = ComputeDistanceResult(kernel);
            
            if (swapResults) {
                Vector3 temp = nearResult.pointA;
                nearResult.pointA = nearResult.pointB;
                nearResult.pointB = temp;
            }

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
            if (_meshFilter == null || _mesh == null)
            {
                Debug.LogErrorFormat("Mesh Shape {0} requires mesh to configure compute kernel", gameObject.name);
                return;
            }

            shader.SetBuffer(kernel.Index, DistanceShaderParameters.MeshVertexBuffer(whichMesh), _vertBuffer);
            shader.SetBuffer(kernel.Index, DistanceShaderParameters.MeshTriangleBuffer(whichMesh), _triBuffer);
            shader.SetInt(DistanceShaderParameters.MeshTriangleCount(whichMesh), _triBuffer.count);
            shader.SetInt(DistanceShaderParameters.MeshTriangleIndexes(whichMesh), _triIndexes);
            shader.SetMatrix(DistanceShaderParameters.MeshTransformation(whichMesh), _meshFilter.transform.localToWorldMatrix);
        }

        public void DrawTriangles() {
            if (showTrianglesObject == null) {
                showTrianglesObject = new GameObject("Triangle Visualisation");
                showTrianglesObject.transform.parent = _meshFilter.transform;
                showTrianglesObject.transform.localScale = Vector3.one;
                showTrianglesObject.transform.localPosition = Vector3.zero;
                showTrianglesObject.transform.localRotation = Quaternion.identity;
                showTrianglesObject.AddComponent<MeshRenderer>();
                showTrianglesObject.AddComponent<MeshFilter>();
            }

            Material temp = Instantiate(TrianglesMaterial);
            temp.SetFloat("_WireframeVal", TrianglesThickness);
            MeshRenderer mr = showTrianglesObject.GetComponent<MeshRenderer>();
            MeshFilter mf = showTrianglesObject.GetComponent<MeshFilter>();


            mr.material = temp;
            mf.mesh = _meshFilter.mesh;
        }
    }
}