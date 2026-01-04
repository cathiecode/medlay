using System;
using System.Collections.Generic;
using com.superneko.medlay.Core.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Mathematics;

namespace com.superneko.medlay.Core
{
    /// <summary>
    /// Bakes and unbakes skinned meshes to and from world space.
    /// Please note that MeshBakeProcessor does not handle mesh attributes such as UVs, colors, triangles, etc.
    /// You will need to copy those attributes separately if needed.
    /// Specifically, bindPoses and boneWeights are read but not written back to the mesh so that they must be copied externally.
    /// </summary>
    public class MeshBakeProcessor
    {
        int vertexCount = 0;

        List<Matrix4x4> bindPoses = new List<Matrix4x4>();
        List<BoneWeight> tmpBoneWeights = new List<BoneWeight>();
        NativeArray<BoneWeight> boneWeights;
        NativeArray<float4x4> boneMatrices;

        NativeArray<float3> totalDeltaVertices;
        NativeArray<float3> totalDeltaNormals;
        NativeArray<float3> totalDeltaTangents;

        Vector3[] deltaVertices = new Vector3[0];
        Vector3[] deltaNormals = new Vector3[0];
        Vector3[] deltaTangents = new Vector3[0];

        ~MeshBakeProcessor()
        {
            if (boneWeights.IsCreated) boneWeights.Dispose();
            if (boneMatrices.IsCreated) boneMatrices.Dispose();
            if (totalDeltaVertices.IsCreated) totalDeltaVertices.Dispose();
            if (totalDeltaNormals.IsCreated) totalDeltaNormals.Dispose();
            if (totalDeltaTangents.IsCreated) totalDeltaTangents.Dispose();
        }

        void ResetArrays(MedlayWritableMeshData meshData, Renderer renderer)
        {
            Profiler.BeginSample("MeshBakeProcessor.ResetArrays");

            vertexCount = meshData.vertexCount;

            Profiler.BeginSample("MeshBakeProcessor.ResetArrays_BindPoses");
            if (meshData.BaseMesh.bindposeCount == 0)
            {
                bindPoses = new List<Matrix4x4>(new Matrix4x4[] { Matrix4x4.identity });
            }
            else
            {
                if (bindPoses.Count != meshData.BaseMesh.bindposeCount)
                {
                    bindPoses = new List<Matrix4x4>(new Matrix4x4[meshData.BaseMesh.bindposeCount]);
                }

                meshData.BaseMesh.GetBindposes(bindPoses);
            }
            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.ResetArrays_BoneWeights");
            if (boneWeights.Length != vertexCount)
            {
                Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateBoneWeights");
                if (boneWeights.IsCreated) boneWeights.Dispose();
                boneWeights = new NativeArray<BoneWeight>(vertexCount, Allocator.Persistent);
                tmpBoneWeights = new List<BoneWeight>(new BoneWeight[vertexCount]);
                Profiler.EndSample();
            }

            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.ResetArrays_GetBoneWeights");
            meshData.BaseMesh.GetBoneWeights(tmpBoneWeights);
            for (int i = 0; i < tmpBoneWeights.Count; i++)
            {
                boneWeights[i] = tmpBoneWeights[i];
            }
            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.ResetArrays_BoneMatrices");
            if (renderer is SkinnedMeshRenderer)
            {
                var smr = renderer as SkinnedMeshRenderer;

                if (smr.bones.Length != bindPoses.Count)
                {
                    if (bindPoses.Count == 1)
                    {
                        // Static mesh assigned to SMR
                        if (boneMatrices.IsCreated) boneMatrices.Dispose();
                        boneMatrices = new NativeArray<float4x4>(1, Allocator.Persistent);
                        boneMatrices[0] = renderer.transform.localToWorldMatrix;
                    }
                    else
                    {
                        Profiler.EndSample();
                        // TODO: Handle
                        throw new Exception("Bone count mismatch between SkinnedMeshRenderer and Mesh bindposes.");
                    }
                }
                else
                {
                    if (smr.bones.Length != boneMatrices.Length)
                    {
                        Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateBoneMatrices");
                        if (boneMatrices.IsCreated) boneMatrices.Dispose();
                        boneMatrices = new NativeArray<float4x4>(smr.bones.Length, Allocator.Persistent);
                        Profiler.EndSample();
                    }

                    var bones = smr.bones; // Allocation

                    for (int i = 0; i < bones.Length; i++)
                    {
                        var bone = bones[i];
                        if (bone == null)
                        {
                            boneMatrices[i] = Matrix4x4.identity;
                        }
                        else
                        {
                            boneMatrices[i] = bones[i].localToWorldMatrix * bindPoses[i];
                        }
                    }
                }
            }
            else
            {
                if (boneMatrices.IsCreated) boneMatrices.Dispose();

                boneMatrices = new NativeArray<float4x4>(bindPoses.Count, Allocator.Persistent);

                for (int i = 0; i < boneMatrices.Length; i++)
                {
                    boneMatrices[i] = renderer.transform.localToWorldMatrix;
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateDeltaArrays");
            if (totalDeltaVertices.Length != vertexCount)
            {
                if (totalDeltaVertices.IsCreated) totalDeltaVertices.Dispose();
                totalDeltaVertices = new NativeArray<float3>(vertexCount, Allocator.Persistent);
            }
            if (totalDeltaNormals.Length != vertexCount)
            {
                if (totalDeltaNormals.IsCreated) totalDeltaNormals.Dispose();
                totalDeltaNormals = new NativeArray<float3>(vertexCount, Allocator.Persistent);
            }
            if (totalDeltaTangents.Length != vertexCount)
            {
                if (totalDeltaTangents.IsCreated) totalDeltaTangents.Dispose();
                totalDeltaTangents = new NativeArray<float3>(vertexCount, Allocator.Persistent);
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

            Profiler.EndSample();
        }

        public void BakeMeshToWorld(MedlayWritableMeshData meshData, Renderer renderer)
        {
            Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld");

            Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld_Setup");

            var vertices = meshData.GetVertices();
            var normals = meshData.GetNormals();
            var tangents = meshData.GetTangents();

            ResetArrays(meshData, renderer);

            if (renderer is SkinnedMeshRenderer)
            {
                Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld_BlendShape");

                var smr = renderer as SkinnedMeshRenderer;

                int blendShapeCount = meshData.BaseMesh.blendShapeCount;

                for (int i = 0; i < blendShapeCount; i++)
                {
                    if (i == 0)
                    {
                        for (int v = 0; v < vertexCount; v++)
                        {
                            totalDeltaVertices[v] = float3.zero;
                            totalDeltaNormals[v] = float3.zero;
                            totalDeltaTangents[v] = float3.zero;
                        }
                    }

                    float weight = smr.GetBlendShapeWeight(i);
                    if (weight <= 0) continue;
                    float weightNormalized = weight / 100f;

                    int frameIndex = meshData.BaseMesh.GetBlendShapeFrameCount(i) - 1;
                    meshData.BaseMesh.GetBlendShapeFrameVertices(i, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                    for (int v = 0; v < vertexCount; v++)
                    {
                        totalDeltaVertices[v] += (float3)(deltaVertices[v] * weightNormalized);
                        totalDeltaNormals[v] += (float3)(deltaNormals[v] * weightNormalized);
                        totalDeltaTangents[v] += (float3)(deltaTangents[v] * weightNormalized);
                    }
                }

                Profiler.EndSample();
            }

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

            Profiler.EndSample();
        }

        public void UnBakeMeshFromWorld(MedlayWritableMeshData meshData, Renderer renderer)
        {
            Profiler.BeginSample("MeshBakeProcessor.UnbakeMesh");

            Profiler.BeginSample("MeshBakeProcessor.UnbakeMesh_Setup");

            var vertices = meshData.GetVertices();
            var normals = meshData.GetNormals();
            var tangents = meshData.GetTangents();

            ResetArrays(meshData, renderer);

            if (renderer is SkinnedMeshRenderer)
            {
                var smr = renderer as SkinnedMeshRenderer;

                Profiler.BeginSample("MeshBakeProcessor.UnbakeMesh_BlendShape");

                int blendShapeCount = meshData.BaseMesh.blendShapeCount;
                for (int i = 0; i < blendShapeCount; i++)
                {
                    float weight = smr.GetBlendShapeWeight(i);
                    if (weight <= 0) continue;
                    float weightNormalized = weight / 100f;

                    int frameIndex = meshData.BaseMesh.GetBlendShapeFrameCount(i) - 1;
                    meshData.BaseMesh.GetBlendShapeFrameVertices(i, frameIndex, deltaVertices, deltaNormals, deltaTangents);

                    for (int v = 0; v < vertexCount; v++)
                    {
                        totalDeltaVertices[v] += (float3)(deltaVertices[v] * weightNormalized);
                        if (normals.Length > 0) totalDeltaNormals[v] += (float3)(deltaNormals[v] * weightNormalized);
                        if (tangents.Length > 0) totalDeltaTangents[v] += (float3)(deltaTangents[v] * weightNormalized);
                    }
                }

                Profiler.EndSample();
            }

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

            Profiler.EndSample();
        }
    }
}
