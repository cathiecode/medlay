using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace com.superneko.medlay.Core.Internal.Unsafe
{
    public static class MeshDataUtils
    {
        public static NativeArray<float2> Reinterpret(NativeArray<Vector2> source) => Reinterpret<Vector2, float2>(source);
        public static NativeArray<float3> Reinterpret(NativeArray<Vector3> source) => Reinterpret<Vector3, float3>(source);
        public static NativeArray<float4> Reinterpret(NativeArray<Vector4> source) => Reinterpret<Vector4, float4>(source);
        public static NativeArray<Vector2> Reinterpret(NativeArray<float2> source) => Reinterpret<float2, Vector2>(source);
        public static NativeArray<Vector3> Reinterpret(NativeArray<float3> source) => Reinterpret<float3, Vector3>(source);
        public static NativeArray<Vector4> Reinterpret(NativeArray<float4> source) => Reinterpret<float4, Vector4>(source);
        public static NativeArray<Matrix4x4> Reinterpret(NativeArray<float4x4> source) => Reinterpret<float4x4, Matrix4x4>(source);
        public static NativeArray<float4x4> Reinterpret(NativeArray<Matrix4x4> source) => Reinterpret<Matrix4x4, float4x4>(source);

        static NativeArray<U> Reinterpret<T, U>(NativeArray<T> source) where U: struct where T: struct
        {
            return source.Reinterpret<U>(UnsafeUtility.SizeOf<T>());
        }
    }
}
