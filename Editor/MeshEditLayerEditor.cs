namespace com.superneko.medlay.Editor
{
    public abstract class MeshEditLayerEditor<T> : IMeshEditLayerEditor where T : Core.MeshEditLayer
    {
        protected T target;

        void IMeshEditLayerEditor.SetTarget(Core.MeshEditLayer target)
        {
            this.target = (T)target;
        }

        public abstract void OnSceneGUI(UnityEngine.Matrix4x4 baseTransform);
        public abstract void OnDrawGizmos(UnityEngine.Matrix4x4 baseTransform);
        public abstract float InspectorHeight { get; }
        public abstract void OnInspectorGUI(UnityEngine.Rect rect, UnityEditor.SerializedProperty serializedProperty);
    }
}
