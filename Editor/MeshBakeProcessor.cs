using System;
using System.Collections.Generic;
using com.superneko.medlay.Editor.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace com.superneko.medlay.Editor
{
    static class Matrix4x4Extensions
    {
        public static Matrix4x4 ScalarMultiply(this Matrix4x4 matrix, float scalar)
        {
            Matrix4x4 result = new Matrix4x4();
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    result[row, col] = matrix[row, col] * scalar;
                }
            }
            return result;
        }

        public static Matrix4x4 Add(this Matrix4x4 a, Matrix4x4 b)
        {
            Matrix4x4 result = new Matrix4x4();
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    result[row, col] = a[row, col] + b[row, col];
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Bakes and unbakes skinned meshes to and from world space.
    /// Please note that MeshBakeProcessor does not handle mesh attributes such as UVs, colors, triangles, etc.
    /// You will need to copy those attributes separately if needed.
    /// Specifically, bindPoses and boneWeights are read but not written back to the mesh so that they must be copied externally.
    /// </summary>
    public class MeshBakeProcessor
    {
        int vertexCount = 0;
        NativeArray<Vector3> vertices;
        NativeArray<Vector3> normals;
        NativeArray<Vector4> tangents;

        List<Matrix4x4> bindPoses = new List<Matrix4x4>();
        List<BoneWeight> tmpBoneWeights = new List<BoneWeight>();
        NativeArray<BoneWeight> boneWeights;
        NativeArray<Matrix4x4> boneMatrices;

        NativeArray<Vector3> totalDeltaVertices;
        NativeArray<Vector3> totalDeltaNormals;
        NativeArray<Vector3> totalDeltaTangents;

        Vector3[] deltaVertices = new Vector3[0];
        Vector3[] deltaNormals = new Vector3[0];
        Vector3[] deltaTangents = new Vector3[0];

        ~MeshBakeProcessor()
        {
            if (vertices.IsCreated) vertices.Dispose();
            if (normals.IsCreated) normals.Dispose();
            if (tangents.IsCreated) tangents.Dispose();
            if (boneWeights.IsCreated) boneWeights.Dispose();
            if (boneMatrices.IsCreated) boneMatrices.Dispose();
            if (totalDeltaVertices.IsCreated) totalDeltaVertices.Dispose();
            if (totalDeltaNormals.IsCreated) totalDeltaNormals.Dispose();
            if (totalDeltaTangents.IsCreated) totalDeltaTangents.Dispose();
        }

        public void ResetArrays(SkinnedMeshRenderer smr, Mesh mesh)
        {
            Profiler.BeginSample("MeshBakeProcessor.ResetArrays");

            Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AcquireAndPopulateMeshData");
            using var meshDataArray = Mesh.AcquireReadOnlyMeshData(mesh);
            var meshData = meshDataArray[0];

            vertexCount = mesh.vertexCount;

            if (vertices.Length != vertexCount)
            {
                if (vertices.IsCreated) vertices.Dispose();
                vertices = new NativeArray<Vector3>(vertexCount, Allocator.Persistent);
            }

            meshData.GetVertices(vertices);

            if (mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Normal) && normals.Length != vertexCount)
            {
                if (normals.IsCreated) normals.Dispose();
                normals = new NativeArray<Vector3>(vertexCount, Allocator.Persistent);
            }

            meshData.GetNormals(normals);

            if (mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent) && tangents.Length != vertexCount)
            {
                if (tangents.IsCreated) tangents.Dispose();
                tangents = new NativeArray<Vector4>(vertexCount, Allocator.Persistent);
            }

            meshData.GetTangents(tangents);

            Profiler.EndSample();

            if (bindPoses.Count != mesh.bindposeCount)
            {
                if (mesh.bindposeCount == 0)
                {

                }

                bindPoses = new List<Matrix4x4>(new Matrix4x4[mesh.bindposeCount]);
            }

            mesh.GetBindposes(bindPoses);

            if (boneWeights.Length != vertexCount)
            {
                Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateBoneWeights");
                if (boneWeights.IsCreated) boneWeights.Dispose();
                boneWeights = new NativeArray<BoneWeight>(vertexCount, Allocator.Persistent);
                tmpBoneWeights = new List<BoneWeight>(new BoneWeight[vertexCount]);
                Profiler.EndSample();
            }

            mesh.GetBoneWeights(tmpBoneWeights);

            boneWeights.CopyFrom(tmpBoneWeights.ToArray());

            if (smr.bones.Length != bindPoses.Count)
            {
                Profiler.EndSample();
                // TODO: Handle
                throw new Exception("Bone count mismatch between SkinnedMeshRenderer and Mesh bindposes.");
            }

            if (smr.bones.Length != boneMatrices.Length)
            {
                Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateBoneMatrices");
                if (boneMatrices.IsCreated) boneMatrices.Dispose();
                boneMatrices = new NativeArray<Matrix4x4>(smr.bones.Length, Allocator.Persistent);
                Profiler.EndSample();
            }

            Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateDeltaArrays");
            if (totalDeltaVertices.Length != vertexCount)
            {
                if (totalDeltaVertices.IsCreated) totalDeltaVertices.Dispose();
                totalDeltaVertices = new NativeArray<Vector3>(vertexCount, Allocator.Persistent);
            }
            if (totalDeltaNormals.Length != vertexCount)
            {
                if (totalDeltaNormals.IsCreated) totalDeltaNormals.Dispose();
                totalDeltaNormals = new NativeArray<Vector3>(vertexCount, Allocator.Persistent);
            }
            if (totalDeltaTangents.Length != vertexCount)
            {
                if (totalDeltaTangents.IsCreated) totalDeltaTangents.Dispose();
                totalDeltaTangents = new NativeArray<Vector3>(vertexCount, Allocator.Persistent);
            }
            if (deltaVertices.Length != vertexCount)
            {
                deltaVertices = new Vector3[vertexCount];
            }
            if (deltaNormals.Length != vertexCount)
            {
                deltaNormals = new Vector3[vertexCount];
            }
            if (deltaTangents.Length != vertexCount)
            {
                deltaTangents = new Vector3[vertexCount];
            }
            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.ResetArrays_CalculateBoneMatrix");
            var bones = smr.bones;
            for (int i = 0; i < bones.Length; i++)
            {
                boneMatrices[i] = bones[i].localToWorldMatrix * bindPoses[i].inverse;
            }
            Profiler.EndSample();

            Profiler.EndSample();
        }

        public void BakeMeshToWorld(Mesh meshToBake, SkinnedMeshRenderer smr, Mesh bakedMesh)
        {
            Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld");

            Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld_Setup");

            ResetArrays(smr, meshToBake);

            Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld_BlendShape");

            int blendShapeCount = bakedMesh.blendShapeCount;

            for (int i = 0; i < blendShapeCount; i++)
            {
                float weight = smr.GetBlendShapeWeight(i);
                if (weight <= 0) continue;
                float weightNormalized = weight / 100f;

                int frameIndex = bakedMesh.GetBlendShapeFrameCount(i) - 1;
                bakedMesh.GetBlendShapeFrameVertices(i, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                for (int v = 0; v < vertexCount; v++)
                {
                    totalDeltaVertices[v] += deltaVertices[v] * weightNormalized;
                    totalDeltaNormals[v] += deltaNormals[v] * weightNormalized;
                    totalDeltaTangents[v] += deltaTangents[v] * weightNormalized;
                }
            }

            Profiler.EndSample();

            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld_VertexProcessing");

            MeshBakeProcessorBurst.BakeMeshToWorld_VertexProcessing(
                vertexCount,
                ref boneWeights,
                ref boneMatrices,
                ref vertices,
                ref normals,
                ref tangents,
                ref totalDeltaVertices,
                ref totalDeltaNormals,
                ref totalDeltaTangents
            );

            Profiler.EndSample();

            bakedMesh.SetVertices(vertices);
            bakedMesh.SetNormals(normals);
            bakedMesh.SetTangents(tangents);

            Profiler.EndSample();
        }

        public void UnBakeMeshFromWorld(Mesh bakedMesh, SkinnedMeshRenderer smr, Mesh unbakedMesh)
        {
            Profiler.BeginSample("MeshBakeProcessor.UnbakeMesh");

            Profiler.BeginSample("MeshBakeProcessor.UnbakeMesh_Setup");

            ResetArrays(smr, bakedMesh);

            Profiler.BeginSample("MeshBakeProcessor.UnbakeMesh_BlendShape");

            int blendShapeCount = bakedMesh.blendShapeCount;

            for (int i = 0; i < blendShapeCount; i++)
            {
                float weight = smr.GetBlendShapeWeight(i);
                if (weight <= 0) continue;
                float weightNormalized = weight / 100f;

                int frameIndex = bakedMesh.GetBlendShapeFrameCount(i) - 1;
                bakedMesh.GetBlendShapeFrameVertices(i, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                for (int v = 0; v < vertexCount; v++)
                {
                    totalDeltaVertices[v] += deltaVertices[v] * weightNormalized;
                    if (normals.Length > 0) totalDeltaNormals[v] += deltaNormals[v] * weightNormalized;
                    if (tangents.Length > 0) totalDeltaTangents[v] += deltaTangents[v] * weightNormalized;
                }
            }

            Profiler.EndSample();

            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.UnbakeMesh_VertexProcessing");

            MeshBakeProcessorBurst.UnbakeMeshToLocal_VertexProcessing(
                vertexCount,
                ref boneWeights,
                ref boneMatrices,
                ref vertices,
                ref normals,
                ref tangents,
                ref totalDeltaVertices,
                ref totalDeltaNormals,
                ref totalDeltaTangents
            );

            Profiler.EndSample();

            unbakedMesh.SetVertices(vertices);
            unbakedMesh.SetNormals(normals);
            unbakedMesh.SetTangents(tangents);

            Profiler.EndSample();
        }
    }
}
