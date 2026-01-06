using UnityEngine;

namespace com.superneko.medlay.Editor
{
    public class MeshEditLayerEditContext : IMeshEditLayerEditContext
    {
        public Matrix4x4 WorldToBaseMatrix { get; }

        public MeshEditLayerEditContext(Matrix4x4 worldToBaseMatrix)
        {
            WorldToBaseMatrix = worldToBaseMatrix;
        }
    }
}
