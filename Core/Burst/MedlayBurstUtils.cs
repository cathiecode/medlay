using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace com.superneko.medlay.Core.Burst
{   
    [BurstCompile]
    public struct SkinningInfo
    {
        public NativeArray<float4x4> bindposes;
        public NativeArray<BoneWeight1> allBoneWeights;
        public NativeArray<int> boneWeightStartIndexPerVertex;
        public NativeArray<byte> boneWeightCountsPerVertex;
        public NativeArray<float4x4> boneMatrices;
    }

    [BurstCompile]
    public static class MedlayBurstUtils
    {
        [BurstCompile]
        public static void GetVertexWorldPosition(
            ref int vertexIndex,
            ref float3 vertexPosition,
            ref float3 vertexNormal,
            ref float4 vertexTangent,
            ref SkinningInfo skinningInfo,
            out float3 skinnedPosition,
            out float3 skinnedNormal,
            out float4 skinnedTangent
        )
        {
            skinnedPosition = float3.zero;
            skinnedNormal = float3.zero;
            var skinnedTangentTmp = float3.zero;

            int startIndex = skinningInfo.boneWeightStartIndexPerVertex[vertexIndex];
            int endIndex = skinningInfo.boneWeightStartIndexPerVertex[vertexIndex] + skinningInfo.boneWeightCountsPerVertex[vertexIndex];

            for (int i = startIndex; i < endIndex; i++)
            {
                BoneWeight1 bw = skinningInfo.allBoneWeights[i];
                float weight = bw.weight;
                int boneIndex = bw.boneIndex;

                float4x4 boneMatrix = skinningInfo.boneMatrices[boneIndex];
                float4x4 bindpose = skinningInfo.bindposes[boneIndex];
                float4x4 finalMatrix = math.mul(boneMatrix, bindpose);

                skinnedPosition += math.transform(finalMatrix, vertexPosition) * weight;
                skinnedNormal += math.normalize(math.mul((float3x3)finalMatrix, vertexNormal)) * weight;
                skinnedTangentTmp += math.normalize(math.mul((float3x3)finalMatrix, vertexTangent.xyz)) * weight;
            }

            skinnedTangent = new float4(skinnedTangentTmp, vertexTangent.w);
        }

        /*[BurstCompile]
        public static void GetSkinnedVertexPositionSimple(
            ref int vertexIndex,
            ref float3 vertexPosition,
            ref SkinningInfo skinningInfo,
            out float3 skinnedPosition
        )
        {
            skinnedPosition = float3.zero;

            int startIndex = skinningInfo.boneWeightStartIndexPerVertex[vertexIndex];
            int endIndex = skinningInfo.boneWeightStartIndexPerVertex[vertexIndex + 1];

            for (int i = startIndex; i < endIndex; i++)
            {
                BoneWeight1 bw = skinningInfo.allBoneWeights[i];
                float weight = bw.weight;
                int boneIndex = bw.boneIndex;

                float4x4 boneMatrix = skinningInfo.boneMatrices[boneIndex];
                float4x4 bindpose = skinningInfo.bindposes[boneIndex];
                float4x4 finalMatrix = math.mul(boneMatrix, bindpose);

                skinnedPosition += math.transform(finalMatrix, vertexPosition) * weight;
            }
        }*/

        [BurstCompile]
        public static void GetVertexLocalPosition(
            ref int vertexIndex,
            ref float3 vertexPosition,
            ref float3 vertexNormal,
            ref float4 vertexTangent,
            ref SkinningInfo skinningInfo,
            out float3 skinnedPosition,
            out float3 skinnedNormal,
            out float4 skinnedTangent
        )
        {
            int startIndex = skinningInfo.boneWeightStartIndexPerVertex[vertexIndex];
            int endIndex = skinningInfo.boneWeightStartIndexPerVertex[vertexIndex] + skinningInfo.boneWeightCountsPerVertex[vertexIndex];

            float4x4 skinMatrixSum = float4x4.zero;

            for (int i = startIndex; i < endIndex; i++)
            {
                BoneWeight1 bw = skinningInfo.allBoneWeights[i];
                float weight = bw.weight;
                int boneIndex = bw.boneIndex;

                float4x4 boneMatrix = skinningInfo.boneMatrices[boneIndex];
                skinMatrixSum += boneMatrix * weight;
            }

            float4x4 invSkinMatrix = math.inverse(skinMatrixSum);

            skinnedPosition = math.transform(invSkinMatrix, vertexPosition);
            skinnedNormal = math.rotate(invSkinMatrix, vertexNormal);
            skinnedTangent = new float4(math.rotate(invSkinMatrix, vertexTangent.xyz), vertexTangent.w);
        }
    }
}
