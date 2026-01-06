namespace com.superneko.medlay.Core
{
    internal class BakeMeshEditLayer : MeshEditLayer {}

    internal class BakeMeshEditLayerProcessor : MeshEditLayerProcessor<BakeMeshEditLayer>
    {
        MeshBakeProcessor meshBakeProcessor;

        public BakeMeshEditLayerProcessor(MeshBakeProcessor meshBakeProcessor)
        {
            this.meshBakeProcessor = meshBakeProcessor;
        }

        public override void ProcessMeshEditLayer(BakeMeshEditLayer meshEditLayer, IMeshEditContext context)
        {
            meshBakeProcessor.BakeMeshToBase(context.WritableMeshData, context.OriginalRenderer, context.WorldToBaseMatrix);
        }
    }
}
