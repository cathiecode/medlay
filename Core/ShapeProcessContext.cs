using UnityEngine;

namespace com.superneko.medlay.Core
{
    internal class ShapeProcessContext : IShapeProcessContext
    {
        Mesh meshInWorld;

        Mesh IShapeProcessContext.MeshInWorld { get => meshInWorld; set => meshInWorld = value; }

        static ShapeProcessContext FromSkinnedMeshRenderer(SkinnedMeshRenderer skinnedMeshRenderer)
        {
            var baseMesh = new Mesh();
            skinnedMeshRenderer.BakeMesh(baseMesh);

            var context = new ShapeProcessContext()
            {
                meshInWorld = baseMesh
            };

            return context;
        }
    }
}
