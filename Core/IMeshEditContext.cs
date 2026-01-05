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
    }
}
