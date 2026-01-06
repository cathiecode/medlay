using System.Collections.Generic;
using com.superneko.medlay.Core;
using UnityEngine;

namespace com.superneko.medlay.Editor
{
    public sealed class MeshEditLayerEditorRegistry
    {
        Dictionary<System.Type, System.Func<IMeshEditLayerEditor>> editorFactories = new Dictionary<System.Type, System.Func<IMeshEditLayerEditor>>();

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

        public void RegisterMeshEditLayerEditor<T>(System.Func<IMeshEditLayerEditor> editorFactory)
        {
            var meshEditLayerType = typeof(T);
            if (!editorFactories.ContainsKey(meshEditLayerType))
            {
                editorFactories.Add(meshEditLayerType, editorFactory);
            }
        }

        public IMeshEditLayerEditor CreateFor(MeshEditLayer meshEditLayer, Object targetObject)
        {
            var meshEditLayerType = meshEditLayer.GetType();

            if (!editorFactories.TryGetValue(meshEditLayerType, out var editorFactory))
            {
                throw new System.Exception($"Mesh Edit Layer Editor for {meshEditLayerType} is not registered."); // TODO: Fallback editor
            }

            var editor = editorFactory();

            editor.SetTarget(meshEditLayer);
            editor.SetTargetObject(targetObject);

            return editor;
        }
    }
}
