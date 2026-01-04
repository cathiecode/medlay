using com.superneko.medlay.Core;
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
            var smr = TestUtils.LoadAvatarSMR("6404d1b6bcec6c44d8dd03187a2c4d49");

            var bakedMesh = Object.Instantiate(smr.sharedMesh);

            using var writableMeshData = MedlayWritableMeshData.Create(smr.sharedMesh, Unity.Collections.Allocator.Temp);

            Assert.DoesNotThrow(() =>
            {
                new MeshBakeProcessor().BakeMeshToWorld(writableMeshData, smr);
            });
        }

        [Test]
        public void BakeMeshToWorld_ProducesBakedVertices()
        {
            var smr = TestUtils.LoadAvatarSMR("6404d1b6bcec6c44d8dd03187a2c4d49");
            var originalMesh = smr.sharedMesh;
            var bakedMesh = Object.Instantiate(originalMesh);

            using var writableMeshData = MedlayWritableMeshData.Create(bakedMesh, Unity.Collections.Allocator.Temp);
            new MeshBakeProcessor().BakeMeshToWorld(writableMeshData, smr);
            MedlayWritableMeshData.Writeback(writableMeshData, bakedMesh);
            
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
            var smr = TestUtils.LoadAvatarSMR("6404d1b6bcec6c44d8dd03187a2c4d49");

            var bakedMesh = Object.Instantiate(smr.sharedMesh);

            using var writableMeshData = MedlayWritableMeshData.Create(bakedMesh, Unity.Collections.Allocator.Temp);

            new MeshBakeProcessor().BakeMeshToWorld(writableMeshData, smr);

            Assert.DoesNotThrow(() =>
            {
                new MeshBakeProcessor().UnBakeMeshFromWorld(writableMeshData, smr);
            });
        }

        public void UnbakeMesh_ProducesOriginalVertices(string originalGuid)
        {
            var smr = TestUtils.LoadAvatarSMR(originalGuid);

            var originalMesh = smr.sharedMesh;

            var modifiedMesh = Object.Instantiate(originalMesh);
            using var modifiedMeshData = MedlayWritableMeshData.Create(modifiedMesh, Unity.Collections.Allocator.Temp);
            new MeshBakeProcessor().BakeMeshToWorld(modifiedMeshData, smr);
            new MeshBakeProcessor().UnBakeMeshFromWorld(modifiedMeshData, smr);
            MedlayWritableMeshData.Writeback(modifiedMeshData, modifiedMesh);

            Assert.AreEqual(originalMesh.vertexCount, modifiedMesh.vertexCount);

            TestUtils.AssertMeshesAreSame(originalMesh, modifiedMesh);
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
            var smr = TestUtils.LoadAvatarSMR("6404d1b6bcec6c44d8dd03187a2c4d49");
            var modifiedMesh = Object.Instantiate(smr.sharedMesh);

            var processor = new MeshBakeProcessor();

            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    using var modifiedMeshData = MedlayWritableMeshData.Create(modifiedMesh, Unity.Collections.Allocator.Temp);
                    processor.BakeMeshToWorld(modifiedMeshData, smr);
                    processor.UnBakeMeshFromWorld(modifiedMeshData, smr);
                    MedlayWritableMeshData.Writeback(modifiedMeshData, modifiedMesh);
                }
            });
        }
    }
}
