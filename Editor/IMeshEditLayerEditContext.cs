using UnityEngine;

namespace com.superneko.medlay.Editor
{
    public interface IMeshEditLayerEditContext
    {
        Matrix4x4 WorldToBaseMatrix { get; }
        Matrix4x4 BaseToWorldMatrix => WorldToBaseMatrix.inverse;
        void TryGetContextData<T>(out T data);
    }
}
