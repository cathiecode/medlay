using com.superneko.medlay.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace com.superneko.medlay.Tests
{
    class MeshUnbakeProcessorTests
    {
        [Test]
        public void BakeMeshToWorld_DoesNotThrow()
        {
            var shapell = TestObjectLoader.LoadTestObject<GameObject>("6404d1b6bcec6c44d8dd03187a2c4d49");

            var smr = shapell.GetComponentInChildren<SkinnedMeshRenderer>();

            var bakedMesh = Object.Instantiate(smr.sharedMesh);

            Assert.DoesNotThrow(() =>
            {
                new MeshBakeProcessor().BakeMeshToWorld(smr.sharedMesh, smr, bakedMesh);
            });
        }

        [Test]
        public void BakeMeshToWorld_ProducesBakedVertices()
        {
            var shapell = TestObjectLoader.LoadTestObject<GameObject>("6404d1b6bcec6c44d8dd03187a2c4d49");
            var smr = shapell.GetComponentInChildren<SkinnedMeshRenderer>();
            var originalMesh = smr.sharedMesh;
            var bakedMesh = Object.Instantiate(originalMesh);
            new MeshBakeProcessor().BakeMeshToWorld(originalMesh, smr, bakedMesh);
            Assert.AreEqual(originalMesh.vertexCount, bakedMesh.vertexCount);
            var originalVertices = originalMesh.vertices;
            var bakedVertices = bakedMesh.vertices;
            for (int i = 0; i < originalMesh.vertexCount; i++)
            {
                Assert.Greater((originalVertices[i] - bakedVertices[i]).magnitude, 0.0001f, $"Vertex {i} matches unexpectedly.");
            }
        }

        [Test]
        public void UnbakeMesh_DoesNotThrow()
        {
            var shapell = TestObjectLoader.LoadTestObject<GameObject>("6404d1b6bcec6c44d8dd03187a2c4d49");

            var smr = shapell.GetComponentInChildren<SkinnedMeshRenderer>();

            var bakedMesh = Object.Instantiate(smr.sharedMesh);

            new MeshBakeProcessor().BakeMeshToWorld(smr.sharedMesh, smr, bakedMesh);

            var unbakedMesh = Object.Instantiate(smr.sharedMesh);

            Assert.DoesNotThrow(() =>
            {
                new MeshBakeProcessor().UnBakeMeshFromWorld(bakedMesh, smr, unbakedMesh);
            });
        }

        public void UnbakeMesh_ProducesOriginalVertices(string originalGuid)
        {
            var shapell = TestObjectLoader.LoadTestObject<GameObject>(originalGuid);

            var smr = shapell.GetComponentInChildren<SkinnedMeshRenderer>();
            var originalMesh = smr.sharedMesh;

            var bakedMesh = Object.Instantiate(originalMesh);
            new MeshBakeProcessor().BakeMeshToWorld(originalMesh, smr, bakedMesh);

            var unbakedMesh = Object.Instantiate(originalMesh);
            new MeshBakeProcessor().UnBakeMeshFromWorld(bakedMesh, smr, unbakedMesh);

            Assert.AreEqual(originalMesh.vertexCount, unbakedMesh.vertexCount);

            var originalVertices = originalMesh.vertices;
            var unbakedVertices = unbakedMesh.vertices;

            var originalNormals = originalMesh.normals;
            var unbakedNormals = unbakedMesh.normals;

            var originalTangents = originalMesh.tangents;
            var unbakedTangents = unbakedMesh.tangents;

            var totalVerticesError = 0f;
            var totalNormalsError = 0f;
            var totalTangentsError = 0f;

            var maxVertexError = 0f;
            var maxNormalError = 0f;
            var maxTangentError = 0f;

            for (int i = 0; i < originalMesh.vertexCount; i++)
            {
                var vertexError = (originalVertices[i] - unbakedVertices[i]).magnitude;
                var normalError = (originalNormals[i] - unbakedNormals[i]).magnitude;
                var tangentError = (originalTangents[i] - unbakedTangents[i]).magnitude;

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
        }

        [Test]
        public void UnbakeMesh_ProducesOriginalVertices_PoseOnly()
        {
            UnbakeMesh_ProducesOriginalVertices("6404d1b6bcec6c44d8dd03187a2c4d49");
        }

        [Test]
        public void UnbakeMesh_ProducesOriginalVertices_PoseAndBlendShape()
        {
            UnbakeMesh_ProducesOriginalVertices("55eb1dc98a84ca547b4c78dd8ab9ff3a");
        }

        [Test]
        public void MeshBakeProcessor_SequencialProcess()
        {
            var shapell = TestObjectLoader.LoadTestObject<GameObject>("6404d1b6bcec6c44d8dd03187a2c4d49");

            var smr = shapell.GetComponentInChildren<SkinnedMeshRenderer>();

            var bakedMesh = Object.Instantiate(smr.sharedMesh);
            var unbakedMesh = Object.Instantiate(smr.sharedMesh);

            var processor = new MeshBakeProcessor();

            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    processor.BakeMeshToWorld(smr.sharedMesh, smr, bakedMesh);
                    processor.UnBakeMeshFromWorld(bakedMesh, smr, unbakedMesh);
                }
            });
        }
    }
}
