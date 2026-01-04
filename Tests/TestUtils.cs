using NUnit.Framework;
using UnityEngine;

namespace com.superneko.medlay.Tests
{
    internal static class TestUtils
    {
        public static T CreateInstanceWithPrivateConstructor<T>() where T : class
        {
            var constructor = typeof(T).GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null,
                System.Type.EmptyTypes,
                null);

            if (constructor == null)
            {
                throw new System.InvalidOperationException($"Type {typeof(T).FullName} does not have a private parameterless constructor.");
            }

            return (T)constructor.Invoke(null);
        }

        public static SkinnedMeshRenderer LoadAvatarSMR(string guid)
        {
            var asset = LoadAssetByGUID<GameObject>(guid);
            var instance = Object.Instantiate(asset);
            var smr = instance.GetComponentInChildren<SkinnedMeshRenderer>();

            return smr;
        }

        public static T LoadAssetByGUID<T>(string guid) where T : Object
        {
            var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var obj = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
            return obj;
        }

        public static void AssertMeshesAreSame(Mesh originalMesh, Mesh modifiedMesh, bool allowExactMatch = false)
        {
            var originalVertices = originalMesh.vertices;
            var modifiedVertices = modifiedMesh.vertices;

            var originalNormals = originalMesh.normals;
            var modifiedNormals = modifiedMesh.normals;

            var originalTangents = originalMesh.tangents;
            var modifiedTangents = modifiedMesh.tangents;

            var totalVerticesError = 0f;
            var totalNormalsError = 0f;
            var totalTangentsError = 0f;

            var maxVertexError = 0f;
            var maxNormalError = 0f;
            var maxTangentError = 0f;

            for (int i = 0; i < originalMesh.vertexCount; i++)
            {
                Assert.AreEqual(originalMesh.vertexCount, modifiedMesh.vertexCount, "Vertex count does not match.");

                var vertexError = (originalVertices[i] - modifiedVertices[i]).magnitude;
                var normalError = (originalNormals[i] - modifiedNormals[i]).magnitude;
                var tangentError = (originalTangents[i] - modifiedTangents[i]).magnitude;

                Assert.Less(vertexError, 0.001f, $"Vertex {i} does not match.");
                Assert.Less(normalError, 0.001f, $"Normal {i} does not match.");
                Assert.Less(tangentError, 0.001f, $"Tangent {i} does not match.");

                totalVerticesError += vertexError;
                totalNormalsError += normalError;
                totalTangentsError += tangentError;

                if (vertexError > maxVertexError) maxVertexError = vertexError;
                if (normalError > maxNormalError) maxNormalError = normalError;
                if (tangentError > maxTangentError) maxTangentError = tangentError;
            }

            Debug.Log($"Vertex error avg:{totalVerticesError / originalMesh.vertexCount}, max:{maxVertexError}");
            Debug.Log($"Normal error avg:{totalNormalsError / originalMesh.vertexCount}, max:{maxNormalError}");
            Debug.Log($"Tangent error avg:{totalTangentsError / originalMesh.vertexCount}, max:{maxTangentError}");

            if (totalVerticesError.Equals(0f) && totalNormalsError.Equals(0f) && totalTangentsError.Equals(0f))
            {
                if (!allowExactMatch)
                {
                    Assert.Fail("Meshes are exactly the same. This might indicate that no modifications were applied.");
                } else
                {
                    Debug.LogWarning("Meshes are exactly the same.");
                }
            }
        }
    }
}
