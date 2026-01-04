using System;
using System.Collections.Generic;
using com.superneko.medlay.Core;

namespace com.superneko.medlay.Editor
{
    public sealed class MeshEditLayerEditorRegistry
    {
        Dictionary<System.Type, Func<IMeshEditLayerEditor>> editorFactories = new Dictionary<System.Type, Func<IMeshEditLayerEditor>>();

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

        public void RegisterMeshEditLayerEditor<T>(Func<IMeshEditLayerEditor> editor)
        {
            var meshEditLayerType = typeof(T);
            if (!editorFactories.ContainsKey(meshEditLayerType))
            {
                editorFactories.Add(meshEditLayerType, editor);
            }
        }

        public IMeshEditLayerEditor CreateMeshEditLayerEditor(System.Type meshEditLayerType)
        {
            if (!editorFactories.TryGetValue(meshEditLayerType, out var editorFactory))
            {
                throw new System.Exception($"Mesh Edit Layer Editor for {meshEditLayerType} is not registered."); // TODO: Fallback editor
            }

            var editor = editorFactory();

            editor.SetTarget((MeshEditLayer)Activator.CreateInstance(meshEditLayerType));

            return editor;
        }
    }
}
