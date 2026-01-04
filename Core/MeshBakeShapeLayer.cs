namespace com.superneko.medlay.Core
{
    internal class BakeMeshEditLayer : MeshEditLayer {}

    internal class BakeMeshEditLayerProcessor : MeshEditLayerProcessor<BakeMeshEditLayer>
    {
        MeshBakeProcessor meshBakeProcessor = new MeshBakeProcessor();

        public override void ProcessMeshEditLayer(BakeMeshEditLayer meshEditLayer, IMeshEditContext context)
        {
            meshBakeProcessor.BakeMeshToWorld(context.WritableMeshData, context.OriginalRenderer);
        }
    }
}
