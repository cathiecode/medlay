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

            var hasNormals = originalNormals.Length > 0;

            var originalTangents = originalMesh.tangents;
            var modifiedTangents = modifiedMesh.tangents;

            var hasTangents = originalTangents.Length > 0;

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
                Assert.Less(vertexError, 0.001f, $"Vertex {i} does not match.");
                totalVerticesError += vertexError;
                if (vertexError > maxVertexError) maxVertexError = vertexError;

                if (hasNormals)
                {
                    var normalError = (originalNormals[i] - modifiedNormals[i]).magnitude;
                    Assert.Less(normalError, 0.001f, $"Normal {i} does not match.");
                    totalNormalsError += normalError;
                    if (normalError > maxNormalError) maxNormalError = normalError;
                }

                if (hasTangents)
                {
                    var tangentError = (originalTangents[i] - modifiedTangents[i]).magnitude;
                    Assert.Less(tangentError, 0.001f, $"Tangent {i} does not match.");
                    totalTangentsError += tangentError;
                    if (tangentError > maxTangentError) maxTangentError = tangentError;
                }
            }

            Debug.Log($"Vertex error avg:{totalVerticesError / originalMesh.vertexCount}, max:{maxVertexError}");
            Debug.Log($"Normal error avg:{totalNormalsError / originalMesh.vertexCount}, max:{maxNormalError}");
            Debug.Log($"Tangent error avg:{totalTangentsError / originalMesh.vertexCount}, max:{maxTangentError}");

            if (totalVerticesError.Equals(0f) && totalNormalsError.Equals(0f) && totalTangentsError.Equals(0f))
            {
                if (!allowExactMatch)
                {
                    Assert.Fail("Meshes are exactly the same. This might indicate that no modifications were applied.");
                }
                else
                {
                    Debug.LogWarning("Meshes are exactly the same.");
                }
            }
        }

        public static void AssertMeshDoesNotHaveNaN(Mesh mesh)
        {
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var tangents = mesh.tangents;

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                var vertexIsNan = float.IsNaN(vertices[i].magnitude);
                var normalIsNan = normals.Length == 0 ? false : float.IsNaN(normals[i].magnitude);
                var tangentIsNan = tangents.Length == 0 ? false : float.IsNaN(tangents[i].magnitude);

                if (vertexIsNan || normalIsNan || tangentIsNan)
                {
                    Assert.Fail("Mesh includes NaN");
                }
            }
        }

        public static Mesh CreateTriangleMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
            };
            mesh.triangles = new int[] { 0, 1, 2 };

            return mesh;
        }
    }
}
