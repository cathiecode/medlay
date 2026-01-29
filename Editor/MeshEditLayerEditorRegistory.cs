using System.Collections.Generic;
using com.superneko.medlay.Core;
using UnityEngine;

namespace com.superneko.medlay.Editor
{
    public sealed class MeshEditLayerEditorRegistry
    {
        Dictionary<System.Type, IMeshEditLayerDefinition> layerDefinitions = new Dictionary<System.Type, IMeshEditLayerDefinition>();

        static MeshEditLayerEditorRegistry instance;

        public static MeshEditLayerEditorRegistry Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MeshEditLayerEditorRegistry();
                }

                return instance;
            }
        }

        public MeshEditLayerEditorRegistry() { }

        public void RegisterMeshEditLayerEditor<T>(IMeshEditLayerDefinition layerDefinition)
        {
            var meshEditLayerType = typeof(T);
            if (!layerDefinitions.ContainsKey(meshEditLayerType))
            {
                layerDefinitions.Add(meshEditLayerType, layerDefinition);
            }
        }

        public IMeshEditLayerEditor CreateFor(MeshEditLayer meshEditLayer, Object targetObject)
        {
            var meshEditLayerType = meshEditLayer.GetType();

            if (!layerDefinitions.TryGetValue(meshEditLayerType, out var editorFactory))
            {
                throw new System.Exception($"Mesh Edit Layer Editor for {meshEditLayerType} is not registered."); // TODO: Fallback editor
            }

            var editor = editorFactory.CreateEditorInstance();

            editor.SetTarget(meshEditLayer);
            editor.SetTargetObject(targetObject);

            return editor;
        }

        public IEnumerable<IMeshEditLayerDefinition> RegisteredMeshEditLayerDefinitions => layerDefinitions.Values;
    }
}
