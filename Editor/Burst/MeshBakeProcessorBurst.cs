using Unity.Burst;
using Unity.Collections;
using UnityEngine;

namespace com.superneko.medlay.Editor.Burst
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
        public static void MultiplyPoint3x4(ref Matrix4x4 matrix, ref Vector3 point, out Vector3 result)
        {
            float x = matrix.m00 * point.x + matrix.m01 * point.y + matrix.m02 * point.z + matrix.m03;
            float y = matrix.m10 * point.x + matrix.m11 * point.y + matrix.m12 * point.z + matrix.m13;
            float z = matrix.m20 * point.x + matrix.m21 * point.y + matrix.m22 * point.z + matrix.m23;
            result = new Vector3(x, y, z);
        }

        [BurstCompile]
        public static void MultiplyVector(ref Matrix4x4 matrix, ref Vector3 vector, out Vector3 result)
        {
            float x = matrix.m00 * vector.x + matrix.m01 * vector.y + matrix.m02 * vector.z;
            float y = matrix.m10 * vector.x + matrix.m11 * vector.y + matrix.m12 * vector.z;
            float z = matrix.m20 * vector.x + matrix.m21 * vector.y + matrix.m22 * vector.z;
            result = new Vector3(x, y, z);
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
            ref NativeArray<Matrix4x4> boneMatrices,
            ref NativeArray<Vector3> vertices,
            ref NativeArray<Vector3> normals,
            ref NativeArray<Vector4> tangents,
            ref NativeArray<Vector3> totalDeltaVertices,
            ref NativeArray<Vector3> totalDeltaNormals,
            ref NativeArray<Vector3> totalDeltaTangents
        )
        {
            for (int i = 0; i < vertexCount; i++)
            {

                var weight = boneWeights[i];

                Matrix4x4 skinMatrix = Matrix4x4.zero;
                if (weight.weight0 > 0)
                {
                    var boneMatrix0 = boneMatrices[weight.boneIndex0];
                    M44.ScalarMultiply(ref boneMatrix0, weight.weight0, out var scaledMatrix0);
                    M44.AssignAdd(ref skinMatrix, ref scaledMatrix0);
                }

                if (weight.weight1 > 0)
                {
                    var boneMatrix1 = boneMatrices[weight.boneIndex1];
                    M44.ScalarMultiply(ref boneMatrix1, weight.weight1, out var scaledMatrix1);
                    M44.AssignAdd(ref skinMatrix, ref scaledMatrix1);
                }

                if (weight.weight2 > 0)
                {
                    var boneMatrix2 = boneMatrices[weight.boneIndex2];
                    M44.ScalarMultiply(ref boneMatrix2, weight.weight2, out var scaledMatrix2);
                    M44.AssignAdd(ref skinMatrix, ref scaledMatrix2);
                }

                if (weight.weight3 > 0)
                {
                    var boneMatrix3 = boneMatrices[weight.boneIndex3];
                    M44.ScalarMultiply(ref boneMatrix3, weight.weight3, out var scaledMatrix3);
                    M44.AssignAdd(ref skinMatrix, ref scaledMatrix3);
                }

                vertices[i] = skinMatrix.MultiplyPoint3x4(vertices[i] + totalDeltaVertices[i]);

                if (normals.Length > 0)
                {
                    normals[i] = skinMatrix.MultiplyVector(normals[i] + totalDeltaNormals[i]).normalized;
                }

                if (tangents.Length > 0)
                {
                    Vector4 t = tangents[i];
                    var tangentWithDelta = skinMatrix.MultiplyVector(new Vector3(t.x, t.y, t.z) + totalDeltaTangents[i]);
                    tangents[i] = new Vector4(tangentWithDelta.x, tangentWithDelta.y, tangentWithDelta.z, t.w);
                }
            }
        }

        [BurstCompile]
        public static void UnbakeMeshToLocal_VertexProcessing(
            int vertexCount,
            ref NativeArray<BoneWeight> boneWeights,
            ref NativeArray<Matrix4x4> boneMatrices,
            ref NativeArray<Vector3> vertices,
            ref NativeArray<Vector3> normals,
            ref NativeArray<Vector4> tangents,
            ref NativeArray<Vector3> totalDeltaVertices,
            ref NativeArray<Vector3> totalDeltaNormals,
            ref NativeArray<Vector3> totalDeltaTangents
        )
        {
            for (int i = 0; i < vertexCount; i++)
            {
                var weight = boneWeights[i];

                Matrix4x4 skinMatrixSum = Matrix4x4.zero;

                if (weight.weight0 > 0) {
                    var boneMatrix0 = boneMatrices[weight.boneIndex0];
                    M44.ScalarMultiply(ref boneMatrix0, weight.weight0, out var scaledMatrix0);
                    M44.AssignAdd(ref skinMatrixSum, ref scaledMatrix0);
                }

                if (weight.weight1 > 0) {
                    var boneMatrix1 = boneMatrices[weight.boneIndex1];
                    M44.ScalarMultiply(ref boneMatrix1, weight.weight1, out var scaledMatrix1);
                    M44.AssignAdd(ref skinMatrixSum, ref scaledMatrix1);
                }

                if (weight.weight2 > 0) {
                    var boneMatrix2 = boneMatrices[weight.boneIndex2];
                    M44.ScalarMultiply(ref boneMatrix2, weight.weight2, out var scaledMatrix2);
                    M44.AssignAdd(ref skinMatrixSum, ref scaledMatrix2);
                }

                if (weight.weight3 > 0) {
                    var boneMatrix3 = boneMatrices[weight.boneIndex3];
                    M44.ScalarMultiply(ref boneMatrix3, weight.weight3, out var scaledMatrix3);
                    M44.AssignAdd(ref skinMatrixSum, ref scaledMatrix3);
                }

                Matrix4x4 invSkinMatrix = Matrix4x4.identity;
                if (!Matrix4x4.Inverse3DAffine(skinMatrixSum, ref invSkinMatrix))
                {
                    invSkinMatrix = skinMatrixSum.inverse;
                }


                Vector3 unskinnedPos = invSkinMatrix.MultiplyPoint3x4(vertices[i]);

                vertices[i] = unskinnedPos - totalDeltaVertices[i];

                if (normals.Length > 0)
                {
                    Vector3 unskinnedNormal = invSkinMatrix.MultiplyVector(normals[i]);
                    normals[i] = (unskinnedNormal - totalDeltaNormals[i]).normalized;
                }

                if (tangents.Length > 0)
                {
                    Vector4 t = tangents[i];
                    Vector3 unskinnedTangent = invSkinMatrix.MultiplyVector(new Vector3(t.x, t.y, t.z));
                    Vector3 finalTangent = (unskinnedTangent - totalDeltaTangents[i]).normalized;
                    tangents[i] = new Vector4(finalTangent.x, finalTangent.y, finalTangent.z, t.w);
                }
            }

        }
    }
}
