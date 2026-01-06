using com.superneko.medlay.Core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Profiling;

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
                TestUtils.CreateInstanceWithPrivateConstructor<MeshBakeProcessor>().BakeMeshToBase(writableMeshData, smr);
            });
        }

        [Test]
        public void BakeMeshToWorld_ProducesBakedVertices()
        {
            var smr = TestUtils.LoadAvatarSMR("6404d1b6bcec6c44d8dd03187a2c4d49");
            var originalMesh = smr.sharedMesh;
            var bakedMesh = Object.Instantiate(originalMesh);

            using var writableMeshData = MedlayWritableMeshData.Create(bakedMesh, Unity.Collections.Allocator.Temp);
            TestUtils.CreateInstanceWithPrivateConstructor<MeshBakeProcessor>().BakeMeshToBase(writableMeshData, smr);
            MedlayWritableMeshData.WritebackAndDispose(writableMeshData, bakedMesh);
            
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

            TestUtils.CreateInstanceWithPrivateConstructor<MeshBakeProcessor>().BakeMeshToBase(writableMeshData, smr);

            Assert.DoesNotThrow(() =>
            {
                TestUtils.CreateInstanceWithPrivateConstructor<MeshBakeProcessor>().UnBakeMeshFromBase(writableMeshData, smr);
            });
        }

        public void UnbakeMesh_ProducesOriginalVertices(string originalGuid)
        {
            var smr = TestUtils.LoadAvatarSMR(originalGuid);

            var originalMesh = smr.sharedMesh;

            var modifiedMesh = Object.Instantiate(originalMesh);
            using var modifiedMeshData = MedlayWritableMeshData.Create(modifiedMesh, Unity.Collections.Allocator.Temp);
            TestUtils.CreateInstanceWithPrivateConstructor<MeshBakeProcessor>().BakeMeshToBase(modifiedMeshData, smr);
            TestUtils.CreateInstanceWithPrivateConstructor<MeshBakeProcessor>().UnBakeMeshFromBase(modifiedMeshData, smr);
            MedlayWritableMeshData.WritebackAndDispose(modifiedMeshData, modifiedMesh);

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
        public void UnbakeMesh_ProducesOriginalVertices_StaticMeshAssignedToSMR()
        {
            UnbakeMesh_ProducesOriginalVertices("10a61e39ad8edab4bbc379ca484bd361");
        }

        [Test]
        public void UnbakeMesh_ProducesOriginalVertices_TransformZeroed()
        {
            UnbakeMesh_ProducesOriginalVertices("a135a5d900e527d4dafd3059aa5a4de1");
        }

        [Test]
        public void UnbakeMesh_ProducesOriginalVertices_TransformRemoved()
        {
            UnbakeMesh_ProducesOriginalVertices("1f49f43f88e9f8a4b8c3dfb255a74f1b");
        }

        [Test]
        public void UnbakeMesh_ProducesOriginalVertices_SkinnedMeshAssignedToMeshRenderer()
        {
            var go = TestUtils.LoadAssetByGUID<GameObject>("e194e85a0547ae242874392a3eff7e96");

            var meshRenderer = go.GetComponentInChildren<MeshRenderer>();

            var originalMesh = meshRenderer.GetComponent<MeshFilter>().sharedMesh;

            var modifiedMesh = Object.Instantiate(originalMesh);
            using var modifiedMeshData = MedlayWritableMeshData.Create(modifiedMesh, Unity.Collections.Allocator.Temp);
            TestUtils.CreateInstanceWithPrivateConstructor<MeshBakeProcessor>().BakeMeshToBase(modifiedMeshData, meshRenderer);
            TestUtils.CreateInstanceWithPrivateConstructor<MeshBakeProcessor>().UnBakeMeshFromBase(modifiedMeshData, meshRenderer);
            MedlayWritableMeshData.WritebackAndDispose(modifiedMeshData, modifiedMesh);

            Assert.AreEqual(originalMesh.vertexCount, modifiedMesh.vertexCount);

            TestUtils.AssertMeshesAreSame(originalMesh, modifiedMesh);

        }

        [Test]
        public void MeshBakeProcessor_SequencialProcess()
        {
            var smr = TestUtils.LoadAvatarSMR("6404d1b6bcec6c44d8dd03187a2c4d49");
            var modifiedMesh = Object.Instantiate(smr.sharedMesh);

            var processor = TestUtils.CreateInstanceWithPrivateConstructor<MeshBakeProcessor>();

            Assert.DoesNotThrow(() =>
            {
                for (int i = 0; i < 1000; i++)
                {
                    Profiler.BeginSample("MeshBakeProcessor_SequencialProcess_Iteration");
                    using var modifiedMeshData = MedlayWritableMeshData.Create(modifiedMesh, Unity.Collections.Allocator.Temp);
                    processor.BakeMeshToBase(modifiedMeshData, smr);
                    processor.UnBakeMeshFromBase(modifiedMeshData, smr);
                    MedlayWritableMeshData.WritebackAndDispose(modifiedMeshData, modifiedMesh);
                    Profiler.EndSample();
                }
            });
        }

        [Test]
        public void MeshBakeProcessor_SameProcessorBakeAndUnbake_ProducesOriginalVertices()
        {
            var smr = TestUtils.LoadAvatarSMR("6404d1b6bcec6c44d8dd03187a2c4d49");

            var originalMesh = smr.sharedMesh;

            var modifiedMesh = Object.Instantiate(originalMesh);
            using var modifiedMeshData = MedlayWritableMeshData.Create(modifiedMesh, Unity.Collections.Allocator.Temp);

            var processor = TestUtils.CreateInstanceWithPrivateConstructor<MeshBakeProcessor>();

            processor.BakeMeshToBase(modifiedMeshData, smr);
            processor.UnBakeMeshFromBase(modifiedMeshData, smr);
            MedlayWritableMeshData.WritebackAndDispose(modifiedMeshData, modifiedMesh);

            Assert.AreEqual(originalMesh.vertexCount, modifiedMesh.vertexCount);

            TestUtils.AssertMeshesAreSame(originalMesh, modifiedMesh, allowExactMatch: true);
        }
    }
}
