using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Mathematics;

namespace com.superneko.medlay.Core
{
    using com.superneko.medlay.Core.Internal.Unsafe;
    using Internal.Burst;
    using UnityEngine.Rendering;

    /// <summary>
    /// Bakes and unbakes skinned meshes to and from world space.
    /// Please note that MeshBakeProcessor does not handle mesh attributes such as UVs, colors, triangles, etc.
    /// You will need to copy those attributes separately if needed.
    /// Specifically, bindPoses and boneWeights are read but not written back to the mesh so that they must be copied externally.
    /// </summary>
    public sealed class MeshBakeProcessor
    {
        struct BlendShapeKey
        {
            int blendShapeId;
            int frameIndex;
        }

        public struct Assumptions
        {
            public bool boneMatricesAreNotChanged;
        }

        struct BlendShapeContainer : IDisposable
        {
            public NativeArray<float3> deltaVertices;
            public NativeArray<float3> deltaNormals;
            public NativeArray<float3> deltaTangents;

            public BlendShapeContainer(int vertexCount)
            {
                deltaVertices = new NativeArray<float3>(vertexCount, Allocator.Persistent);
                deltaNormals = new NativeArray<float3>(vertexCount, Allocator.Persistent);
                deltaTangents = new NativeArray<float3>(vertexCount, Allocator.Persistent);
            }

            void IDisposable.Dispose()
            {
                if (deltaVertices.IsCreated) deltaVertices.Dispose();
                if (deltaNormals.IsCreated) deltaNormals.Dispose();
                if (deltaTangents.IsCreated) deltaTangents.Dispose();
            }
        }

        int vertexCount = 0;

        NativeArray<float4x4> boneMatrices;

        NativeArray<float3> totalDeltaVertices;
        NativeArray<float3> totalDeltaNormals;
        NativeArray<float3> totalDeltaTangents;

        NativeArray<float3> referenceBakedVertices;
        NativeArray<float3> referenceBakedNormals;
        NativeArray<float4> referenceBakedTangents;
        NativeArray<float3> referenceUnbakedVertices;
        NativeArray<float3> referenceUnbakedNormals;
        NativeArray<float4> referenceUnbakedTangents;

        DisposableCache<(int, int), BlendShapeContainer> blendShapeCache = new DisposableCache<(int, int), BlendShapeContainer>();

        float[] blendShapeWeights;

        internal MeshBakeProcessor()
        {

        }

        ~MeshBakeProcessor()
        {
            if (boneMatrices.IsCreated) boneMatrices.Dispose();
            if (totalDeltaVertices.IsCreated) totalDeltaVertices.Dispose();
            if (totalDeltaNormals.IsCreated) totalDeltaNormals.Dispose();
            if (totalDeltaTangents.IsCreated) totalDeltaTangents.Dispose();
            if (referenceBakedVertices.IsCreated) referenceBakedVertices.Dispose();
            if (referenceBakedNormals.IsCreated) referenceBakedNormals.Dispose();
            if (referenceBakedTangents.IsCreated) referenceBakedTangents.Dispose();
            if (referenceUnbakedVertices.IsCreated) referenceUnbakedVertices.Dispose();
            if (referenceUnbakedNormals.IsCreated) referenceUnbakedNormals.Dispose();
            if (referenceUnbakedTangents.IsCreated) referenceUnbakedTangents.Dispose();
            blendShapeCache.Clear();
        }

        void ResetArrays(MedlayWritableMeshData meshData, Renderer renderer, Transform[] bones, Matrix4x4 worldToBaseMatrix, Assumptions assumptions = default)
        {
            Profiler.BeginSample("MeshBakeProcessor.ResetArrays");

            vertexCount = meshData.vertexCount;

            Profiler.BeginSample("MeshBakeProcessor.ResetArrays_BoneMatrices");
            if (!assumptions.boneMatricesAreNotChanged)
            {
                NativeArray<float4x4> bindPoses;

                if (meshData.BaseMesh.bindposeCount == 0)
                {
                    bindPoses = new NativeArray<float4x4>(1, Allocator.Temp);
                    bindPoses[0] = float4x4.identity;
                }
                else
                {
                    bindPoses = meshData.GetBindposesReadOnly();
                }

                if (bones != null)
                {
                    if (bones.Length != bindPoses.Length)
                    {
                        if (bindPoses.Length == 1)
                        {
                            // Static mesh assigned to SMR
                            ReallocateIfNeeded(ref boneMatrices, 1);
                            boneMatrices[0] = worldToBaseMatrix * renderer.transform.localToWorldMatrix;
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
                        ReallocateIfNeeded(ref boneMatrices, bones.Length);

                        Profiler.BeginSample("MeshBakeProcessor.ResetArrays_BoneMatrices_Calculate");

                        for (int i = 0; i < bones.Length; i++)
                        {
                            var bone = bones[i];
                            if (bone == null)
                            {
                                boneMatrices[i] = float4x4.identity;
                            }
                            else
                            {
                                // NOTE: float4x4 * is CROSS PRODUCT.
                                boneMatrices[i] = worldToBaseMatrix * bone.localToWorldMatrix * (Matrix4x4)bindPoses[i];
                            }
                        }

                        Profiler.EndSample();
                    }
                }
                else
                {
                    ReallocateIfNeeded(ref boneMatrices, bindPoses.Length);

                    for (int i = 0; i < boneMatrices.Length; i++)
                    {
                        boneMatrices[i] = worldToBaseMatrix * renderer.transform.localToWorldMatrix;
                    }
                }
                bindPoses.Dispose();
            }
            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateDeltaArrays");
            ReallocateIfNeeded(ref totalDeltaVertices, vertexCount);
            ReallocateIfNeeded(ref totalDeltaNormals, vertexCount);
            ReallocateIfNeeded(ref totalDeltaTangents, vertexCount);
            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateReferenceArrays");
            ReallocateIfNeeded(ref referenceBakedVertices, vertexCount);
            ReallocateIfNeeded(ref referenceBakedNormals, vertexCount);
            ReallocateIfNeeded(ref referenceBakedTangents, vertexCount);
            ReallocateIfNeeded(ref referenceUnbakedVertices, vertexCount);
            ReallocateIfNeeded(ref referenceUnbakedNormals, vertexCount);
            ReallocateIfNeeded(ref referenceUnbakedTangents, vertexCount);
            Profiler.EndSample();

            Profiler.EndSample();

            // TODO: Blendshape invalidation
        }

        public void BakeMeshToBase(MedlayWritableMeshData meshData, Renderer renderer, Transform[] bones = null)
        {
            BakeMeshToBase(meshData, renderer, bones, Matrix4x4.identity);
        }

        public void BakeMeshToBase(MedlayWritableMeshData meshData, Renderer renderer, Transform[] bones, Matrix4x4 worldToBaseMatrix)
        {
            Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld");

            Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld_Setup");

            var vertices = MeshDataUtils.Reinterpret(meshData.GetVertices());
            var normals = meshData.HasVertexAttribute(VertexAttribute.Normal) ? MeshDataUtils.Reinterpret(meshData.GetNormals()) : default;
            var tangents = meshData.HasVertexAttribute(VertexAttribute.Tangent) ? MeshDataUtils.Reinterpret(meshData.GetTangents()) : default;

            ResetArrays(meshData, renderer, bones, worldToBaseMatrix);

            if (renderer is SkinnedMeshRenderer)
            {
                var smr = renderer as SkinnedMeshRenderer;

                UpdateTotalDelta(meshData, smr);
            }

            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld_StoreUnbakedReference");
            referenceUnbakedVertices.CopyFrom(vertices);
            if (normals.IsCreated) referenceUnbakedNormals.CopyFrom(normals);
            if (tangents.IsCreated) referenceUnbakedTangents.CopyFrom(tangents);
            Profiler.EndSample();

            var allBoneWeights = meshData.GetAllBoneWeightsReadOnly();
            var bonesPerVertex = meshData.GetBonesPerVertexReadOnly();

            Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld_VertexProcessing");

            MeshBakeProcessorBurst.BakeMeshToWorld_VertexProcessing(
                vertexCount,
                ref allBoneWeights,
                ref bonesPerVertex,
                ref boneMatrices,
                ref vertices,
                ref normals,
                ref tangents,
                ref totalDeltaVertices,
                ref totalDeltaNormals,
                ref totalDeltaTangents
            );

            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld_StoreBakedReference");
            referenceBakedVertices.CopyFrom(vertices);
            if (normals.IsCreated) referenceBakedNormals.CopyFrom(normals);
            if (tangents.IsCreated) referenceBakedTangents.CopyFrom(tangents);
            Profiler.EndSample();

            Profiler.EndSample();

            blendShapeCache.Tick();
        }

        public void UnBakeMeshFromBase(MedlayWritableMeshData meshData, Renderer renderer, Transform[] bones = null)
        {
            UnBakeMeshFromBase(meshData, renderer, bones, Matrix4x4.identity);
        }

        public void UnBakeMeshFromBase(MedlayWritableMeshData meshData, Renderer renderer, Transform[] bones, Matrix4x4 worldToBaseMatrix, Assumptions assumptions = default)
        {
            Profiler.BeginSample("MeshBakeProcessor.UnbakeMesh");

            Profiler.BeginSample("MeshBakeProcessor.UnbakeMesh_Setup");

            var vertices = MeshDataUtils.Reinterpret(meshData.GetVertices());
            var normals = meshData.HasVertexAttribute(VertexAttribute.Normal) ? MeshDataUtils.Reinterpret(meshData.GetNormals()) : default;
            var tangents = meshData.HasVertexAttribute(VertexAttribute.Tangent) ? MeshDataUtils.Reinterpret(meshData.GetTangents()) : default;

            ResetArrays(meshData, renderer, bones, worldToBaseMatrix, assumptions);

            if (renderer is SkinnedMeshRenderer)
            {
                var smr = renderer as SkinnedMeshRenderer;

                UpdateTotalDelta(meshData, smr);
            }

            Profiler.EndSample();

            var allBoneWeights = meshData.GetAllBoneWeightsReadOnly();
            var bonesPerVertex = meshData.GetBonesPerVertexReadOnly();

            Profiler.BeginSample("MeshBakeProcessor.UnbakeMesh_VertexProcessing");

            MeshBakeProcessorBurst.UnbakeMeshToLocal_VertexProcessing(
                vertexCount,
                ref allBoneWeights,
                ref bonesPerVertex,
                ref boneMatrices,
                ref vertices,
                ref normals,
                ref tangents,
                ref totalDeltaVertices,
                ref totalDeltaNormals,
                ref totalDeltaTangents,
                ref referenceBakedVertices,
                ref referenceBakedNormals,
                ref referenceBakedTangents,
                ref referenceUnbakedVertices,
                ref referenceUnbakedNormals,
                ref referenceUnbakedTangents
            );

            Profiler.EndSample();

            Profiler.EndSample();

            blendShapeCache.Tick();
        }

        void UpdateTotalDelta(MedlayWritableMeshData meshData, SkinnedMeshRenderer smr)
        {
            Profiler.BeginSample("MeshBakeProcessor.UpdateTotalDelta");
            int blendShapeCount = meshData.BaseMesh.blendShapeCount;

            if (!IsBlendShapesChanged(meshData, smr))
            {
                Profiler.EndSample();
                return;
            }

            MeshBakeProcessorBurst.ClearBlendShapeInfoLoop(
                ref totalDeltaVertices,
                ref totalDeltaNormals,
                ref totalDeltaTangents
            );

            for (int i = 0; i < blendShapeCount; i++)
            {
                float weight = smr.GetBlendShapeWeight(i);

                if (weight <= 0) continue;

                // FIXME: Multi frame blendshape support

                Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld_BlendShape_CacheLookup");
                var blendShape = blendShapeCache.GetOrCalculate((i, meshData.BaseMesh.GetBlendShapeFrameCount(i) - 1), () =>
                {
                    Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld_BlendShape_Calculate");
                    var container = new BlendShapeContainer(vertexCount);

                    var tmpDeltaVertices = new Vector3[vertexCount];
                    var tmpDeltaNormals = new Vector3[vertexCount];
                    var tmpDeltaTangents = new Vector3[vertexCount];

                    var frameIndex = meshData.BaseMesh.GetBlendShapeFrameCount(i) - 1;

                    meshData.BaseMesh.GetBlendShapeFrameVertices(i, frameIndex, tmpDeltaVertices, tmpDeltaNormals, tmpDeltaTangents);

                    container.deltaVertices.Reinterpret<Vector3>().CopyFrom(tmpDeltaVertices);
                    container.deltaNormals.Reinterpret<Vector3>().CopyFrom(tmpDeltaNormals);
                    container.deltaTangents.Reinterpret<Vector3>().CopyFrom(tmpDeltaTangents);

                    Profiler.EndSample();

                    return container;
                });
                Profiler.EndSample();

                float weightNormalized = weight / 100f;

                MeshBakeProcessorBurst.BlendShapeAddLoop(
                    ref totalDeltaVertices,
                    ref totalDeltaNormals,
                    ref totalDeltaTangents,
                    ref blendShape.deltaVertices,
                    ref blendShape.deltaNormals,
                    ref blendShape.deltaTangents,
                    weightNormalized
                );
            }
            Profiler.EndSample();
        }

        bool IsBlendShapesChanged(MedlayWritableMeshData meshData, SkinnedMeshRenderer smr)
        {
            var changed = false;

            var blendShapeCount = meshData.BaseMesh.blendShapeCount;

            if (blendShapeWeights == null)
            {
                blendShapeWeights = new float[blendShapeCount];

                changed = true;
            }

            if (blendShapeWeights.Length != blendShapeCount)
            {
                blendShapeWeights = new float[blendShapeCount];

                changed = true;
            }

            for (int i = 0; i < blendShapeCount; i++)
            {
                var weight = smr.GetBlendShapeWeight(i);

                if (blendShapeWeights[i] != weight)
                {
                    blendShapeWeights[i] = weight;
                    changed = true;
                }
            }

            return changed;
        }

        static void ReallocateIfNeeded<T>(ref NativeArray<T> array, int requiredLength) where T : struct
        {
            if (array.Length != requiredLength || !array.IsCreated)
            {
                if (array.IsCreated) array.Dispose();
                array = new NativeArray<T>(requiredLength, Allocator.Persistent);
            }
        }
    }
}
