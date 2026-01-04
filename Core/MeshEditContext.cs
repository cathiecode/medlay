using Unity.Collections;
using UnityEngine;

namespace com.superneko.medlay.Core
{
    internal class MeshEditContext : IMeshEditContext
    {
        public MedlayWritableMeshData WritableMeshData { get; internal set; }

        public Renderer OriginalRenderer { get; internal set; }

        public static MeshEditContext FromRenderer(Renderer renderer)
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
                WritableMeshData = MedlayWritableMeshData.Create(mesh, Allocator.Temp),
                OriginalRenderer = renderer
            };

            return context;
        }
    }
}
