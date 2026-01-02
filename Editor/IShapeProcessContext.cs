using UnityEngine;

namespace com.superneko.medlay.Editor
{
    public interface IShapeProcessContext
    {
        Mesh MeshInWorld { get; set; }
    }
}
