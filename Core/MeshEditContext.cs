using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace com.superneko.medlay.Core
{
    internal class MeshEditContext : IMeshEditContext
    {
        MedlayWritableMeshData writableMeshData;

        public MedlayWritableMeshData WritableMeshData
        {
            get {
                if (writableMeshData == null)
                {
                    writableMeshData = MedlayWritableMeshData.Create(mesh, Allocator.Temp);
                }

                return writableMeshData;
            }
        }

        Mesh mesh;

        public Mesh Mesh
        {
            get
            {
                WritebackIfNeed();

                return mesh;
            }
        }

        public Renderer OriginalRenderer { get; internal set; }

        public void WritebackIfNeed()
        {
            if (writableMeshData != null)
            {
                MedlayWritableMeshData.WritebackAndDispose(writableMeshData, mesh);
                writableMeshData = null;
            }
        }

        public static MeshEditContext FromRenderer(Renderer renderer, Mesh writebackMesh)
        {
            Mesh mesh;

            switch (renderer)
            {
                case SkinnedMeshRenderer smr:
                    mesh = smr.sharedMesh;
                    break;
                case MeshRenderer mr:
                    var mf = mr.GetComponent<MeshFilter>();
                    mesh = mf.sharedMesh;
                    break;
                default:
                    throw new System.Exception("Unsupported renderer type: " + renderer.GetType().Name);
            }

            var context = new MeshEditContext()
            {
                writableMeshData = MedlayWritableMeshData.Create(mesh, Allocator.Temp),
                mesh = writebackMesh,
                OriginalRenderer = renderer
            };

            return context;
        }
    }
}
