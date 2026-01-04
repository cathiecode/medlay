using UnityEngine;

namespace com.superneko.medlay.Core
{
    public interface IMeshEditContext
    {
        Renderer OriginalRenderer { get; }
        MedlayWritableMeshData WritableMeshData { get; set; } // TODO: internal set
    }
}
