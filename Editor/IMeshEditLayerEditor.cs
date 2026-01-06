using com.superneko.medlay.Core;
using UnityEngine;

namespace com.superneko.medlay.Editor
{
    public interface IMeshEditLayerEditor
    {
        internal void SetTarget(MeshEditLayer target);
        internal void SetObjectTarget(Object targetObject);
        void OnSceneGUI(IMeshEditLayerEditContext context);
        void OnDrawGizmos(IMeshEditLayerEditContext context);
        float InspectorHeight { get; }
        void OnInspectorGUI(UnityEngine.Rect rect, UnityEditor.SerializedProperty serializedProperty);
    }
}
