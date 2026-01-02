using System;
using System.Collections.Generic;
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
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();

        List<Matrix4x4> bindPoses = new List<Matrix4x4>();
        List<BoneWeight> boneWeights = new List<BoneWeight>();
        List<Matrix4x4> boneMatrices = new List<Matrix4x4>();

        Vector3[] totalDeltaVertices = new Vector3[0];
        Vector3[] totalDeltaNormals = new Vector3[0];
        Vector3[] totalDeltaTangents = new Vector3[0];

        Vector3[] deltaVertices = new Vector3[0];
        Vector3[] deltaNormals = new Vector3[0];
        Vector3[] deltaTangents = new Vector3[0];

        public void ResetArrays(SkinnedMeshRenderer smr, Mesh mesh)
        {
            Profiler.BeginSample("MeshBakeProcessor.ResetArrays");

            vertexCount = mesh.vertexCount;

            if (vertices.Count != vertexCount)
            {
                Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateVertices");
                vertices = new List<Vector3>(new Vector3[vertexCount]);
                Profiler.EndSample();
            }

            mesh.GetVertices(vertices);

            if (mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Normal) && normals.Count != vertexCount)
            {
                Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateNormals");
                normals = new List<Vector3>(new Vector3[vertexCount]);
                Profiler.EndSample();
            }

            mesh.GetNormals(normals);

            if (mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Tangent) && tangents.Count != vertexCount)
            {
                Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateTangents");
                tangents = new List<Vector4>(new Vector4[vertexCount]);
                Profiler.EndSample();
            }

            mesh.GetTangents(tangents);

            if (bindPoses.Count != mesh.bindposeCount)
            {
                if (mesh.bindposeCount == 0)
                {

                }

                bindPoses = new List<Matrix4x4>(new Matrix4x4[mesh.bindposeCount]);
            }

            mesh.GetBindposes(bindPoses);

            if (boneWeights.Count != vertexCount)
            {
                Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateBoneWeights");
                boneWeights = new List<BoneWeight>(new BoneWeight[vertexCount]);
                Profiler.EndSample();
            }

            mesh.GetBoneWeights(boneWeights);

            if (smr.bones.Length != bindPoses.Count)
            {
                Profiler.EndSample();
                // TODO: Handle
                throw new Exception("Bone count mismatch between SkinnedMeshRenderer and Mesh bindposes.");
            }

            if (smr.bones.Length != boneMatrices.Count)
            {
                Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateBoneMatrices");
                boneMatrices = new List<Matrix4x4>(new Matrix4x4[smr.bones.Length]);
                Profiler.EndSample();
            }

            Profiler.BeginSample("MeshBakeProcessor.ResetArrays_AllocateDeltaArrays");
            if (totalDeltaVertices.Length != vertexCount)
            {
                totalDeltaVertices = new Vector3[vertexCount];
            }
            if (totalDeltaNormals.Length != vertexCount)
            {
                totalDeltaNormals = new Vector3[vertexCount];
            }
            if (totalDeltaTangents.Length != vertexCount)
            {
                totalDeltaTangents = new Vector3[vertexCount];
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
                    if (normals.Count > 0) totalDeltaNormals[v] += deltaNormals[v] * weightNormalized;
                    if (tangents.Count > 0) totalDeltaTangents[v] += deltaTangents[v] * weightNormalized;
                }
            }

            Profiler.EndSample();

            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.BakeMeshToWorld_VertexProcessing");

            for (int i = 0; i < vertexCount; i++)
            {
                var weight = boneWeights[i];
                Matrix4x4 skinMatrix = Matrix4x4.zero;
                if (weight.weight0 > 0) skinMatrix = skinMatrix.Add(boneMatrices[weight.boneIndex0].ScalarMultiply(weight.weight0));
                if (weight.weight1 > 0) skinMatrix = skinMatrix.Add(boneMatrices[weight.boneIndex1].ScalarMultiply(weight.weight1));
                if (weight.weight2 > 0) skinMatrix = skinMatrix.Add(boneMatrices[weight.boneIndex2].ScalarMultiply(weight.weight2));
                if (weight.weight3 > 0) skinMatrix = skinMatrix.Add(boneMatrices[weight.boneIndex3].ScalarMultiply(weight.weight3));

                vertices[i] = skinMatrix.MultiplyPoint3x4(vertices[i] + totalDeltaVertices[i]);

                if (normals.Count > 0)
                {
                    normals[i] = skinMatrix.MultiplyVector(normals[i] + totalDeltaNormals[i]).normalized;
                }

                if (tangents.Count > 0)
                {
                    Vector4 t = tangents[i];
                    Vector3 skinnedTangent = skinMatrix.MultiplyVector(new Vector3(t.x, t.y, t.z) + totalDeltaTangents[i]).normalized;
                    tangents[i] = new Vector4(skinnedTangent.x, skinnedTangent.y, skinnedTangent.z, t.w);
                }
            }

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
                    if (normals.Count > 0) totalDeltaNormals[v] += deltaNormals[v] * weightNormalized;
                    if (tangents.Count > 0) totalDeltaTangents[v] += deltaTangents[v] * weightNormalized;
                }
            }

            Profiler.EndSample();

            Profiler.EndSample();

            Profiler.BeginSample("MeshBakeProcessor.UnbakeMesh_VertexProcessing");

            for (int i = 0; i < vertexCount; i++)
            {
                var weight = boneWeights[i];
                Matrix4x4 skinMatrixSum = Matrix4x4.zero;
                if (weight.weight0 > 0) skinMatrixSum = skinMatrixSum.Add(boneMatrices[weight.boneIndex0].ScalarMultiply(weight.weight0));
                if (weight.weight1 > 0) skinMatrixSum = skinMatrixSum.Add(boneMatrices[weight.boneIndex1].ScalarMultiply(weight.weight1));
                if (weight.weight2 > 0) skinMatrixSum = skinMatrixSum.Add(boneMatrices[weight.boneIndex2].ScalarMultiply(weight.weight2));
                if (weight.weight3 > 0) skinMatrixSum = skinMatrixSum.Add(boneMatrices[weight.boneIndex3].ScalarMultiply(weight.weight3));

                Matrix4x4 invSkinMatrix = skinMatrixSum.inverse;

                Vector3 unskinnedPos = invSkinMatrix.MultiplyPoint3x4(vertices[i]);

                vertices[i] = unskinnedPos - totalDeltaVertices[i];

                if (normals.Count > 0)
                {
                    Vector3 unskinnedNormal = invSkinMatrix.MultiplyVector(normals[i]);
                    normals[i] = (unskinnedNormal - totalDeltaNormals[i]).normalized;
                }

                if (tangents.Count > 0)
                {
                    Vector4 t = tangents[i];
                    Vector3 unskinnedTangent = invSkinMatrix.MultiplyVector(new Vector3(t.x, t.y, t.z));
                    Vector3 finalTangent = (unskinnedTangent - totalDeltaTangents[i]).normalized;
                    tangents[i] = new Vector4(finalTangent.x, finalTangent.y, finalTangent.z, t.w);
                }
            }

            Profiler.EndSample();

            unbakedMesh.SetVertices(vertices);
            unbakedMesh.SetNormals(normals);
            unbakedMesh.SetTangents(tangents);

            Profiler.EndSample();
        }
    }
}
