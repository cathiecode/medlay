using com.superneko.medlay.Core;
using UnityEngine;

namespace com.superneko.medlay.Editor
{
    public interface IMeshEditLayerDefinition
    {
        MeshEditLayer CreateInstance();
        IMeshEditLayerEditor CreateEditorInstance();

        Texture2D Icon { get => null; }
        string Name { get; }
        string Description { get => null; }
    }
}
