using UnityEngine;

namespace com.superneko.medlay.Editor
{
    public abstract class MeshEditLayerEditor<T> : IMeshEditLayerEditor where T : Core.MeshEditLayer
    {
        protected T target;
        protected Object targetObject;

        void IMeshEditLayerEditor.SetTarget(Core.MeshEditLayer target)
        {
            this.target = (T)target;
        }

        void IMeshEditLayerEditor.SetTargetObject(Object targetObject)
        {
            this.targetObject = targetObject;
        }

        public abstract void OnSceneGUI(IMeshEditLayerEditContext context);
        public abstract void OnDrawGizmos(IMeshEditLayerEditContext context);
        public abstract float InspectorHeight { get; }
        public abstract void OnInspectorGUI(IMeshEditLayerEditContext context, Rect rect, UnityEditor.SerializedProperty serializedProperty);
    }
}
