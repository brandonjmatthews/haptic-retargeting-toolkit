/*
 * HRTK: MeshComputeHelpers.cs
 *
 * Copyright (c) 2023 Brandon Matthews
 */

using UnityEngine;

namespace HRTK.Modules.ShapeRetargeting {

    public enum WhichMesh {
        MeshA,
        MeshB
    }

    public static class Vector3Extension {
        
        public static Vector4[] toVector4Array (this Vector3[] v3)
        {
            return System.Array.ConvertAll<Vector3, Vector4> (v3, toVector4);
        }

        public static Vector4 toVector4 (Vector3 v3)
        {
            return new Vector4 (v3.x, v3.y, v3.z, 0.0f);
        }
    }

    public static class DistanceShaderParameters {
        // Kernels
        public static string MeshToPointsKernel = "MeshToPoints";
        public static string SkinToPointsKernel = "SkinToPoints";
        public static string MeshToMeshKernel = "MeshToMesh";
        public static string SkinToMeshKernel = "SkinToMesh";
        public static string SkinToSkinKernel = "SkinToSkin";

        public static string SkinToPrimitivesKernel = "SkinToPrimitives";
        public static string MeshToPrimitivesKernel = "MeshToPrimitives";


        public static string PointsArray = "_Points";
        public static string PointsCount = "_PointsCount";

        public static string PrimitiveBuffer = "_Primitives";
        public static string PrimitiveCount = "_PrimitiveCount";

        public static string MeshVertexBuffer(WhichMesh w) {
            if (w == WhichMesh.MeshA) return "_MeshVertBufferA";
            else return "_MeshVertBufferB";
        }

        public static string MeshTriangleBuffer(WhichMesh w) {
            if (w == WhichMesh.MeshA) return "_MeshTriBufferA";
            else return "_MeshTriBufferB";
        }

        public static string MeshTriangleIndexes(WhichMesh w) {
            if (w == WhichMesh.MeshA) return "_MeshTriangleIndexesA";
            else return "_MeshTriangleIndexesB";
        }

        public static string MeshTriangleCount(WhichMesh w) {
            if (w == WhichMesh.MeshA) return "_MeshTriangleCountA";
            else return "_MeshTriangleCountB";
        }

        public static string MeshTransformation(WhichMesh w) {
            if (w == WhichMesh.MeshA) return "_MeshTransformationA";
            else return "_MeshTransformationB";
        }

        public static string SkinnedWeights(WhichMesh w) {
            if (w == WhichMesh.MeshA) return "_SkinnedWeightsA";
            else return "_SkinnedWeightsB";
        }

        public static string SkinnedBoneMatrices(WhichMesh w) {
            if (w == WhichMesh.MeshA) return "_SkinnedBoneMatricesA";
            else return "_SkinnedBoneMatricesB";
        }
    }

    public class ComputeKernel
    {
        public int Index { get { return index; } }
        public uint ThreadX { get { return threadX; } }
        public uint ThreadY { get { return threadY; } }
        public uint ThreadZ { get { return threadZ; } }

        int index;
        uint threadX, threadY, threadZ;

        public ComputeKernel(ComputeShader shader, string key)
        {
            index = shader.FindKernel(key);

            if (index < 0)
            {
                Debug.LogWarning("Can't find kernel");
                return;
            }

            shader.GetKernelThreadGroupSizes(index, out threadX, out threadY, out threadZ);
        }
    }

    public class ComputeProgram {
        public ComputeShader ShaderInstance;
        public ComputeKernel Kernel;
    }

    public struct Weight
    {
        public int BoneIndex0;
        public float Weight0;
        public int BoneIndex1;
        public float Weight1;
        public int BoneIndex2;
        public float Weight2;
        public int BoneIndex3;
        public float Weight3;
    }

    public struct ComputePrimitive {
        public int type;
        public float radius;
        public float extraFloat;
        public Vector3 position;
        public Vector3 extraVector;
        public Matrix4x4 transform;
    }

}