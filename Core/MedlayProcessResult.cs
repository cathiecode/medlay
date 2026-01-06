using System;
using UnityEngine;

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

        public void AddAsBlendShapeFrame(Mesh mesh, string shapeName, float weight)
        {
            if (disposed)
            {
                throw new System.ObjectDisposedException("MedlayProcessResult");
            }

            disposed = true;

            var deformedMesh = context.Mesh;
            var vertices = deformedMesh.vertices;
            var normals = deformedMesh.normals;
            var tmpTangents = deformedMesh.tangents;
            var tangents = new Vector3[tmpTangents.Length];

            for (int i = 0; i < tmpTangents.Length; i++)
            {
                var t = tmpTangents[i];
                tangents[i] = new Vector3(t.x, t.y, t.z);
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

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
            }
        }
    }
}
