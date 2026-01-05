namespace com.superneko.medlay.Core
{
    internal class MeshUnbakeMeshEditLayer : MeshEditLayer {}

    internal class MeshUnbakeMeshEditLayerProcessor : MeshEditLayerProcessor<MeshUnbakeMeshEditLayer>
    {
        MeshBakeProcessor meshBakeProcessor;

        public MeshUnbakeMeshEditLayerProcessor(MeshBakeProcessor meshBakeProcessor)
        {
            this.meshBakeProcessor = meshBakeProcessor;
        }

        public override void ProcessMeshEditLayer(MeshUnbakeMeshEditLayer meshEditLayer, IMeshEditContext context)
        {
            meshBakeProcessor.UnBakeMeshFromWorld(context.WritableMeshData, context.OriginalRenderer);
        }
    }
}
