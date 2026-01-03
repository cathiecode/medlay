using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using static UnityEngine.Mesh;

namespace com.superneko.medlay.Editor
{
    internal class MedlayMeshUtils
    {
        public static MeshDataArray CreateWritableMeshData(Mesh mesh)
        {
            Profiler.BeginSample("CreateWritableMeshData");

            Profiler.BeginSample("Acquire Buffers");

            // Setup MeshData
            using var meshDataInputArray = Mesh.AcquireReadOnlyMeshData(mesh);
            var meshDataInput = meshDataInputArray[0];

            var meshDataOutputArray = Mesh.AllocateWritableMeshData(1);
            var meshDataOutput = meshDataOutputArray[0];

            Profiler.EndSample();

            // Gather stream info
            Profiler.BeginSample("Gather Stream Info");
            var attributes = mesh.GetVertexAttributes();

            int maxStream = 0;

            foreach (var attribute in attributes)
            {
                maxStream = Mathf.Max(attribute.stream, maxStream);
                // Debug.Log($"[MeshDataTest] {attribute}");
            }

            // TODO: Attribute format compatibility check

            // Debug.Log($"[MeshDataTest] maxStream: {maxStream}");

            Profiler.EndSample();

            // Setup vertex buffer
            Profiler.BeginSample("Setup Vertex Buffer");

            meshDataOutput.SetVertexBufferParams(mesh.vertexCount, attributes);

            for (int i = 0; i <= maxStream; i++)
            {
                var inputVertexData = meshDataInput.GetVertexData<byte>(i);
                var outputVertexData = meshDataOutput.GetVertexData<byte>(i);
                Profiler.BeginSample($"Copy Vertex Stream {i}");
                inputVertexData.CopyTo(outputVertexData);
                Profiler.EndSample();
                // Debug.Log($"[MeshDataTest] Copied vertex stream {i}, size: {inputVertexData.Length} bytes");
            }

            Profiler.EndSample();

            Profiler.BeginSample("Setup Index Buffer");

            // Setup index buffer
            var inputIndexData = meshDataInput.GetIndexData<byte>();

            var indexLength = mesh.indexFormat == IndexFormat.UInt16 ? 2 : 4;

            meshDataOutput.SetIndexBufferParams(inputIndexData.Length / indexLength, mesh.indexFormat);

            var outputIndexData = meshDataOutput.GetIndexData<byte>();
            Profiler.BeginSample("Copy Index Buffer");
            inputIndexData.CopyTo(outputIndexData);
            Profiler.EndSample();

            Profiler.EndSample();

            // Setup submesh
            Profiler.BeginSample("Setup SubMeshes");
            meshDataOutput.subMeshCount = meshDataInput.subMeshCount;

            for (var i = 0; i < mesh.subMeshCount; i++)
            {
                var desc = meshDataInput.GetSubMesh(i);
                meshDataOutput.SetSubMesh(i, desc, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds); // NOTE: We may need to recalculate bounds later
            }
            Profiler.EndSample();

            Profiler.EndSample();

            return meshDataOutputArray;
        }
    }
}
