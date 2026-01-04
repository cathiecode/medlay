using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace com.superneko.medlay.Core.Unsafe
{
    [BurstCompile]
    public static class MeshDataAttributeWriter
    {
        // =========================
        // Public API (strict)
        // =========================

        public static void SetVertices(ref NativeArray<float3> inVertices, Mesh.MeshData meshData)
            => SetF3(meshData, VertexAttribute.Position, ref inVertices,
                expectedDim: 3,
                allowFloat32: true, allowFloat16: true,
                allowSNorm8: false, allowSNorm16: false,
                allowUNorm8: false, allowUNorm16: false);

        public static void SetUV0(ref NativeArray<float2> inUv0, Mesh.MeshData meshData)
            => SetF2(meshData, VertexAttribute.TexCoord0, ref inUv0,
                expectedDim: 2,
                allowFloat32: true, allowFloat16: true);

        public static void SetUV1(ref NativeArray<float2> inUv1, Mesh.MeshData meshData)
            => SetF2(meshData, VertexAttribute.TexCoord1, ref inUv1,
                expectedDim: 2,
                allowFloat32: true, allowFloat16: true);

        public static void SetUV2(ref NativeArray<float2> inUv2, Mesh.MeshData meshData)
            => SetF2(meshData, VertexAttribute.TexCoord2, ref inUv2,
                expectedDim: 2,
                allowFloat32: true, allowFloat16: true);

        public static void SetUV3(ref NativeArray<float2> inUv3, Mesh.MeshData meshData)
            => SetF2(meshData, VertexAttribute.TexCoord3, ref inUv3,
                expectedDim: 2,
                allowFloat32: true, allowFloat16: true);

        public static void SetUV4(ref NativeArray<float2> inUv4, Mesh.MeshData meshData)
            => SetF2(meshData, VertexAttribute.TexCoord4, ref inUv4,
                expectedDim: 2,
                allowFloat32: true, allowFloat16: true);

        public static void SetUV5(ref NativeArray<float2> inUv5, Mesh.MeshData meshData)
            => SetF2(meshData, VertexAttribute.TexCoord5, ref inUv5,
                expectedDim: 2,
                allowFloat32: true, allowFloat16: true);

        public static void SetUV6(ref NativeArray<float2> inUv6, Mesh.MeshData meshData)
            => SetF2(meshData, VertexAttribute.TexCoord6, ref inUv6,
                expectedDim: 2,
                allowFloat32: true, allowFloat16: true);

        public static void SetUV7(ref NativeArray<float2> inUv7, Mesh.MeshData meshData)
            => SetF2(meshData, VertexAttribute.TexCoord7, ref inUv7,
                expectedDim: 2,
                allowFloat32: true, allowFloat16: true);

        public static void SetNormals(ref NativeArray<float3> inNormals, Mesh.MeshData meshData)
            => SetF3(meshData, VertexAttribute.Normal, ref inNormals,
                expectedDim: 3,
                allowFloat32: true, allowFloat16: true,
                allowSNorm8: true, allowSNorm16: true,
                allowUNorm8: false, allowUNorm16: false);

        public static void SetTangents(ref NativeArray<float4> inTangents, Mesh.MeshData meshData)
            => SetF4(meshData, VertexAttribute.Tangent, ref inTangents,
                expectedDim: 4,
                allowFloat32: true, allowFloat16: true,
                allowSNorm8: true, allowSNorm16: true,
                allowUNorm8: false, allowUNorm16: false);

        public static void SetColors(ref NativeArray<float4> inColors, Mesh.MeshData meshData)
            => SetF4(meshData, VertexAttribute.Color, ref inColors,
                expectedDim: 4,
                allowFloat32: true, allowFloat16: true,
                allowSNorm8: false, allowSNorm16: false,
                allowUNorm8: true, allowUNorm16: true);

        public static void SetUVs(int channel, ref NativeArray<float2> inUvs, Mesh.MeshData meshData)
        {
            if ((uint)channel > 7u) { Throw($"[UV] channel must be 0..7, but was {channel}."); return; }
            var attr = GetUvAttribute(channel);

            SetF2(meshData, attr, ref inUvs,
                expectedDim: 2,
                allowFloat32: true, allowFloat16: true);
        }

        public static void SetUVs(int channel, ref NativeArray<float3> inUvs, Mesh.MeshData meshData)
        {
            if ((uint)channel > 7u) { Throw($"[UV] channel must be 0..7, but was {channel}."); return; }
            var attr = GetUvAttribute(channel);

            SetF3(meshData, attr, ref inUvs,
                expectedDim: 3,
                allowFloat32: true, allowFloat16: true,
                allowSNorm8: false, allowSNorm16: false,
                allowUNorm8: false, allowUNorm16: false);
        }

        public static void SetUVs(int channel, ref NativeArray<float4> inUvs, Mesh.MeshData meshData)
        {
            if ((uint)channel > 7u) { Throw($"[UV] channel must be 0..7, but was {channel}."); return; }
            var attr = GetUvAttribute(channel);

            SetF4(meshData, attr, ref inUvs,
                expectedDim: 4,
                allowFloat32: true, allowFloat16: true,
                allowSNorm8: false, allowSNorm16: false,
                allowUNorm8: false, allowUNorm16: false);
        }

        private static VertexAttribute GetUvAttribute(int channel)
        {
            // channel 0..7 -> TexCoord0..TexCoord7
            return (VertexAttribute)((int)VertexAttribute.TexCoord0 + channel);
        }

        // =========================
        // Core: resolve layout + dispatch
        // =========================

        private static void ResolveLayoutOrThrow(
            Mesh.MeshData meshData,
            VertexAttribute attr,
            int expectedDim,
            out int vertexCount,
            out VertexAttributeFormat fmt,
            out int stream,
            out int offset,
            out int stride,
            out NativeArray<byte> vb)
        {
            ValidateCommonOrThrow(meshData, attr);

            int dim = meshData.GetVertexAttributeDimension(attr);
            fmt = meshData.GetVertexAttributeFormat(attr);
            stream = meshData.GetVertexAttributeStream(attr);
            offset = meshData.GetVertexAttributeOffset(attr);
            stride = meshData.GetVertexBufferStride(stream);

            if (dim != expectedDim)
                Throw($"[{attr}] Dimension must be {expectedDim}, but was {dim}.");

            if (stream < 0) Throw($"[{attr}] Invalid stream.");
            if (offset < 0) Throw($"[{attr}] Invalid offset.");
            if (stride <= 0) Throw($"[{attr}] Invalid stride.");

            vb = meshData.GetVertexData<byte>(stream);

            int elementSize = GetFormatSizeOrThrow(fmt);
            int bytes = expectedDim * elementSize;

            if (offset + bytes > stride)
                Throw($"[{attr}] Attribute overruns stride (offset+size > stride).");

            vertexCount = meshData.vertexCount;
            long required = (long)offset + (long)(vertexCount - 1) * stride + bytes;
            if (required > vb.Length)
                Throw($"[{attr}] Vertex buffer too small for computed layout.");
        }

        private static void SetF2(
            Mesh.MeshData meshData,
            VertexAttribute attr,
            ref NativeArray<float2> src,
            int expectedDim,
            bool allowFloat32,
            bool allowFloat16)
        {
            ValidateLengthOrThrow(src.Length, meshData.vertexCount, attr);

            ResolveLayoutOrThrow(meshData, attr, expectedDim,
                out int vertexCount, out var fmt, out _, out int offset, out int stride, out var vb);

            switch (fmt)
            {
                case VertexAttributeFormat.Float32:
                    if (!allowFloat32) Throw($"[{attr}] Float32 not allowed.");
                    WriteFloat32_F2(ref src, ref vb, vertexCount, stride, offset);
                    return;

                case VertexAttributeFormat.Float16:
                    if (!allowFloat16) Throw($"[{attr}] Float16 not allowed.");
                    WriteFloat16_F2(ref src, ref vb, vertexCount, stride, offset);
                    return;

                default:
                    Throw($"[{attr}] Unsupported format: {fmt}");
                    return;
            }
        }

        private static void SetF3(
            Mesh.MeshData meshData,
            VertexAttribute attr,
            ref NativeArray<float3> src,
            int expectedDim,
            bool allowFloat32,
            bool allowFloat16,
            bool allowSNorm8,
            bool allowSNorm16,
            bool allowUNorm8,
            bool allowUNorm16)
        {
            ValidateLengthOrThrow(src.Length, meshData.vertexCount, attr);

            ResolveLayoutOrThrow(meshData, attr, expectedDim,
                out int vertexCount, out var fmt, out _, out int offset, out int stride, out var vb);

            switch (fmt)
            {
                case VertexAttributeFormat.Float32:
                    if (!allowFloat32) Throw($"[{attr}] Float32 not allowed.");
                    WriteFloat32_F3(ref src, ref vb, vertexCount, stride, offset);
                    return;

                case VertexAttributeFormat.Float16:
                    if (!allowFloat16) Throw($"[{attr}] Float16 not allowed.");
                    WriteFloat16_F3(ref src, ref vb, vertexCount, stride, offset);
                    return;

                case VertexAttributeFormat.SNorm8:
                    if (!allowSNorm8) Throw($"[{attr}] SNorm8 not allowed.");
                    WriteSNorm8_F3(ref src, ref vb, vertexCount, stride, offset);
                    return;

                case VertexAttributeFormat.SNorm16:
                    if (!allowSNorm16) Throw($"[{attr}] SNorm16 not allowed.");
                    WriteSNorm16_F3(ref src, ref vb, vertexCount, stride, offset);
                    return;

                case VertexAttributeFormat.UNorm8:
                    if (!allowUNorm8) Throw($"[{attr}] UNorm8 not allowed.");
                    WriteUNorm8_F3(ref src, ref vb, vertexCount, stride, offset);
                    return;

                case VertexAttributeFormat.UNorm16:
                    if (!allowUNorm16) Throw($"[{attr}] UNorm16 not allowed.");
                    WriteUNorm16_F3(ref src, ref vb, vertexCount, stride, offset);
                    return;

                default:
                    Throw($"[{attr}] Unsupported format: {fmt}");
                    return;
            }
        }

        private static void SetF4(
            Mesh.MeshData meshData,
            VertexAttribute attr,
            ref NativeArray<float4> src,
            int expectedDim,
            bool allowFloat32,
            bool allowFloat16,
            bool allowSNorm8,
            bool allowSNorm16,
            bool allowUNorm8,
            bool allowUNorm16)
        {
            ValidateLengthOrThrow(src.Length, meshData.vertexCount, attr);

            ResolveLayoutOrThrow(meshData, attr, expectedDim,
                out int vertexCount, out var fmt, out _, out int offset, out int stride, out var vb);

            switch (fmt)
            {
                case VertexAttributeFormat.Float32:
                    if (!allowFloat32) Throw($"[{attr}] Float32 not allowed.");
                    WriteFloat32_F4(ref src, ref vb, vertexCount, stride, offset);
                    return;

                case VertexAttributeFormat.Float16:
                    if (!allowFloat16) Throw($"[{attr}] Float16 not allowed.");
                    WriteFloat16_F4(ref src, ref vb, vertexCount, stride, offset);
                    return;

                case VertexAttributeFormat.SNorm8:
                    if (!allowSNorm8) Throw($"[{attr}] SNorm8 not allowed.");
                    WriteSNorm8_F4(ref src, ref vb, vertexCount, stride, offset);
                    return;

                case VertexAttributeFormat.SNorm16:
                    if (!allowSNorm16) Throw($"[{attr}] SNorm16 not allowed.");
                    WriteSNorm16_F4(ref src, ref vb, vertexCount, stride, offset);
                    return;

                case VertexAttributeFormat.UNorm8:
                    if (!allowUNorm8) Throw($"[{attr}] UNorm8 not allowed.");
                    WriteUNorm8_F4(ref src, ref vb, vertexCount, stride, offset);
                    return;

                case VertexAttributeFormat.UNorm16:
                    if (!allowUNorm16) Throw($"[{attr}] UNorm16 not allowed.");
                    WriteUNorm16_F4(ref src, ref vb, vertexCount, stride, offset);
                    return;

                default:
                    Throw($"[{attr}] Unsupported format: {fmt}");
                    return;
            }
        }

        // =========================
        // Burst kernels: Float32
        // =========================

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteFloat32_F2(ref NativeArray<float2> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float2* srcPtr = (float2*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float2 v = srcPtr[i];
                UnsafeUtility.MemCpy(p, &v, 8);
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteFloat32_F3(ref NativeArray<float3> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float3* srcPtr = (float3*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float3 v = srcPtr[i];
                UnsafeUtility.MemCpy(p, &v, 12);
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteFloat32_F4(ref NativeArray<float4> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float4* srcPtr = (float4*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float4 v = srcPtr[i];
                UnsafeUtility.MemCpy(p, &v, 16);
            }
        }

        // =========================
        // Burst kernels: Float16
        // =========================

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteFloat16_F2(ref NativeArray<float2> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float2* srcPtr = (float2*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float2 v = Sanitize(srcPtr[i]);
                uint2 h = math.f32tof16(v);
                ushort hx = (ushort)h.x, hy = (ushort)h.y;
                UnsafeUtility.MemCpy(p + 0, &hx, 2);
                UnsafeUtility.MemCpy(p + 2, &hy, 2);
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteFloat16_F3(ref NativeArray<float3> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float3* srcPtr = (float3*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float3 v = Sanitize(srcPtr[i]);
                uint3 h = math.f32tof16(v);
                ushort hx = (ushort)h.x, hy = (ushort)h.y, hz = (ushort)h.z;
                UnsafeUtility.MemCpy(p + 0, &hx, 2);
                UnsafeUtility.MemCpy(p + 2, &hy, 2);
                UnsafeUtility.MemCpy(p + 4, &hz, 2);
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteFloat16_F4(ref NativeArray<float4> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float4* srcPtr = (float4*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float4 v = Sanitize(srcPtr[i]);
                uint4 h = math.f32tof16(v);
                ushort hx = (ushort)h.x, hy = (ushort)h.y, hz = (ushort)h.z, hw = (ushort)h.w;
                UnsafeUtility.MemCpy(p + 0, &hx, 2);
                UnsafeUtility.MemCpy(p + 2, &hy, 2);
                UnsafeUtility.MemCpy(p + 4, &hz, 2);
                UnsafeUtility.MemCpy(p + 6, &hw, 2);
            }
        }

        // =========================
        // Burst kernels: SNorm (Normals/Tangents)
        // =========================

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteSNorm8_F3(ref NativeArray<float3> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float3* srcPtr = (float3*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float3 v = Sanitize(srcPtr[i]);
                p[0] = (byte)(sbyte)SNorm8(v.x);
                p[1] = (byte)(sbyte)SNorm8(v.y);
                p[2] = (byte)(sbyte)SNorm8(v.z);
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteSNorm16_F3(ref NativeArray<float3> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float3* srcPtr = (float3*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float3 v = Sanitize(srcPtr[i]);
                short x = SNorm16(v.x), y = SNorm16(v.y), z = SNorm16(v.z);
                UnsafeUtility.MemCpy(p + 0, &x, 2);
                UnsafeUtility.MemCpy(p + 2, &y, 2);
                UnsafeUtility.MemCpy(p + 4, &z, 2);
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteSNorm8_F4(ref NativeArray<float4> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float4* srcPtr = (float4*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float4 v = Sanitize(srcPtr[i]);
                p[0] = (byte)(sbyte)SNorm8(v.x);
                p[1] = (byte)(sbyte)SNorm8(v.y);
                p[2] = (byte)(sbyte)SNorm8(v.z);
                p[3] = (byte)(sbyte)SNorm8(v.w);
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteSNorm16_F4(ref NativeArray<float4> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float4* srcPtr = (float4*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float4 v = Sanitize(srcPtr[i]);
                short x = SNorm16(v.x), y = SNorm16(v.y), z = SNorm16(v.z), w = SNorm16(v.w);
                UnsafeUtility.MemCpy(p + 0, &x, 2);
                UnsafeUtility.MemCpy(p + 2, &y, 2);
                UnsafeUtility.MemCpy(p + 4, &z, 2);
                UnsafeUtility.MemCpy(p + 6, &w, 2);
            }
        }

        // =========================
        // Burst kernels: UNorm (Colors)
        // =========================

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteUNorm8_F4(ref NativeArray<float4> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float4* srcPtr = (float4*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float4 v = Sanitize(srcPtr[i]);
                p[0] = UNorm8(v.x);
                p[1] = UNorm8(v.y);
                p[2] = UNorm8(v.z);
                p[3] = UNorm8(v.w);
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteUNorm16_F4(ref NativeArray<float4> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float4* srcPtr = (float4*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float4 v = Sanitize(srcPtr[i]);
                ushort x = UNorm16(v.x), y = UNorm16(v.y), z = UNorm16(v.z), w = UNorm16(v.w);
                UnsafeUtility.MemCpy(p + 0, &x, 2);
                UnsafeUtility.MemCpy(p + 2, &y, 2);
                UnsafeUtility.MemCpy(p + 4, &z, 2);
                UnsafeUtility.MemCpy(p + 6, &w, 2);
            }
        }

        // Optional: UNorm for float3 (not used by current public API)
        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteUNorm8_F3(ref NativeArray<float3> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float3* srcPtr = (float3*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float3 v = Sanitize(srcPtr[i]);
                p[0] = UNorm8(v.x);
                p[1] = UNorm8(v.y);
                p[2] = UNorm8(v.z);
            }
        }

        [BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
        private static unsafe void WriteUNorm16_F3(ref NativeArray<float3> src, ref NativeArray<byte> dst, int n, int stride, int offset)
        {
            byte* dstBase = (byte*)dst.GetUnsafePtr() + offset;
            float3* srcPtr = (float3*)src.GetUnsafeReadOnlyPtr();
            for (int i = 0; i < n; i++)
            {
                byte* p = dstBase + (i * stride);
                float3 v = Sanitize(srcPtr[i]);
                ushort x = UNorm16(v.x), y = UNorm16(v.y), z = UNorm16(v.z);
                UnsafeUtility.MemCpy(p + 0, &x, 2);
                UnsafeUtility.MemCpy(p + 2, &y, 2);
                UnsafeUtility.MemCpy(p + 4, &z, 2);
            }
        }

        // =========================
        // Validation (Editor strict)
        // =========================

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ValidateCommonOrThrow(Mesh.MeshData meshData, VertexAttribute attr)
        {
            if (meshData.vertexCount <= 0) Throw($"[{attr}] meshData has no vertices.");
            if (!meshData.HasVertexAttribute(attr)) Throw($"[{attr}] MeshData has no such attribute.");
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void ValidateLengthOrThrow(int srcLength, int vertexCount, VertexAttribute attr)
        {
            if (srcLength != vertexCount)
                Throw($"[{attr}] src.Length must equal meshData.vertexCount (strict contract).");
        }

        // =========================
        // Helpers
        // =========================

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetFormatSizeOrThrow(VertexAttributeFormat fmt) => fmt switch
        {
            VertexAttributeFormat.Float32 or VertexAttributeFormat.UInt32 or VertexAttributeFormat.SInt32 => 4,
            VertexAttributeFormat.Float16 or VertexAttributeFormat.UNorm16 or VertexAttributeFormat.SNorm16
                or VertexAttributeFormat.UInt16 or VertexAttributeFormat.SInt16 => 2,
            VertexAttributeFormat.UNorm8 or VertexAttributeFormat.SNorm8
                or VertexAttributeFormat.UInt8 or VertexAttributeFormat.SInt8 => 1,
            _ => ThrowReturn0($"Unsupported VertexAttributeFormat: {fmt}")
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float2 Sanitize(float2 v)
        {
            v.x = math.isfinite(v.x) ? v.x : 0f;
            v.y = math.isfinite(v.y) ? v.y : 0f;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float3 Sanitize(float3 v)
        {
            v.x = math.isfinite(v.x) ? v.x : 0f;
            v.y = math.isfinite(v.y) ? v.y : 0f;
            v.z = math.isfinite(v.z) ? v.z : 0f;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float4 Sanitize(float4 v)
        {
            v.x = math.isfinite(v.x) ? v.x : 0f;
            v.y = math.isfinite(v.y) ? v.y : 0f;
            v.z = math.isfinite(v.z) ? v.z : 0f;
            v.w = math.isfinite(v.w) ? v.w : 0f;
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte UNorm8(float x)
        {
            int q = (int)math.round(math.saturate(x) * 255.0f);
            return (byte)q;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ushort UNorm16(float x)
        {
            int q = (int)math.round(math.saturate(x) * 65535.0f);
            return (ushort)q;
        }

        // SNORM: -1..1 -> [-127..127] / [-32767..32767]（-128/-32768 は使わない）
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static sbyte SNorm8(float x)
        {
            float c = math.clamp(x, -1f, 1f);
            int q = (int)math.floor(c * 127.0f + (c >= 0 ? 0.5f : -0.5f));
            q = math.clamp(q, -127, 127);
            return (sbyte)q;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short SNorm16(float x)
        {
            float c = math.clamp(x, -1f, 1f);
            int q = (int)math.floor(c * 32767.0f + (c >= 0 ? 0.5f : -0.5f));
            q = math.clamp(q, -32767, 32767);
            return (short)q;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void Throw(string message)
        {
            throw new System.ArgumentException(message);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int ThrowReturn0(string message)
        {
            Throw(message);
            return 0;
        }
    }
}

