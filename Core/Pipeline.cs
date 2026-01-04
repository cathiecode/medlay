using UnityEngine;

namespace com.superneko.medlay.Core
{
    public sealed class Pipeline
    {
        Renderer originalRenderer;
        Mesh deformedMesh;

        (MeshEditLayer, IMeshEditLayerProcessor)[] meshEditLayers;

        public Pipeline(Renderer renderer, (MeshEditLayer, IMeshEditLayerProcessor)[] meshEditLayers)
        {
            this.originalRenderer = renderer;            

            deformedMesh = new Mesh();
            deformedMesh.name = "MedlayDeformedMesh";

            this.meshEditLayers = new (MeshEditLayer, IMeshEditLayerProcessor)[meshEditLayers.Length + 2];
            this.meshEditLayers[0] = (new BakeMeshEditLayer(), new BakeMeshEditLayerProcessor());

            for (int i = 0; i < meshEditLayers.Length; i++)
            {
                this.meshEditLayers[i + 1] = meshEditLayers[i];
            }

            this.meshEditLayers[this.meshEditLayers.Length - 1] = (new MeshUnbakeMeshEditLayer(), new MeshUnbakeMeshEditLayerProcessor());
        }

        public void Process()
        {
            var context = MeshEditContext.FromRenderer(originalRenderer);

            foreach (var (meshEditLayer, processor) in meshEditLayers)
            {
                processor.ProcessMeshEditLayer(meshEditLayer, context);
            }

            MedlayWritableMeshData.Writeback(context.WritableMeshData, deformedMesh);
        }

        public Mesh GetDeformedMesh()
        {
            return deformedMesh;
        }
    }
}
 