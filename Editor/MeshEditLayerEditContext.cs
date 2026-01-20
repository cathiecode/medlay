using System.Collections.Generic;
using UnityEngine;

namespace com.superneko.medlay.Editor
{
    public class MeshEditLayerEditContext : IMeshEditLayerEditContext
    {
        public Matrix4x4 WorldToBaseMatrix { get; }
        
        Dictionary<System.Type, object> contextData = new Dictionary<System.Type, object>();

        public MeshEditLayerEditContext(Matrix4x4 worldToBaseMatrix, Dictionary<System.Type, object> contextData = null)
        {
            WorldToBaseMatrix = worldToBaseMatrix;

            if (contextData != null)
            {
                this.contextData = contextData;
            }
        }

        public void TryGetContextData<T>(out T data)
        {
            if (contextData.TryGetValue(typeof(T), out var obj))
            {
                data = (T)obj;
                return;
            }

            data = default;
        }
    }
}
