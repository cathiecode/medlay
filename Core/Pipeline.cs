using System.Collections.Generic;
using System.Linq;
using PlasticGui;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace com.superneko.medlay.Core
{
    public sealed class MedlayPipeline
    {
        Renderer originalRenderer;
        Mesh deformedMesh;

        (MeshEditLayer, IMeshEditLayerProcessor)[] meshEditLayers;
        BakeMeshEditLayer bakeLayer = new BakeMeshEditLayer();
        MeshUnbakeMeshEditLayer unbakeLayer = new MeshUnbakeMeshEditLayer();
        BakeMeshEditLayerProcessor bakeProcessor;
        MeshUnbakeMeshEditLayerProcessor unbakeProcessor;
        Medlay registry;

        public MedlayPipeline(Renderer renderer, (MeshEditLayer, IMeshEditLayerProcessor)[] meshEditLayers, Medlay medlay)
        {
            this.originalRenderer = renderer;

            var originalMesh = renderer switch
            {
                SkinnedMeshRenderer smr => smr.sharedMesh,
                MeshRenderer mr => mr.GetComponent<MeshFilter>().sharedMesh,
                _ => throw new System.Exception("Unsupported renderer type: " + renderer.GetType().Name)
            };

            var meshBakeProcessor = new MeshBakeProcessor();

            deformedMesh = Object.Instantiate(originalMesh);
            deformedMesh.name = "MedlayDeformedMesh";

            this.meshEditLayers = meshEditLayers;

            this.bakeProcessor = new BakeMeshEditLayerProcessor(meshBakeProcessor);
            this.unbakeProcessor = new MeshUnbakeMeshEditLayerProcessor(meshBakeProcessor);

            this.registry = medlay;
        }

        public void Process()
        {
            var context = MeshEditContext.FromRenderer(originalRenderer, deformedMesh);

            bakeProcessor.ProcessMeshEditLayer(bakeLayer, context);

            foreach (var (meshEditLayer, processor) in meshEditLayers)
            {
                processor.ProcessMeshEditLayer(meshEditLayer, context);
            }

            unbakeProcessor.ProcessMeshEditLayer(unbakeLayer, context);

            context.WritebackIfNeed();
        }

        public Mesh GetDeformedMesh()
        {
            return deformedMesh;
        }

        public void Refresh(ICollection<MeshEditLayer> newMeshEditLayers)
        {
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
