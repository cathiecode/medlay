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
            meshBakeProcessor.BakeMeshToWorld(context.WritableMeshData, context.OriginalRenderer);
        }
    }
}
