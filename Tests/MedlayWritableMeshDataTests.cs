using com.superneko.medlay.Core;
using UnityEngine;
using Unity.Mathematics;
using NUnit.Framework;
using Unity.Collections;

namespace com.superneko.medlay.Tests
{
    class MedlayWritableMeshDataTests
    {
        [Test]
        public void CreateMedlayWritableMeshData_FromMesh_CreatesWritableMeshData()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
            };
            mesh.triangles = new int[] { 0, 1, 2 };

            var medlayWritableMeshData = MedlayWritableMeshData.Create(mesh, Allocator.Temp);

            var vertices = medlayWritableMeshData.GetVertices();

            Assert.AreEqual(3, vertices.Length);
            Assert.AreEqual(new float3(0, 0, 0), vertices[0]);
            Assert.AreEqual(new float3(1, 0, 0), vertices[1]);
            Assert.AreEqual(new float3(0, 1, 0), vertices[2]);

            medlayWritableMeshData.Dispose();
        }

        [Test]
        public void GetVertices_ModifyVertices_WritesBackToMeshData()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
            };
            mesh.triangles = new int[] { 0, 1, 2 };

            var medlayWritableMeshData = MedlayWritableMeshData.Create(mesh, Allocator.Temp);

            var vertices = medlayWritableMeshData.GetVertices();

            // Modify vertices
            vertices[0] = new float3(0, 0, 1);
            vertices[1] = new float3(1, 0, 1);
            vertices[2] = new float3(0, 1, 1);

            // Write back changes
            MedlayWritableMeshData.WritebackAndDispose(medlayWritableMeshData, mesh);

            // Verify changes in MeshData
            var updatedVertices = mesh.vertices;

            Assert.AreEqual(new Vector3(0, 0, 1), updatedVertices[0]);
            Assert.AreEqual(new Vector3(1, 0, 1), updatedVertices[1]);
            Assert.AreEqual(new Vector3(0, 1, 1), updatedVertices[2]);
        }

        [Test]
        public void Writeback_ComplexSituation()
        {
            var smr = TestUtils.LoadAvatarSMR("6404d1b6bcec6c44d8dd03187a2c4d49");

            var originalVertices = smr.sharedMesh.vertices;

            var medlayWritableMeshData = MedlayWritableMeshData.Create(smr.sharedMesh, Allocator.Temp);

            var vertices = medlayWritableMeshData.GetVertices();

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += new Vector3(1.1f, 2.2f, 3.3f);
            }

            var updatedMesh = Object.Instantiate(smr.sharedMesh);

            MedlayWritableMeshData.WritebackAndDispose(medlayWritableMeshData, updatedMesh);

            var updatedVertices = updatedMesh.vertices;

            var totalError = 0f;

            for (int i = 0; i < updatedVertices.Length; i++)
            {
                var error = (originalVertices[i] + new Vector3(1.1f, 2.2f, 3.3f) - updatedVertices[i]).magnitude;
                Assert.Less(error, 0.0001f, $"Vertex {i} did not update correctly.");
                totalError += error;
            }

            Debug.Log($"Average vertex error: {totalError / updatedVertices.Length}");
        }
    }
}