using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;

namespace com.superneko.medlay.Core.Internal.Burst
{
    [BurstCompile]
    public static class MeshBakeProcessorBurst
    {
        [BurstCompile]
        public static void BakeMeshToWorld_VertexProcessing(
            int vertexCount,
            ref NativeArray<BoneWeight1> allBoneWeights,
            ref NativeArray<byte> bonesPerVertex,
            ref NativeArray<float4x4> boneMatrices,
            ref NativeArray<float3> vertices,
            ref NativeArray<float3> normals,
            ref NativeArray<float4> tangents,
            ref NativeArray<float3> totalDeltaVertices,
            ref NativeArray<float3> totalDeltaNormals,
            ref NativeArray<float3> totalDeltaTangents
        )
        {
            int boneWeightIndex = 0;
            for (int i = 0; i < vertexCount; i++)
            {
                float4x4 skinMatrix = Unity.Mathematics.float4x4.zero;

                if (bonesPerVertex.Length == 0)
                {
                    skinMatrix = boneMatrices[0];
                }
                else
                {
                    for (int j = 0; j < bonesPerVertex[i]; j++)
                    {
                        var bw = allBoneWeights[boneWeightIndex++];
                        var boneMatrix = bw.boneIndex;
                        skinMatrix += boneMatrices[boneMatrix] * bw.weight;
                    }
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
            ref NativeArray<BoneWeight1> allBoneWeights,
            ref NativeArray<byte> bonesPerVertex,
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

            int boneWeightIndex = 0;
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
                    if (bonesPerVertex.Length > 0) {
                        boneWeightIndex += bonesPerVertex[i];
                    }

                    continue;
                }

                float4x4 skinMatrixSum = Unity.Mathematics.float4x4.zero;

                if (bonesPerVertex.Length == 0)
                {
                    skinMatrixSum = boneMatrices[0];
                }
                else
                {
                    for (int j = 0; j < bonesPerVertex[i]; j++)
                    {
                        var bw = allBoneWeights[boneWeightIndex++];
                        var boneMatrix = bw.boneIndex;
                        skinMatrixSum += boneMatrices[boneMatrix] * bw.weight;
                    }
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

