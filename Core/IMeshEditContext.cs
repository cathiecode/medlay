using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace com.superneko.medlay.Core
{
    public interface IMeshEditContext
    {
        Renderer OriginalRenderer { get; }
        MedlayWritableMeshData WritableMeshData { get; }
        Mesh Mesh { get; }
        Matrix4x4 WorldToBaseMatrix { get; }
        Transform[] Bones { get; set; }
        internal bool BoneIsChanged { get; }

        void TryGetProcessContextData<T>(out T data) where T : class;
    }
}
