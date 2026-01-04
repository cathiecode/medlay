namespace com.superneko.medlay.Core
{
    internal class MeshUnbakeMeshEditLayer : MeshEditLayer {}

    internal class MeshUnbakeMeshEditLayerProcessor : MeshEditLayerProcessor<MeshUnbakeMeshEditLayer>
    {
        MeshBakeProcessor meshBakeProcessor = new MeshBakeProcessor();

        public override void ProcessMeshEditLayer(MeshUnbakeMeshEditLayer meshEditLayer, IMeshEditContext context)
        {
            meshBakeProcessor.UnBakeMeshFromWorld(context.WritableMeshData, context.OriginalRenderer);
        }
    }
}
