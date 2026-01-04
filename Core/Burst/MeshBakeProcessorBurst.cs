using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using static Unity.Mathematics.math;
using UnityEngine;

namespace com.superneko.medlay.Core.Burst
{
    [BurstCompile]
    static class M44
    {
        [BurstCompile]
        public static void ScalarMultiply(ref Matrix4x4 matrix, float scalar, out Matrix4x4 result)
        {
            result = new Matrix4x4();

            result.m00 = matrix.m00 * scalar;
            result.m01 = matrix.m01 * scalar;
            result.m02 = matrix.m02 * scalar;
            result.m03 = matrix.m03 * scalar;
            result.m10 = matrix.m10 * scalar;
            result.m11 = matrix.m11 * scalar;
            result.m12 = matrix.m12 * scalar;
            result.m13 = matrix.m13 * scalar;
            result.m20 = matrix.m20 * scalar;
            result.m21 = matrix.m21 * scalar;
            result.m22 = matrix.m22 * scalar;
            result.m23 = matrix.m23 * scalar;
            result.m30 = matrix.m30 * scalar;
            result.m31 = matrix.m31 * scalar;
            result.m32 = matrix.m32 * scalar;
            result.m33 = matrix.m33 * scalar;
        }

        [BurstCompile]
        public static void Add(ref Matrix4x4 a, ref Matrix4x4 b, out Matrix4x4 result)
        {
            result = new Matrix4x4();

            result.m00 = a.m00 + b.m00;
            result.m01 = a.m01 + b.m01;
            result.m02 = a.m02 + b.m02;
            result.m03 = a.m03 + b.m03;
            result.m10 = a.m10 + b.m10;
            result.m11 = a.m11 + b.m11;
            result.m12 = a.m12 + b.m12;
            result.m13 = a.m13 + b.m13;
            result.m20 = a.m20 + b.m20;
            result.m21 = a.m21 + b.m21;
            result.m22 = a.m22 + b.m22;
            result.m23 = a.m23 + b.m23;
            result.m30 = a.m30 + b.m30;
            result.m31 = a.m31 + b.m31;
            result.m32 = a.m32 + b.m32;
            result.m33 = a.m33 + b.m33;
        }

        [BurstCompile]
        public static void MultiplyPoint3x4(ref Matrix4x4 matrix, ref float3 point, out float3 result)
        {
            float x = matrix.m00 * point.x + matrix.m01 * point.y + matrix.m02 * point.z + matrix.m03;
            float y = matrix.m10 * point.x + matrix.m11 * point.y + matrix.m12 * point.z + matrix.m13;
            float z = matrix.m20 * point.x + matrix.m21 * point.y + matrix.m22 * point.z + matrix.m23;
            result = new float3(x, y, z);
        }

        [BurstCompile]
        public static void MultiplyVector(ref Matrix4x4 matrix, ref float3 vector, out float3 result)
        {
            float x = matrix.m00 * vector.x + matrix.m01 * vector.y + matrix.m02 * vector.z;
            float y = matrix.m10 * vector.x + matrix.m11 * vector.y + matrix.m12 * vector.z;
            float z = matrix.m20 * vector.x + matrix.m21 * vector.y + matrix.m22 * vector.z;
            result = new float3(x, y, z);
        }

        [BurstCompile]
        public static void AssignAdd(ref Matrix4x4 a, ref Matrix4x4 b)
        {
            a.m00 += b.m00;
            a.m01 += b.m01;
            a.m02 += b.m02;
            a.m03 += b.m03;
            a.m10 += b.m10;
            a.m11 += b.m11;
            a.m12 += b.m12;
            a.m13 += b.m13;
            a.m20 += b.m20;
            a.m21 += b.m21;
            a.m22 += b.m22;
            a.m23 += b.m23;
            a.m30 += b.m30;
            a.m31 += b.m31;
            a.m32 += b.m32;
            a.m33 += b.m33;
        }

        [BurstCompile]
        public static void Inverse(ref Matrix4x4 matrix, out Matrix4x4 result)
        {
            result = matrix.inverse;
        }

        [BurstCompile]
        public static void Multiply(ref Matrix4x4 a, ref Matrix4x4 b, out Matrix4x4 result)
        {
            result = a * b;
        }
    }

    [BurstCompile]
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
                } else
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
            ref NativeArray<float3> totalDeltaTangents
        )
        {
            for (int i = 0; i < vertexCount; i++)
            {
                var weight = boneWeights[i];

                float4x4 skinMatrixSum = Unity.Mathematics.float4x4.zero;

                if (weight.weight0 > 0) {
                    var boneMatrix0 = boneMatrices[weight.boneIndex0];
                    skinMatrixSum += boneMatrix0 * weight.weight0;
                } else
                {
                    // Processing a static mesh assigned to a SkinnedMeshRenderer
                    skinMatrixSum = boneMatrices[0];
                }

                if (weight.weight1 > 0) {
                    var boneMatrix1 = boneMatrices[weight.boneIndex1];
                    skinMatrixSum += boneMatrix1 * weight.weight1;
                }

                if (weight.weight2 > 0) {
                    var boneMatrix2 = boneMatrices[weight.boneIndex2];
                    skinMatrixSum += boneMatrix2 * weight.weight2;
                }

                if (weight.weight3 > 0) {
                    var boneMatrix3 = boneMatrices[weight.boneIndex3];
                    skinMatrixSum += boneMatrix3 * weight.weight3;
                }

                float4x4 invSkinMatrix = math.inverse(skinMatrixSum);

                float3 unskinnedPos = math.transform(invSkinMatrix, vertices[i]);

                vertices[i] = unskinnedPos - totalDeltaVertices[i];

                if (normals.Length > 0)
                {
                    float3 unskinnedNormal = math.rotate(invSkinMatrix, normals[i]);
                    normals[i] = normalize(unskinnedNormal - totalDeltaNormals[i]);
                }

                if (tangents.Length > 0)
                {
                    float4 t = tangents[i];
                    float3 unskinnedTangent = math.rotate(invSkinMatrix, new float3(t.x, t.y, t.z));
                    float3 finalTangent = normalize(unskinnedTangent - totalDeltaTangents[i]);
                    tangents[i] = new float4(finalTangent.x, finalTangent.y, finalTangent.z, t.w);
                }
            }
        }
    }
}

