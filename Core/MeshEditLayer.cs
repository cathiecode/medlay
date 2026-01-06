namespace com.superneko.medlay.Core
{
    [System.Serializable]
    public abstract class MeshEditLayer
    {
        public long Id { get; protected set; } = LongId.Generate();
    }
}
