using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace com.superneko.medlay.Core
{
    public struct MedlayProcessResult : IDisposable
    {
        bool disposed;

        MeshEditContext context;

        internal MedlayProcessResult(MeshEditContext context)
        {
            this.disposed = false;
            this.context = context;
        }

        public Mesh AsMesh()
        {
            if (disposed)
            {
                throw new System.ObjectDisposedException("MedlayProcessResult");
            }

            disposed = true;

            return context.Mesh;
        }

        public Transform[] GetBones()
        {
            return context.Bones;
        }

        public void AddAsBlendShapeFrame(Mesh mesh, string shapeName, float weight)
        {
            if (disposed)
            {
                throw new System.ObjectDisposedException("MedlayProcessResult");
            }

            disposed = true;

            var originalVertices = mesh.vertices;
            var originalNormals = mesh.normals;
            var originalTangents = mesh.tangents;

            originalNormals = originalNormals.Length == 0 ? null : originalNormals;
            originalTangents = originalTangents.Length == 0 ? null : originalTangents;

            var hasNormals = originalNormals != null;
            var hasTangents = originalTangents != null;

            var deformedMesh = context.Mesh;

            var vertices = deformedMesh.vertices;
            var normals = hasNormals ? deformedMesh.normals : null;
            var tmpTangents = hasTangents ? deformedMesh.tangents : null;
            var tangents = hasTangents ? new Vector3[tmpTangents.Length] : null;

            for (int i = 0; i < originalVertices.Length; i++)
            {
                vertices[i] -= originalVertices[i];
                if (hasNormals)
                {
                    normals[i] -= originalNormals[i];
                }
                if (hasTangents)
                {
                    var t = originalTangents[i];
                    var dt = tmpTangents[i];
                    tangents[i] = new Vector3(dt.x - t.x, dt.y - t.y, dt.z - t.z);
                }
            }

            mesh.AddBlendShapeFrame(shapeName, weight, vertices, normals, tangents);
        }

        public void ApplyToMesh(Mesh mesh)
        {
            if (disposed)
            {
                throw new System.ObjectDisposedException("MedlayProcessResult");
            }

            disposed = true;

            Mesh.ApplyAndDisposeWritableMeshData(context.WritableMeshData.meshDataArray, mesh);
        }

        public void ApplyToMeshAndPreserveBlendShape(Mesh mesh)
        {
            if (disposed)
            {
                throw new System.ObjectDisposedException("MedlayProcessResult");
            }

            disposed = true;

            mesh.SetVertices(context.WritableMeshData.GetVertices());

            if (context.WritableMeshData.HasVertexAttribute(VertexAttribute.Normal)) {
                mesh.SetNormals(context.WritableMeshData.GetNormals());
            }

            if (context.WritableMeshData.HasVertexAttribute(VertexAttribute.Tangent)) {
                mesh.SetTangents(context.WritableMeshData.GetTangents());
            }

            mesh.boneWeights = context.Mesh.boneWeights;
            mesh.bindposes = context.Mesh.bindposes;

            context.DisposeWritableMeshIfAvailable();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
            }

            context.DisposeWritableMeshIfAvailable();
        }
    }
}
