using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

namespace com.superneko.medlay.Core
{
    public sealed class MedlayPipeline : System.IDisposable
    {
        Renderer originalRenderer;
        Mesh deformedMesh;

        (MeshEditLayer, IMeshEditLayerProcessor)[] meshEditLayers;
        BakeMeshEditLayer bakeLayer = new BakeMeshEditLayer();
        MeshUnbakeMeshEditLayer unbakeLayer = new MeshUnbakeMeshEditLayer();
        BakeMeshEditLayerProcessor bakeProcessor;
        MeshUnbakeMeshEditLayerProcessor unbakeProcessor;
        Medlay registry;
        Matrix4x4 worldToBaseMatrix;
        MeshBakeProcessor meshBakeProcessor;

        public MedlayPipeline(Renderer renderer, (MeshEditLayer, IMeshEditLayerProcessor)[] meshEditLayers, Medlay medlay, Matrix4x4 worldToBaseMatrix)
        {
            this.originalRenderer = renderer;
            this.worldToBaseMatrix = worldToBaseMatrix;

            var meshBakeProcessor = new MeshBakeProcessor();

            this.meshEditLayers = meshEditLayers;

            this.bakeProcessor = new BakeMeshEditLayerProcessor(meshBakeProcessor);
            this.unbakeProcessor = new MeshUnbakeMeshEditLayerProcessor(meshBakeProcessor);

            this.registry = medlay;
            this.meshBakeProcessor = meshBakeProcessor;
        }

        public void Dispose()
        {
            if (deformedMesh != null)
            {
                Object.DestroyImmediate(deformedMesh);
                deformedMesh = null;
            }
            
            if (meshBakeProcessor != null)
            {
                meshBakeProcessor = null;
            }
        }

        public MedlayProcessResult Process()
        {
            Profiler.BeginSample("MedlayPipeline.Process");

            // TODO: Optimize
            if (deformedMesh == null)
            {
                var originalMesh = originalRenderer switch
                {
                    SkinnedMeshRenderer smr => smr.sharedMesh,
                    MeshRenderer mr => mr.GetComponent<MeshFilter>().sharedMesh,
                    _ => throw new System.Exception("Unsupported renderer type: " + originalRenderer.GetType().Name)
                };

                deformedMesh = Object.Instantiate(originalMesh);
                deformedMesh.name = "MedlayDeformedMesh";
            }

            var context = MeshEditContext.FromRenderer(originalRenderer, deformedMesh, worldToBaseMatrix);

            bakeProcessor.ProcessMeshEditLayer(bakeLayer, context);

            foreach (var (meshEditLayer, processor) in meshEditLayers)
            {
                Profiler.BeginSample("MedlayPipeline.Process - " + meshEditLayer.GetType().Name);

                processor.ProcessMeshEditLayer(meshEditLayer, context);

                Profiler.EndSample();
            }

            unbakeProcessor.ProcessMeshEditLayer(unbakeLayer, context);

            context.WritebackIfNeed();
            Profiler.EndSample();

            return new MedlayProcessResult(context);
        }

        public void Refresh(ICollection<MeshEditLayer> newMeshEditLayers, Matrix4x4 newWorldToBaseMatrix)
        {
            this.worldToBaseMatrix = newWorldToBaseMatrix;

            (MeshEditLayer, IMeshEditLayerProcessor)[] updatedLayers = meshEditLayers;

            if (updatedLayers.Length != newMeshEditLayers.Count)
            {
                updatedLayers = new (MeshEditLayer, IMeshEditLayerProcessor)[newMeshEditLayers.Count];
            }

            for (int i = 0; i < updatedLayers.Length; i++)
            {
                var newLayer = newMeshEditLayers.ElementAt(i);
                var (oldLayer, oldProcessor) = updatedLayers[i];

                if (oldLayer == newLayer)
                {
                    updatedLayers[i] = (newLayer, oldProcessor);
                    // Processor reusability query
                }
                else
                {
                    registry.GetProcessorForMeshEditLayer(newLayer, out var processor);

                    updatedLayers[i] = (newLayer, processor);
                }
            }

            meshEditLayers = updatedLayers;
        }
    }
}
