using UnityEditor;
using UnityEngine;

namespace com.superneko.medlay.Editor
{
    public abstract class MeshEditLayerEditor<T> : IMeshEditLayerEditor where T : Core.MeshEditLayer
    {
        protected T target;
        protected Object targetObject;
        private float cachedHeight = -1f;

        void IMeshEditLayerEditor.SetTarget(Core.MeshEditLayer target)
        {
            this.target = (T)target;
        }

        void IMeshEditLayerEditor.SetTargetObject(Object targetObject)
        {
            this.targetObject = targetObject;
        }

        public virtual void OnSceneGUI(IMeshEditLayerEditContext context) {}
        public virtual void OnDrawGizmos(IMeshEditLayerEditContext context) {}
        public virtual float InspectorHeight => cachedHeight;
        public virtual void OnInspectorGUI(IMeshEditLayerEditContext context, Rect rect, UnityEditor.SerializedProperty serializedProperty)
        {
            var y = rect.y;
            cachedHeight = 0;
            foreach (var property in serializedProperty)
            {
                var sp = property as SerializedProperty;
                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, EditorGUI.GetPropertyHeight(sp)), sp);
                y += EditorGUI.GetPropertyHeight(sp);
                cachedHeight += EditorGUI.GetPropertyHeight(sp);
            }
        }
    }
}
