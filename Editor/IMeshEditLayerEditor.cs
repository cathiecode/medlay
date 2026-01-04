using com.superneko.medlay.Core;

namespace com.superneko.medlay.Editor
{
    public interface IMeshEditLayerEditor
    {
        internal void SetTarget(MeshEditLayer target);
        void OnSceneGUI(UnityEngine.Matrix4x4 baseTransform);
        void OnDrawGizmos(UnityEngine.Matrix4x4 baseTransform);
        float InspectorHeight { get; }
        void OnInspectorGUI(UnityEngine.Rect rect, UnityEditor.SerializedProperty serializedProperty);
    }
}
