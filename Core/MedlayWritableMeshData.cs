
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using static UnityEngine.Mesh;
using UnityEngine.Profiling;

namespace com.superneko.medlay.Core
{
    using Internal.Unsafe;
    public sealed class MedlayWritableMeshData : System.IDisposable
    {
        Mesh baseMesh;
        MeshDataArray meshDataArray;

        NativeArray<float3> vertices;
        NativeArray<float3> normals;
        NativeArray<float4> tangents;
        NativeArray<int> boneWeightStartIndexPerVertex;

        Allocator allocator;

        MeshData meshData => meshDataArray[0];

        public int vertexCount => meshData.vertexCount;
        public int bindposeCount => baseMesh.bindposes.Length;

        /// <summary>
        /// The original Mesh this MeshData is based on.
        /// Modifications to the BaseMesh will be REVERTED on Writeback.
        /// </summary>
        public Mesh BaseMesh => baseMesh;

        /// <summary>
        /// The underlying MeshData.
        /// Modifications to the MeshData will be REVERTED on Writeback.
        /// </summary>
        public MeshData MeshData => meshData;

        public NativeArray<float3> GetVertices()
        {
            if (vertices.IsCreated)
            {
                return vertices;
            }

            Profiler.BeginSample("MedlayWritableMeshData.GetVertices");

            vertices = new NativeArray<float3>(meshData.vertexCount, allocator);

            meshData.GetVertices(MeshDataUtils.Reinterpret(vertices));

            Profiler.EndSample();

            return vertices;
        }

        public NativeArray<float3> GetNormals()
        {
            if (normals.IsCreated)
            {
                return normals;
            }

            Profiler.BeginSample("MedlayWritableMeshData.GetNormals");

            normals = new NativeArray<float3>(meshData.vertexCount, allocator);

            meshData.GetNormals(MeshDataUtils.Reinterpret(normals));

            Profiler.EndSample();

            return normals;
        }

        public NativeArray<float4> GetTangents()
        {
            if (tangents.IsCreated)
            {
                return tangents;
            }

            Profiler.BeginSample("MedlayWritableMeshData.GetTangents");

            tangents = new NativeArray<float4>(meshData.vertexCount, allocator);

            meshData.GetTangents(MeshDataUtils.Reinterpret(tangents));

            Profiler.EndSample();

            return tangents;
        }

        public NativeArray<BoneWeight1> GetAllBoneWeightsReadOnly()
        {
            return baseMesh.GetAllBoneWeights();
        }

        public NativeArray<byte> GetBonesPerVertexReadOnly()
        {
            return baseMesh.GetBonesPerVertex();
        }

        public NativeArray<int> GetBoneWeightStartIndexPerVertexReadOnly()
        {
            if (boneWeightStartIndexPerVertex.IsCreated)
            {
                return boneWeightStartIndexPerVertex;
            }

            Profiler.BeginSample("MedlayWritableMeshData.GetBoneWeightStartIndexPerVertex");

            var bonesPerVertex = GetBonesPerVertexReadOnly();

            boneWeightStartIndexPerVertex = new NativeArray<int>(bonesPerVertex.Length, allocator);

            int i = 0;

            for (int v = 0; v < bonesPerVertex.Length; v++)
            {
                boneWeightStartIndexPerVertex[v] = i;

                i += bonesPerVertex[v];
            }

            Profiler.EndSample();

            return boneWeightStartIndexPerVertex;
        }

        public NativeArray<float4x4> GetBindposesReadOnly()
        {
            return MeshDataUtils.Reinterpret(baseMesh.GetBindposes());
        }

        void Writeback()
        {
            Profiler.BeginSample("MedlayWritableMeshData.Writeback");

            if (vertices.IsCreated)
            {
                MeshDataAttributeWriter.SetVertices(ref vertices, meshData);
            }

            if (normals.IsCreated)
            {
                MeshDataAttributeWriter.SetNormals(ref normals, meshData);
            }

            if (tangents.IsCreated)
            {
                MeshDataAttributeWriter.SetTangents(ref tangents, meshData);
            }

            Profiler.EndSample();
        }

        public void Dispose()
        {
            if (vertices.IsCreated)
            {
                vertices.Dispose();
            }

            if (normals.IsCreated)
            {
                normals.Dispose();
            }

            if (tangents.IsCreated)
            {
                tangents.Dispose();
            }

            if (boneWeightStartIndexPerVertex.IsCreated)
            {
                boneWeightStartIndexPerVertex.Dispose();
            }
        }

        public static MedlayWritableMeshData Create(Mesh mesh, Allocator allocator)
        {
            Profiler.BeginSample("MedlayWritableMeshData.Create");

            var meshDataArray = MedlayMeshUtils.CreateWritableMeshData(mesh);

            Profiler.EndSample();

            return new MedlayWritableMeshData()
            {
                meshDataArray = meshDataArray,
                allocator = allocator,
                baseMesh = mesh
            };
        }

        public static void WritebackAndDispose(MedlayWritableMeshData writableMeshData, Mesh mesh)
        {
            Profiler.BeginSample("MedlayWritableMeshData.WritebackStatic");

            var bindposes = writableMeshData.baseMesh.bindposes;

            writableMeshData.Writeback();
            ApplyAndDisposeWritableMeshData(writableMeshData.meshDataArray, mesh, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            writableMeshData.Dispose();

            // Restore bindposes
            mesh.bindposes = bindposes;

            // TODO: BlendShapes

            Profiler.EndSample();
        }
    }
}
