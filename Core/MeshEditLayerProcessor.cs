namespace com.superneko.medlay.Core
{
    public abstract class MeshEditLayerProcessor<T> : IMeshEditLayerProcessor where T : MeshEditLayer
    {
        public abstract void ProcessMeshEditLayer(T meshEditLayer, IMeshEditContext context);

        void IMeshEditLayerProcessor.ProcessMeshEditLayer(MeshEditLayer meshEditLayer, IMeshEditContext context)
        {
            ProcessMeshEditLayer((T)meshEditLayer, context);
        }
    }
}
