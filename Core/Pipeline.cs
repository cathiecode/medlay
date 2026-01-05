using PlasticGui;
using UnityEngine;

namespace com.superneko.medlay.Core
{
    public sealed class MedlayPipeline
    {
        Renderer originalRenderer;
        Mesh deformedMesh;

        (MeshEditLayer, IMeshEditLayerProcessor)[] meshEditLayers;

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

            this.meshEditLayers = new (MeshEditLayer, IMeshEditLayerProcessor)[meshEditLayers.Length + 2];
            this.meshEditLayers[0] = (new BakeMeshEditLayer(), new BakeMeshEditLayerProcessor(meshBakeProcessor));

            for (int i = 0; i < meshEditLayers.Length; i++)
            {
                this.meshEditLayers[i + 1] = meshEditLayers[i];
            }

            this.meshEditLayers[this.meshEditLayers.Length - 1] = (new MeshUnbakeMeshEditLayer(), new MeshUnbakeMeshEditLayerProcessor(meshBakeProcessor));
        }

        public void Process()
        {
            var context = MeshEditContext.FromRenderer(originalRenderer, deformedMesh);

            foreach (var (meshEditLayer, processor) in meshEditLayers)
            {
                processor.ProcessMeshEditLayer(meshEditLayer, context);
            }

            context.WritebackIfNeed();
        }

        public Mesh GetDeformedMesh()
        {
            return deformedMesh;
        }
    }
}
