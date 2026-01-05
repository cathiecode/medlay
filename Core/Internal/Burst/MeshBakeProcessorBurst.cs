using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;

namespace com.superneko.medlay.Core.Internal.Burst
{
    // [BurstCompile]
    public static class MeshBakeProcessorBurst
    {
        [BurstCompile]
        public static void BakeMeshToWorld_VertexProcessing(
            int vertexCount,
            ref NativeArray<BoneWeight> boneWeights,
            ref NativeArray<float4x4> boneMatrices,
            ref NativeArray<float3> vertices,
            ref NativeArray<float3> normals,
            ref NativeArray<float4> tangents,
            ref NativeArray<float3> totalDeltaVertices,
            ref NativeArray<float3> totalDeltaNormals,
            ref NativeArray<float3> totalDeltaTangents
        )
        {
            for (int i = 0; i < vertexCount; i++)
            {

                var weight = boneWeights[i];

                float4x4 skinMatrix = Unity.Mathematics.float4x4.zero;
                if (weight.weight0 > 0)
                {
                    var boneMatrix0 = boneMatrices[weight.boneIndex0];
                    skinMatrix += boneMatrices[weight.boneIndex0] * weight.weight0;
                }
                else
                {
                    // Processing a static mesh assigned to a SkinnedMeshRenderer
                    skinMatrix = boneMatrices[0];
                }

                if (weight.weight1 > 0)
                {
                    var boneMatrix1 = boneMatrices[weight.boneIndex1];
                    skinMatrix += boneMatrices[weight.boneIndex1] * weight.weight1;
                }

                if (weight.weight2 > 0)
                {
                    var boneMatrix2 = boneMatrices[weight.boneIndex2];
                    skinMatrix += boneMatrices[weight.boneIndex2] * weight.weight2;
                }

                if (weight.weight3 > 0)
                {
                    var boneMatrix3 = boneMatrices[weight.boneIndex3];
                    skinMatrix += boneMatrices[weight.boneIndex3] * weight.weight3;
                }

                vertices[i] = math.transform(skinMatrix, vertices[i] + totalDeltaVertices[i]);

                if (normals.Length > 0)
                {
                    normals[i] = math.normalize(math.rotate(skinMatrix, normals[i] + totalDeltaNormals[i]));
                }

                if (tangents.Length > 0)
                {
                    float4 t = tangents[i];
                    var tangentWithDelta = math.rotate(skinMatrix, new float3(t.x, t.y, t.z) + totalDeltaTangents[i]);
                    tangents[i] = new float4(tangentWithDelta.x, tangentWithDelta.y, tangentWithDelta.z, t.w);
                }
            }
        }

        [BurstCompile]
        public static void UnbakeMeshToLocal_VertexProcessing(
            int vertexCount,
            ref NativeArray<BoneWeight> boneWeights,
            ref NativeArray<float4x4> boneMatrices,
            ref NativeArray<float3> vertices,
            ref NativeArray<float3> normals,
            ref NativeArray<float4> tangents,
            ref NativeArray<float3> totalDeltaVertices,
            ref NativeArray<float3> totalDeltaNormals,
            ref NativeArray<float3> totalDeltaTangents,
            ref NativeArray<float3> referenceBakedVertices,
            ref NativeArray<float3> referenceBakedNormals,
            ref NativeArray<float4> referenceBakedTangents,
            ref NativeArray<float3> referenceUnbakedVertices,
            ref NativeArray<float3> referenceUnbakedNormals,
            ref NativeArray<float4> referenceUnbakedTangents
        )
        {
            var hasNormal = normals.Length > 0;
            var hasTangent = tangents.Length > 0;

            for (int i = 0; i < vertexCount; i++)
            {
                bool verticeChanged = !all(referenceBakedVertices[i] == vertices[i]);
                bool normalChanged = hasNormal && !all(referenceBakedNormals[i] == normals[i]);
                bool tangentChanged = hasTangent && !all(referenceBakedTangents[i] == tangents[i]);

                if (!verticeChanged)
                {
                    vertices[i] = referenceUnbakedVertices[i];
                }

                if (!normalChanged && hasNormal)
                {
                    normals[i] = referenceUnbakedNormals[i];
                }

                if (!tangentChanged && hasTangent)
                {
                    tangents[i] = referenceUnbakedTangents[i];
                }

                if (!verticeChanged && !normalChanged && !tangentChanged)
                {
                    continue;
                }

                var weight = boneWeights[i];

                float4x4 skinMatrixSum = Unity.Mathematics.float4x4.zero;

                if (weight.weight0 > 0)
                {
                    var boneMatrix0 = boneMatrices[weight.boneIndex0];
                    skinMatrixSum += boneMatrix0 * weight.weight0;
                }
                else
                {
                    // Processing a static mesh assigned to a SkinnedMeshRenderer
                    skinMatrixSum = boneMatrices[0];
                }

                if (weight.weight1 > 0)
                {
                    var boneMatrix1 = boneMatrices[weight.boneIndex1];
                    skinMatrixSum += boneMatrix1 * weight.weight1;
                }

                if (weight.weight2 > 0)
                {
                    var boneMatrix2 = boneMatrices[weight.boneIndex2];
                    skinMatrixSum += boneMatrix2 * weight.weight2;
                }

                if (weight.weight3 > 0)
                {
                    var boneMatrix3 = boneMatrices[weight.boneIndex3];
                    skinMatrixSum += boneMatrix3 * weight.weight3;
                }

                float4x4 invSkinMatrix = math.inverse(skinMatrixSum);

                if (verticeChanged) vertices[i] = transform(invSkinMatrix, vertices[i]) - totalDeltaVertices[i];

                if (hasNormal && normalChanged) normals[i] = normalize(rotate(invSkinMatrix, normals[i]) - totalDeltaNormals[i]);

                if (hasTangent && tangentChanged)
                {
                    var t = tangents[i];
                    float3 finalTangent = normalize(rotate(invSkinMatrix, t.xyz) - totalDeltaTangents[i]);
                    tangents[i] = new float4(finalTangent.x, finalTangent.y, finalTangent.z, t.w);
                }
            }
        }

        [BurstCompile]
        public static void ClearBlendShapeInfoLoop(
            [WriteOnly] ref NativeArray<float3> deltaVertices,
            [WriteOnly] ref NativeArray<float3> deltaNormals,
            [WriteOnly] ref NativeArray<float3> deltaTangents
        )
        {
            for (int v = 0; v < deltaVertices.Length; v++)
            {
                deltaVertices[v] = Unity.Mathematics.float3.zero;
                deltaNormals[v] = Unity.Mathematics.float3.zero;
                deltaTangents[v] = Unity.Mathematics.float3.zero;
            }
        }

        [BurstCompile]
        public static void BlendShapeAddLoop(
            [WriteOnly] ref NativeArray<float3> vertices,
            [WriteOnly] ref NativeArray<float3> normals,
            [WriteOnly] ref NativeArray<float3> tangents,
            [ReadOnly] ref NativeArray<float3> deltaVertices,
            [ReadOnly] ref NativeArray<float3> deltaNormals,
            [ReadOnly] ref NativeArray<float3> deltaTangents,
            float weightNormalized
        )
        {
            for (int v = 0; v < vertices.Length; v++)
            {
                vertices[v] += deltaVertices[v] * weightNormalized;
                normals[v] += deltaNormals[v] * weightNormalized;
                tangents[v] += deltaTangents[v] * weightNormalized;
            }
        }
    }
}

